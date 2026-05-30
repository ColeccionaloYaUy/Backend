using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Carts;
using ColeccionaloYa.Domain.Carts.Exceptions;
using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDetailDto> {
	private readonly ICConnection _Connection;
	private readonly ICartRepository _CartRepository;
	private readonly IOrderRepository _OrderRepository;
	private readonly IStockRepository _StockRepository;
	private readonly ICouponRepository _CouponRepository;

	public CreateOrderCommandHandler(
		ICConnection connection,
		ICartRepository cartRepository,
		IOrderRepository orderRepository,
		IStockRepository stockRepository,
		ICouponRepository couponRepository) {
		_Connection = connection;
		_CartRepository = cartRepository;
		_OrderRepository = orderRepository;
		_StockRepository = stockRepository;
		_CouponRepository = couponRepository;
	}

	public async Task<OrderDetailDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken) {
		// Address ownership validation.
		if (!await _OrderRepository.AddressBelongsToClientAsync(request.DeliveryAddressId, request.ClientId, cancellationToken)
			|| !await _OrderRepository.AddressBelongsToClientAsync(request.OrderAddressId, request.ClientId, cancellationToken)) {
			throw new InvalidOrderAddressException();
		}

		// The checkout always reads the persistent cart.
		var cart = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		if (cart is null || cart.Items.Count == 0) {
			throw new CartEmptyException();
		}

		// Stock validation against current availability.
		var currentStockByProduct = new Dictionary<int, int>();
		foreach (var item in cart.Items) {
			var currentStock = await _StockRepository.GetCurrentStockAsync(item.IdProduct, cancellationToken);
			currentStockByProduct[item.IdProduct] = currentStock;
			if (item.Quantity > currentStock) {
				throw new InsufficientStockException(item.IdProduct);
			}
		}

		// Totals.
		var subtotal = cart.Items.Sum(i => Math.Round(i.DiscountedPrice * i.Quantity, 2));

		int? couponId = null;
		var couponDiscount = 0m;
		if (cart.IdCoupon.HasValue) {
			var coupon = await _CouponRepository.GetByIdAsync(cart.IdCoupon.Value, cancellationToken);
			var now = DateTime.UtcNow;
			if (coupon is not null
				&& coupon.IsActive
				&& coupon.IsWithinWindow(now)
				&& await _CouponRepository.IsAssignedAsync(coupon.Id, request.ClientId, cancellationToken)) {
				couponId = coupon.Id;
				couponDiscount = Math.Round(subtotal * coupon.Porcentage / 100m, 2);
			}
		}

		var taxes = Math.Round((subtotal - couponDiscount) * CartTax.Rate, 2);
		var total = subtotal - couponDiscount + taxes;

		var lines = cart.Items
			.Select(i => OrderLine.Create(i.IdProduct, i.Quantity, i.DiscountedPrice))
			.ToList();

		var order = Order.Create(request.ClientId, couponId, lines, subtotal, taxes, total, request.Observations);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			var deliverySnapshotId = await _OrderRepository.CreateAddressSnapshotAsync(request.DeliveryAddressId, cancellationToken);
			var orderSnapshotId = await _OrderRepository.CreateAddressSnapshotAsync(request.OrderAddressId, cancellationToken);
			order.AssignSnapshots(deliverySnapshotId, orderSnapshotId);

			await _OrderRepository.CreateOrderAsync(order, cancellationToken);
			await _OrderRepository.CreateOrderLinesAsync(order.Id, lines, cancellationToken);

			foreach (var line in lines) {
				var movement = StockMovement.Create(
					line.ProductId, StockMovementType.Exit, line.Quantity, DateTime.UtcNow, order.Id, $"Order #{order.Id}");
				movement.EnsureDoesNotCauseNegativeStock(currentStockByProduct[line.ProductId]);
				await _StockRepository.CreateAsync(movement, cancellationToken);
			}

			await _OrderRepository.AddStatusHistoryAsync(order.Id, OrderStatus.Pending, null, request.ClientId, cancellationToken);

			await _CartRepository.ClearItemsAsync(cart.IdCart, cancellationToken);

			await _Connection.CommitTransaction(cancellationToken);
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}

		var detail = await _OrderRepository.GetDetailAsync(order.Id, cancellationToken)
			?? throw new OrderNotFoundException(order.Id);
		return OrderDetailDto.From(detail);
	}
}

using System.Globalization;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Orders.ReadModels;
using MediatR;

namespace ColeccionaloYa.Application.Orders.Invoice;

public class GetOrderInvoiceQueryHandler : IRequestHandler<GetOrderInvoiceQuery, InvoiceFile> {
	private static readonly string[] NonInvoiceableStatuses = { "pending", "cancelled" };

	private readonly IOrderRepository _Repository;

	public GetOrderInvoiceQueryHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<InvoiceFile> Handle(GetOrderInvoiceQuery request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_Repository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var data = await _Repository.GetDetailAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		if (NonInvoiceableStatuses.Contains(data.Order.Status)) {
			throw new OrderInvoiceNotAvailableException(request.OrderId);
		}

		var pdf = SimplePdf.FromLines(BuildLines(data));
		return new InvoiceFile(pdf, $"invoice-{request.OrderId}.pdf");
	}

	private static List<string> BuildLines(OrderDetailData data) {
		string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

		var lines = new List<string> {
			"ColeccionaloYa - Invoice",
			$"Order #{data.Order.IdOrder}    Date: {data.Order.CreationDate:yyyy-MM-dd}",
			$"Status: {data.Order.Status}",
			$"Customer: {data.Client.Name} {data.Client.Lastname} <{data.Client.Email}>",
		};

		if (data.Delivery is not null) {
			lines.Add($"Delivery: {data.Delivery.Street} {data.Delivery.DoorNumber}, {data.Delivery.Neighborhood}, {data.Delivery.State}");
		}

		lines.Add("");
		lines.Add("Items:");
		foreach (var line in data.Lines) {
			lines.Add($"  - {line.ProductName} x{line.Quantity}  @ {Money(line.Price)}  = {Money(line.LineTotal)}");
		}

		lines.Add("");
		lines.Add($"Subtotal: {Money(data.Order.Subtotal)} UYU");
		lines.Add($"Taxes: {Money(data.Order.Taxes)} UYU");
		lines.Add($"Total: {Money(data.Order.Total)} UYU");

		return lines;
	}
}

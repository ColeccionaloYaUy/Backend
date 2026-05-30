using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.GetPaymentById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDetailDto> {
	private readonly IPaymentRepository _Repository;

	public GetPaymentByIdQueryHandler(IPaymentRepository repository) {
		_Repository = repository;
	}

	public async Task<PaymentDetailDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetDetailAsync(request.IdPayment, cancellationToken)
			?? throw new PaymentNotFoundException(request.IdPayment);
		return PaymentDetailDto.From(data);
	}
}

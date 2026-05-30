using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Payments.ReadModels;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Payments.Interfaces;

public interface IPaymentRepository {
	Task<PagedData<PaymentListItem>> GetPagedAsync(int page, int pageSize, string? status, int? orderId, int? clientId, string? provider, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken);
	Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<PaymentDetailData?> GetDetailAsync(int id, CancellationToken cancellationToken);
	Task<List<Payment>> GetByOrderAsync(int orderId, CancellationToken cancellationToken);
	Task<Payment?> GetLastByOrderAsync(int orderId, CancellationToken cancellationToken);
	Task<Payment?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
	Task<bool> HasApprovedAsync(int orderId, CancellationToken cancellationToken);
	Task CreateAsync(Payment payment, CancellationToken cancellationToken);
	Task UpdateAsync(Payment payment, CancellationToken cancellationToken);
}

using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using ColeccionaloYa.Persistence.Payments.ReadModels;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Payments.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class PaymentRepository : IPaymentRepository {
	private readonly ICConnection _Connection;

	public PaymentRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Payment MapPayment(ICDataReader rs) {
		return new Payment {
			Id = rs.GetValue<int>("id_payment"),
			OrderId = rs.GetValue<int>("id_order"),
			ExternalId = rs.GetValue<string?>("external_id"),
			PreferenceId = rs.GetValue<string?>("preference_id"),
			Status = PaymentStatusDb.FromDb(rs.GetValue<string>("status")),
			Amount = rs.GetValue<decimal>("amount"),
			Currency = rs.GetValue<string>("currency"),
			PaymentMethod = rs.GetValue<string?>("payment_method"),
			Provider = rs.GetValue<string?>("provider"),
			CreationDate = rs.GetValue<DateTime>("creation_date"),
			UpdatedDate = rs.GetValue<DateTime>("updated_date"),
			RawResponse = rs.GetValue<string?>("raw_response"),
		};
	}

	public async Task<PagedData<PaymentListItem>> GetPagedAsync(int page, int pageSize, string? status, int? orderId, int? clientId, string? provider, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   pa.id_payment, pa.id_order, pa.external_id, pa.preference_id, pa.status::text AS status,
                   pa.amount, pa.currency, pa.payment_method, pa.provider, pa.creation_date, pa.updated_date, pa.raw_response,
                   cl.email AS client_email
            FROM payment pa
            INNER JOIN ""order"" o ON o.id_order = pa.id_order
            INNER JOIN client cl ON cl.id_client = o.id_client
            WHERE (@status IS NULL OR pa.status = @status::payment_status)
              AND (@orderId IS NULL OR pa.id_order = @orderId)
              AND (@clientId IS NULL OR o.id_client = @clientId)
              AND (@provider IS NULL OR pa.provider = @provider)
              AND (@dateFrom IS NULL OR pa.creation_date >= @dateFrom)
              AND (@dateTo IS NULL OR pa.creation_date < (@dateTo::date + INTERVAL '1 day'))
            ORDER BY pa.id_payment DESC";

		return await PaginationHelper.FetchPagedAsync<PaymentListItem>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("status", (object?)status ?? DBNull.Value);
				cmd.AddParameter("orderId", (object?)orderId ?? DBNull.Value);
				cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
				cmd.AddParameter("provider", (object?)provider ?? DBNull.Value);
				cmd.AddParameter("dateFrom", (object?)dateFrom ?? DBNull.Value);
				cmd.AddParameter("dateTo", (object?)dateTo ?? DBNull.Value);
			},
			rs => new PaymentListItem(MapPayment(rs), rs.GetValue<string>("client_email")),
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = BaseSelect() + " WHERE pa.id_payment = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Payment>(MapPayment, cancellationToken);
	}

	public async Task<PaymentDetailData?> GetDetailAsync(int id, CancellationToken cancellationToken) {
		var payment = await GetByIdAsync(id, cancellationToken);
		if (payment is null) {
			return null;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT o.id_order, o.creation_date, o.status::text AS status, COALESCE(o.total, 0) AS total,
                   o.id_client, cl.name || ' ' || cl.lastname AS client_name, cl.email AS client_email
            FROM ""order"" o
            INNER JOIN client cl ON cl.id_client = o.id_client
            WHERE o.id_order = @orderId";
		cmd.AddParameter("orderId", payment.OrderId);
		var order = await cmd.ExecuteSelect(rs => new PaymentOrderBrief(
			rs.GetValue<int>("id_order"),
			rs.GetValue<DateOnly>("creation_date"),
			rs.GetValue<string>("status"),
			rs.GetValue<decimal>("total"),
			rs.GetValue<int>("id_client"),
			rs.GetValue<string>("client_name"),
			rs.GetValue<string>("client_email")), cancellationToken);

		return order is null ? null : new PaymentDetailData(payment, order);
	}

	public async Task<List<Payment>> GetByOrderAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = BaseSelect() + " WHERE pa.id_order = @orderId ORDER BY pa.id_payment DESC";
		cmd.AddParameter("orderId", orderId);
		return await cmd.ExecuteSelectList<Payment>(MapPayment, cancellationToken);
	}

	public async Task<Payment?> GetLastByOrderAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = BaseSelect() + " WHERE pa.id_order = @orderId ORDER BY pa.id_payment DESC LIMIT 1";
		cmd.AddParameter("orderId", orderId);
		return await cmd.ExecuteSelect<Payment>(MapPayment, cancellationToken);
	}

	public async Task<Payment?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = BaseSelect() + " WHERE pa.external_id = @externalId ORDER BY pa.id_payment DESC LIMIT 1";
		cmd.AddParameter("externalId", externalId);
		return await cmd.ExecuteSelect<Payment>(MapPayment, cancellationToken);
	}

	public async Task<bool> HasApprovedAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM payment WHERE id_order = @orderId AND status = 'approved'::payment_status";
		cmd.AddParameter("orderId", orderId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Payment payment, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO payment (id_order, external_id, preference_id, status, amount, currency, payment_method, provider, creation_date, updated_date, raw_response)
            VALUES (@orderId, @externalId, @preferenceId, @status::payment_status, @amount, @currency, @paymentMethod, @provider, @creationDate, @updatedDate, @rawResponse)
            RETURNING id_payment";
		cmd.AddParameter("orderId", payment.OrderId);
		cmd.AddParameter("externalId", (object?)payment.ExternalId ?? DBNull.Value);
		cmd.AddParameter("preferenceId", (object?)payment.PreferenceId ?? DBNull.Value);
		cmd.AddParameter("status", PaymentStatusDb.ToDb(payment.Status));
		cmd.AddParameter("amount", payment.Amount);
		cmd.AddParameter("currency", payment.Currency);
		cmd.AddParameter("paymentMethod", (object?)payment.PaymentMethod ?? DBNull.Value);
		cmd.AddParameter("provider", (object?)payment.Provider ?? DBNull.Value);
		cmd.AddParameter("creationDate", payment.CreationDate);
		cmd.AddParameter("updatedDate", payment.UpdatedDate);
		cmd.AddParameter("rawResponse", (object?)payment.RawResponse ?? DBNull.Value);

		var newId = await cmd.ExecuteGetValue<int>("id_payment", cancellationToken);
		payment.AssignId(newId);
	}

	public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE payment
            SET external_id = @externalId,
                preference_id = @preferenceId,
                status = @status::payment_status,
                payment_method = @paymentMethod,
                provider = @provider,
                updated_date = @updatedDate,
                raw_response = @rawResponse
            WHERE id_payment = @id";
		cmd.AddParameter("id", payment.Id);
		cmd.AddParameter("externalId", (object?)payment.ExternalId ?? DBNull.Value);
		cmd.AddParameter("preferenceId", (object?)payment.PreferenceId ?? DBNull.Value);
		cmd.AddParameter("status", PaymentStatusDb.ToDb(payment.Status));
		cmd.AddParameter("paymentMethod", (object?)payment.PaymentMethod ?? DBNull.Value);
		cmd.AddParameter("provider", (object?)payment.Provider ?? DBNull.Value);
		cmd.AddParameter("updatedDate", payment.UpdatedDate);
		cmd.AddParameter("rawResponse", (object?)payment.RawResponse ?? DBNull.Value);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	private static string BaseSelect() => @"
        SELECT pa.id_payment, pa.id_order, pa.external_id, pa.preference_id, pa.status::text AS status,
               pa.amount, pa.currency, pa.payment_method, pa.provider, pa.creation_date, pa.updated_date, pa.raw_response
        FROM payment pa";
}

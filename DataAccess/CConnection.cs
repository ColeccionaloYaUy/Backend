using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace ColeccionaloYa.DataAccess;

[Injectable(ServiceLifetime.Scoped)]
public class CConnection : ICConnection {
	public CConnection(IConfiguration configuration) {
		var connectionString = configuration.GetConnectionString("value");
		if (string.IsNullOrEmpty(connectionString)) {
			throw new InvalidOperationException("Connection string 'value' is not configured.");
		}

		var builder = new NpgsqlConnectionStringBuilder(connectionString) {
			Timeout = 30,
			TrustServerCertificate = true,
		};

		ConnectionDB = new NpgsqlConnection(builder.ConnectionString);
	}

	internal NpgsqlConnection ConnectionDB { get; set; }
	internal NpgsqlTransaction? Transaction { get; set; }

	public async Task Connect(CancellationToken cancellationToken = default) {
		if (ConnectionDB.State == ConnectionState.Closed) {
			using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
			await ConnectionDB.OpenAsync(linkedCts.Token);
		}
	}

	public async Task Disconnect() {
		if (ConnectionDB.State == ConnectionState.Open) {
			await ConnectionDB.CloseAsync();
		}
	}

	public async Task BeginTransaction(CancellationToken cancellationToken = default) {
		Transaction = await ConnectionDB.BeginTransactionAsync(cancellationToken);
	}

	public async Task CommitTransaction(CancellationToken cancellationToken = default) {
		if (Transaction != null) {
			await Transaction.CommitAsync(cancellationToken);
			Transaction = null;
		}
	}

	public async Task CancelTransaction(CancellationToken cancellationToken = default) {
		if (Transaction != null) {
			await Transaction.RollbackAsync(cancellationToken);
			Transaction = null;
		}
	}

	public ICCommand CreateCommand() {
		return new CCommand(this);
	}
}

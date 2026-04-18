using API.DataAccess.Interfaces;
using API.Utils.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace API.DataAccess;

[Injectable(ServiceLifetime.Scoped)]
public class CConnection : ICConnection {
    public CConnection(IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("value");
        if (string.IsNullOrEmpty(connectionString)) {
            throw new NullReferenceException();
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString) {
            Timeout = 30,
            TrustServerCertificate = true,
        };

        ConnectionDB = new NpgsqlConnection(builder.ConnectionString);
    }

    internal NpgsqlConnection? ConnectionDB { get; set; }
    internal NpgsqlTransaction? Transaction { get; set; }

    public async Task Connect() {
        if (ConnectionDB == null) {
            throw new NullReferenceException("No Connection assigned");
        }

        if (ConnectionDB.State == ConnectionState.Closed) {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await ConnectionDB.OpenAsync(cts.Token);
        }
    }

    public async Task Disconnect() {
        if (ConnectionDB == null) {
            throw new NullReferenceException("No Connection assigned");
        }

        if (ConnectionDB.State == ConnectionState.Open) {
            await ConnectionDB.CloseAsync();
        }
    }

    public async Task BeginTransaction() {
        Transaction = await ConnectionDB!.BeginTransactionAsync();
    }

    public async Task CommitTransaction() {
        if (Transaction != null) {
            await Transaction.CommitAsync();
            Transaction = null;
        }
    }

    public async Task CancelTransaction() {
        if (Transaction != null) {
            await Transaction.RollbackAsync();
            Transaction = null;
        }
    }

    public ICCommand CreateCommand() {
        return new CCommand(this);
    }
}
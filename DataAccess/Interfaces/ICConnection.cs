namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICConnection {
	ICCommand CreateCommand();

	Task Connect(CancellationToken cancellationToken = default);

	Task Disconnect();

	Task BeginTransaction(CancellationToken cancellationToken = default);

	Task CommitTransaction(CancellationToken cancellationToken = default);

	Task CancelTransaction(CancellationToken cancellationToken = default);
}

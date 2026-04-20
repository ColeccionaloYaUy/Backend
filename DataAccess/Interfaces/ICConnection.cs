namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICConnection {
	public ICCommand CreateCommand();

	public Task Connect(CancellationToken cancellationToken = default);

	public Task Disconnect();

	public Task BeginTransaction(CancellationToken cancellationToken = default);

	public Task CommitTransaction(CancellationToken cancellationToken = default);

	public Task CancelTransaction(CancellationToken cancellationToken = default);
}

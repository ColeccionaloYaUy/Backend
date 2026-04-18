namespace API.DataAccess.Interfaces;

public interface ICConnection {
	public ICCommand CreateCommand();

	public Task Connect();


    public Task Disconnect();


    public Task BeginTransaction();

	public Task CommitTransaction();

	public Task CancelTransaction();
}
namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICDataReader {
	public T GetValue<T>(string alias);
	public Task<bool> ReadAsync(CancellationToken cancellationToken = default);
	public Task CloseAsync();
}

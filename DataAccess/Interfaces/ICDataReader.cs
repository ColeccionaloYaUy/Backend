namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICDataReader : IAsyncDisposable {
	T GetValue<T>(string alias);
	Task<bool> ReadAsync(CancellationToken cancellationToken = default);
	Task CloseAsync();
}

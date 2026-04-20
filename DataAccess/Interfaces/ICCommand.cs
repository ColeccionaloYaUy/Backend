using System.Data;

namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICCommand {
	public string CommandText { get; set; }
	public void AddParameter(string name, object value, DbType? type = null);

	public Task ExecuteCommandQuery(Action<ICDataReader> func, CancellationToken cancellationToken = default);
	public Task<bool> ExecuteCommandExists(CancellationToken cancellationToken = default);
	public Task<bool> ExecuteCommandNonQuery(CancellationToken cancellationToken = default);

	public Task<T> ExecuteGetValue<T>(string name, CancellationToken cancellationToken = default);
	public Task<T?> ExecuteSelect<T>(Action<T, ICDataReader> loadData, CancellationToken cancellationToken = default) where T : new();

	public Task<ICDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default);

	public Task<List<T>> ExecuteSelectList<T>(Action<T, ICDataReader> loadData, CancellationToken cancellationToken = default) where T : new();
}

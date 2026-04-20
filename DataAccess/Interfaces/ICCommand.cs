using System.Data;

namespace ColeccionaloYa.DataAccess.Interfaces;

public interface ICCommand {
	string CommandText { get; set; }
	void AddParameter(string name, object? value, DbType? type = null);

	Task ExecuteCommandQuery(Action<ICDataReader> func, CancellationToken cancellationToken = default);
	Task<bool> ExecuteCommandExists(CancellationToken cancellationToken = default);
	Task<bool> ExecuteCommandNonQuery(CancellationToken cancellationToken = default);

	Task<T> ExecuteGetValue<T>(string name, CancellationToken cancellationToken = default);
	Task<T?> ExecuteSelect<T>(Func<ICDataReader, T> map, CancellationToken cancellationToken = default) where T : class;

	Task<ICDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default);

	Task<List<T>> ExecuteSelectList<T>(Func<ICDataReader, T> map, CancellationToken cancellationToken = default);
}

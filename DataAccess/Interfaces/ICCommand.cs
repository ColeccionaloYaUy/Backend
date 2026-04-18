using System.Data;

namespace API.DataAccess.Interfaces;

public interface ICCommand {
	public string CommandText { get; set; }
	public void AddParameter(string name, object value, DbType? type = null);

	public Task ExecuteCommandQuery(Action<ICDataReader> func);
	public Task<bool> ExecuteCommandExists();
	public Task<bool> ExecuteCommandNonQuery();

	public Task<T> ExecuteGetValue<T>(string name);
	public Task<T?> ExecuteSelect<T>(Action<T, ICDataReader> loadData) where T : new();

	public Task<ICDataReader> ExecuteReaderAsync();

	public Task<List<T>> ExecuteSelectList<T>(Action<T, ICDataReader> loadData) where T : new();
}
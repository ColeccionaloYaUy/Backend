namespace API.DataAccess.Interfaces;

public interface ICDataReader {
	public T GetValue<T>(string alias);
	public Task<bool> ReadAsync();
	public Task CloseAsync();
}
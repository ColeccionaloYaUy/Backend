namespace ColeccionaloYa.Persistence.Storage.Interfaces;

public record StoredFile(string Url, string FileName, long Size);

public interface IFileStorage {
	Task<StoredFile> SaveAsync(byte[] content, string originalFileName, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken);
}

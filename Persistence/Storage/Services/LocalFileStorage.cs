using ColeccionaloYa.Persistence.Storage.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Storage.Services;

/// <summary>
/// Local filesystem implementation of <see cref="IFileStorage"/>. Files are written under
/// {currentDirectory}/wwwroot/uploads and exposed through the static files middleware at /uploads/{fileName}.
/// Swapping to Supabase Storage (already referenced by this project) is a matter of providing an
/// alternate <see cref="IFileStorage"/> implementation; no consumer changes are required.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class LocalFileStorage : IFileStorage {
	private const string RelativeFolder = "uploads";

	private static string UploadsPhysicalRoot =>
		Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", RelativeFolder);

	public async Task<StoredFile> SaveAsync(byte[] content, string originalFileName, CancellationToken cancellationToken) {
		Directory.CreateDirectory(UploadsPhysicalRoot);

		var extension = Path.GetExtension(originalFileName);
		var fileName = $"{Guid.NewGuid():N}{extension}";
		var physicalPath = Path.Combine(UploadsPhysicalRoot, fileName);

		await File.WriteAllBytesAsync(physicalPath, content, cancellationToken);

		return new StoredFile($"/{RelativeFolder}/{fileName}", fileName, content.LongLength);
	}

	public Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken) {
		var safeName = Path.GetFileName(fileName);
		var physicalPath = Path.Combine(UploadsPhysicalRoot, safeName);

		if (!File.Exists(physicalPath)) {
			return Task.FromResult(false);
		}

		File.Delete(physicalPath);
		return Task.FromResult(true);
	}
}

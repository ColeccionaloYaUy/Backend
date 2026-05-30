using ColeccionaloYa.Persistence.Storage.Interfaces;

namespace ColeccionaloYa.Application.Upload;

public record UploadedFileDto(string Url, string Filename, long Size) {
	public static UploadedFileDto From(StoredFile file) =>
		new(file.Url, file.FileName, file.Size);
}

using ColeccionaloYa.Domain.Files.Exceptions;

namespace ColeccionaloYa.Application.Files;

public static class ImageValidation {
	public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

	private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) {
		".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp",
	};

	public static void Validate(byte[] content, string fileName) {
		var extension = Path.GetExtension(fileName);
		if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension)) {
			throw new UnsupportedImageTypeException(fileName);
		}

		if (content.LongLength > MaxBytes) {
			throw new PayloadTooLargeException(MaxBytes);
		}
	}
}

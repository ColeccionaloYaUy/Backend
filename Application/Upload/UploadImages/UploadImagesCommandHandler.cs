using ColeccionaloYa.Application.Files;
using ColeccionaloYa.Persistence.Storage.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Upload.UploadImages;

public class UploadImagesCommandHandler : IRequestHandler<UploadImagesCommand, List<UploadedFileDto>> {
	private readonly IFileStorage _FileStorage;

	public UploadImagesCommandHandler(IFileStorage fileStorage) {
		_FileStorage = fileStorage;
	}

	public async Task<List<UploadedFileDto>> Handle(UploadImagesCommand request, CancellationToken cancellationToken) {
		foreach (var item in request.Files) {
			ImageValidation.Validate(item.FileBytes, item.FileName);
		}

		var result = new List<UploadedFileDto>();
		foreach (var item in request.Files) {
			var stored = await _FileStorage.SaveAsync(item.FileBytes, item.FileName, cancellationToken);
			result.Add(UploadedFileDto.From(stored));
		}

		return result;
	}
}

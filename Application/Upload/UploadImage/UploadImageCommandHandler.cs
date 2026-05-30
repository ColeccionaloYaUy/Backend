using ColeccionaloYa.Application.Files;
using ColeccionaloYa.Persistence.Storage.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Upload.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, UploadedFileDto> {
	private readonly IFileStorage _FileStorage;

	public UploadImageCommandHandler(IFileStorage fileStorage) {
		_FileStorage = fileStorage;
	}

	public async Task<UploadedFileDto> Handle(UploadImageCommand request, CancellationToken cancellationToken) {
		ImageValidation.Validate(request.FileBytes, request.FileName);
		var stored = await _FileStorage.SaveAsync(request.FileBytes, request.FileName, cancellationToken);
		return UploadedFileDto.From(stored);
	}
}

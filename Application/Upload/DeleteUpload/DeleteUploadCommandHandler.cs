using ColeccionaloYa.Domain.Files.Exceptions;
using ColeccionaloYa.Persistence.Storage.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Upload.DeleteUpload;

public class DeleteUploadCommandHandler : IRequestHandler<DeleteUploadCommand, Unit> {
	private readonly IFileStorage _FileStorage;

	public DeleteUploadCommandHandler(IFileStorage fileStorage) {
		_FileStorage = fileStorage;
	}

	public async Task<Unit> Handle(DeleteUploadCommand request, CancellationToken cancellationToken) {
		var deleted = await _FileStorage.DeleteAsync(request.FileName, cancellationToken);
		if (!deleted) {
			throw new UploadedFileNotFoundException(request.FileName);
		}

		return Unit.Value;
	}
}

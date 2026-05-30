using MediatR;

namespace ColeccionaloYa.Application.Upload.UploadImages;

public record UploadFileItem(byte[] FileBytes, string FileName);

public record UploadImagesCommand(IReadOnlyList<UploadFileItem> Files) : IRequest<List<UploadedFileDto>>;

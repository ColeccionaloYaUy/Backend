using MediatR;

namespace ColeccionaloYa.Application.Upload.UploadImage;

public record UploadImageCommand(byte[] FileBytes, string FileName) : IRequest<UploadedFileDto>;

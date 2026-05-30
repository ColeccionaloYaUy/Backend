using MediatR;

namespace ColeccionaloYa.Application.Upload.DeleteUpload;

public record DeleteUploadCommand(string FileName) : IRequest<Unit>;

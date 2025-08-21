
using MediatR;
using Microsoft.AspNetCore.Http; // We need this for IFormFile

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommand : IRequest<Guid>
{
    public IFormFile File { get; set; } = null!;
}

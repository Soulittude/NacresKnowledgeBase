using MediatR;
using NacresKnowledgeBase.Core.Entities;
using NacresKnowledgeBase.Infrastructure.Persistence;

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly ApplicationDbContext _context;

    public UploadDocumentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create a new Document entity from the request data
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            UploadedOn = DateTime.UtcNow
        };

        // For now, we are just saving the metadata. We will handle file content later.

        // 2. Add the new entity to the database context
        await _context.Documents.AddAsync(document, cancellationToken);

        // 3. Save the changes to the database
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Return the ID of the new document
        return document.Id;
    }
}
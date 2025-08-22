using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Core.Entities;

namespace NacresKnowledgeBase.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<TextChunk> TextChunks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
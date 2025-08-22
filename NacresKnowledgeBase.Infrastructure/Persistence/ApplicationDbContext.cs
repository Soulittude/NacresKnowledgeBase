using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Core.Entities;
using Pgvector.EntityFrameworkCore;
using NacresKnowledgeBase.Application.Abstractions;

namespace NacresKnowledgeBase.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents
    {
        get; set;
    }
    public DbSet<TextChunk> TextChunks { get; set; }
}
using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Core.Entities;

namespace NacresKnowledgeBase.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Document> Documents { get; set; }
}
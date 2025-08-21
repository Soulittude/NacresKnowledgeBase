using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Core.Entities;

namespace NacresKnowledgeBase.Infrastructure.Persistance;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Document> Documents { get; set; }
}
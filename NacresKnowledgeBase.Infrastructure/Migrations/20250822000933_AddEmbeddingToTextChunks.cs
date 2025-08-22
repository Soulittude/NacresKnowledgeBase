using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace NacresKnowledgeBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingToTextChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "TextChunks",
                type: "vector(1536)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "TextChunks");
        }
    }
}

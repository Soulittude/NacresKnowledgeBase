using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Application.Features.Documents.Commands;
using NacresKnowledgeBase.Infrastructure.Persistence;
using Pgvector.EntityFrameworkCore;
using NacresKnowledgeBase.Application.Abstractions;
using NacresKnowledgeBase.Application.Services;
using NacresKnowledgeBase.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. --- Configure Services ---

// Add our DbContext for database access
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseVector()));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();

// Add MediatR and tell it to find handlers in our Application project
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(UploadDocumentCommand).Assembly));

// This is the service that makes API Controllers work
builder.Services.AddControllers();

builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
});

// These are the standard services for Swagger UI (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 2. --- Build the Application ---
var app = builder.Build();


// 3. --- Configure the HTTP Pipeline ---

// Use Swagger in the development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// We are temporarily disabling this to fix the browser issue.
// app.UseHttpsRedirection();

// This is the line that maps requests to our controllers (like DocumentsController)
app.MapControllers();


// 4. --- Run the Application ---
app.Run();
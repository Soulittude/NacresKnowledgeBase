using MediatR;
using Microsoft.AspNetCore.Mvc;
using NacresKnowledgeBase.Application.Features.Documents.Commands;
using NacresKnowledgeBase.Application.Features.Documents.Queries;

namespace NacresKnowledgeBase.Api.Controllers;

// Bu sınıfı, Swagger'a yardımcı olmak için ekliyoruz.
public class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    // Metodun imzasını IFormFile yerine yeni sınıfımızı kullanacak şekilde değiştiriyoruz.
    public async Task<IActionResult> UploadDocument([FromForm] FileUploadRequest request)
    {
        // Dosyanın null olup olmadığını kontrol ediyoruz.
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Komutu oluştururken request.File kullanıyoruz.
        var command = new UploadDocumentCommand { File = request.File };

        var documentId = await _sender.Send(command);

        return Ok(new { DocumentId = documentId });
    }

    [HttpPost("ask")] // URL: /api/documents/ask
    public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
    {
        var query = new AskQuestionQuery { Question = request.Question };
        var answer = await _sender.Send(query);
        return Ok(new { Answer = answer });
    }

    // Bu yardımcı sınıfı da controller dosyasının içine veya altına ekleyin
    public class AskQuestionRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}
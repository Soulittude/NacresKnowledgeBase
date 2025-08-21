using MediatR;
using Microsoft.AspNetCore.Mvc;
using NacresKnowledgeBase.Application.Features.Documents.Commands;

namespace NacresKnowledgeBase.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var command = new UploadDocumentCommand { File = file };

        var documentId = await _sender.Send(command);

        return Ok(new { documentId = documentId });
    }
}
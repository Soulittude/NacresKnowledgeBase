using MediatR;

namespace NacresKnowledgeBase.Application.Features.Documents.Queries;

// Bu sorgu, bir string (soru) alır ve bir string (cevap) döndürür.
public class AskQuestionQuery : IRequest<string>
{
    public string Question { get; set; } = string.Empty;
}
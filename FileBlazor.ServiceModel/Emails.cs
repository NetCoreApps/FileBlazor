using ServiceStack;
using ServiceStack.DataAnnotations;

namespace FileBlazor.ServiceModel;

[ExcludeMetadata]
[Restrict(InternalOnly = true)]
public class SendEmail : IReturn<EmptyResponse>
{
    public string To { get; set; }
    public string? ToName { get; set; }
    public string Subject { get; set; }
    public string? BodyText { get; set; }
    public string? BodyHtml { get; set; }
}
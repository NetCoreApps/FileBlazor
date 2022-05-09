using ServiceStack;
using FileBlazor.ServiceModel;
using System;
using ServiceStack.FluentValidation;
using ServiceStack.Web;

namespace FileBlazor.ServiceInterface;

public class MyServices : Service
{
    public static string AssertName(string Name) => Name.IsNullOrEmpty() 
        ? throw new ArgumentNullException(nameof(Name))
        : Name;

    public object Any(Hello request) =>
        new HelloResponse { Result = $"Hello, {AssertName(request.Name)}!" };

    public object Any(HelloSecure request) => 
        new HelloResponse { Result = $"Hello, {AssertName(request.Name)}!" };
}


//public class QueryFilesValidator : AbstractValidator<QueryAppUserS3File>, IRequiresRequest
//{
//    public QueryFilesValidator()
//    {
//        var session = Request.GetSession();
//        RuleFor(x => x.AppUserId)
//            .Must(x => x != null && x.ToString() == session.UserAuthId)
//            .When(x => x.FileAccessType == ServiceModel.Types.FileAccessType.Private)
//            .WithMessage("Private files are restricted to users only.");
//    }

//}


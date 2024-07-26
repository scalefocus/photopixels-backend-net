using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace SF.PhotoPixels.Application.Commands.Tus;

public class UpdateMetadataAttribute : ActionFilterAttribute
{
    override public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Request.Method == "POST")
        {
            var headerData = context.HttpContext.Request.Headers["Upload-Metadata"];

            var id = context.HttpContext.User.Identities.First().Claims.First(x => x.Type.Equals("id")).Value;
            var convertedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(id));

            context.HttpContext.Request.Headers["Upload-Metadata"] = headerData + $",userId {convertedId}";
        }

        return next();
    }
}

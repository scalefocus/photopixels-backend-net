using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using SolidTUS.Constants;
using SolidTUS.Models;
using SolidTUS.ProtocolFlows;
using SolidTUS.ProtocolHandlers;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Endpoints = System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.EndpointDataSource>;

using static Microsoft.AspNetCore.Http.HttpMethods;
using SolidTUS.Functional.Models;
namespace SolidTUS.Attributes;

/// <summary>
/// Identifies an action that supports TUS resource creation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TusCreationPhotopixelsAttribute : TusCreationAttribute
{
    /// <summary>
    /// Instantiate a new object of <see cref="TusCreationAttribute"/>
    /// </summary>
    public TusCreationPhotopixelsAttribute()
    {
    }

    /// <summary>
    /// Instantiate a new <see cref="TusCreationAttribute"/> creation endpoint handler.
    /// </summary>
    /// <param name="template">The route template</param>
    public TusCreationPhotopixelsAttribute([StringSyntax("Route")] string template) : base(template)
    {
        
    }
    
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await base.OnActionExecutionAsync(context, next);
    }

    /// <inheritdoc />
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        //base.OnResultExecutionAsync(context, next);
        // Before sending headers -->
        context.HttpContext.Response.OnStarting(state =>
        {
            var ctx = (ResultExecutingContext)state;
            var isPost = IsPost(ctx.HttpContext.Request.Method);
            var response = ctx.HttpContext.Response;
            var isSuccess = response.StatusCode >= 200 && response.StatusCode < 300;
            if (isPost && isSuccess)
            {
                ctx.HttpContext.Response.StatusCode = 201;
                string location = ctx.HttpContext.Response.Headers["Location"]!;
                ctx.HttpContext.Response.Headers["Location"] = location.TrimStart('/');
            }
            return Task.CompletedTask;
        }, context);

        await next();
    }
}

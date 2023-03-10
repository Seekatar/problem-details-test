using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ProblemDetailsTest;

/// <summary>
/// Use the .NET 7 problemDetailsService.WriteAsync to write problem details out
/// This writes details, even if developer exceptions are turned off, otherwise
/// the default middleware just returns minimal details
/// </summary>
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }
            
            context.Response.Clear();
            
            if (ex is AggregateException aggr && aggr.InnerExceptions.Count == 1)
            {
                ex = aggr.InnerExceptions[0];
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
            {
                if (ex is ProblemDetailsException pde)
                {
                    context.Response.StatusCode = pde.ProblemDetails.Status ?? context.Response.StatusCode;
                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = pde.ProblemDetails
                    });
                    if (pde.LogLevel != LogLevel.None)
                        _logger.Log(pde.LogLevel, ex, "An unhandled exception has occurred while executing the request. In " + nameof(ProblemDetailsMiddleware));
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    var pd = new ProblemDetails
                    {
                        Type = ProblemDetailsException.Status500Url,
                        Title = $"Unhandled exception of type {ex.GetType().Name}",
                        Detail = ex.Message,
                        Status = context.Response.StatusCode
                    };
                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = pd
                    });
                    _logger.LogError(ex, "An unhandled exception has occurred while executing the request. In " + nameof(ProblemDetailsMiddleware));
                }
            }
            else
            {
                throw;
            }
        }
    }
}
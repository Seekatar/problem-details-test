using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Reflection;
using ProblemDetailsTest.Models;

namespace ProblemDetailsTest.Controllers;

[Produces("application/json")]
public class ExceptionController : Controller
{
    /// <summary>
    /// Throw a not implemented exception
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="marketEntityId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    [Route("/api/throw/not-implemented/{clientId}/{marketEntityId}")]
    [SwaggerOperation("ThrowNotImplemented")]
    [SwaggerResponse(statusCode: 200, type: typeof(Widget), description: "Ok")]
    public virtual ActionResult ThrowNotImplemented(Guid clientId, int marketEntityId)
    {
        throw new NotImplementedException("Throwing NotImplementedException");
    }

    /// <summary>
    /// Throw a problem details exception
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="marketEntityId"></param>
    /// <param name="logLevel">Info=2,Warning,Error,Critial,None=6</param>
    /// <returns></returns>
    /// <exception cref="ProblemDetailsException"></exception>
    [HttpGet]
    [Route("/api/throw/details/{clientId}/{marketEntityId}/{logLevel}")]
    [SwaggerOperation("ThrowProblemDetails")]
    [SwaggerResponse(statusCode: 200, type: typeof(Widget), description: "Ok")]
    public virtual ActionResult ThrowProblemDetails(Guid clientId, int marketEntityId, LogLevel logLevel = LogLevel.Error)
    {
        var pd = new ProblemDetails()
        {
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1",
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Throwing ProblemDetailsException",
            Detail = $"My detail message, look for a and status of 500 and log level of {logLevel}",
        };
        pd.Extensions["extension_value_int"] = 1232;
        pd.Extensions["extension_value_string"] = "Some value";
        pd.Extensions["method_name"] = MethodBase.GetCurrentMethod()?.Name ?? "Unknown";

        throw new ProblemDetailsException(pd,logLevel);
    }
}

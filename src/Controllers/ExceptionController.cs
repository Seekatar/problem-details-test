using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Reflection;
using ProblemDetailsTest.Models;

namespace ProblemDetailsTest.Controllers;

[Produces("application/json")]
public class ExceptionController : Controller
{
    public static IDictionary<string, object?> NewTestExtension()
    {
        Dictionary<string, object?> extensions = new ();
        SetTestExtension(extensions);
        return extensions;
    }
    static void SetTestExtension(IDictionary<string,object?> extensions)
    {
        extensions["extension_value_int"] = 1232;
        extensions["extension_value_string"] = "Some value";
        extensions["extension_value_now"] = DateTime.Now;
        extensions["method_name"] = MethodBase.GetCurrentMethod()?.Name ?? "Unknown";
    }
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
            Status = StatusCodes.Status500InternalServerError,
            Title = "Throwing ProblemDetailsException",
            Detail = $"My detail message, look for a and status of 500 and log level of {logLevel}",
        };
        SetTestExtension(pd.Extensions);
        pd.Extensions.Add("log_level", logLevel);

        throw new ProblemDetailsException(pd,logLevel);
    }

    /// <summary>
    /// Call ControllerBase.Problem
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("/api/throw/problem")]
    [SwaggerOperation("CallProblem")]
    [SwaggerResponse(statusCode: 200, type: typeof(Widget), description: "Ok")]
    public virtual ActionResult CallProblem()
    {
        // take default values in ProblemDetails that it creates except for detail
        // can't pass in extensions
        return Problem("Hi from problem");
    }

    /// <summary>
    /// Call ControllerBase.ValidationProblem
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("/api/throw/validation-problem")]
    [SwaggerOperation("CallValidationProblem")]
    [SwaggerResponse(statusCode: 200, type: typeof(Widget), description: "Ok")]
    public virtual IActionResult CallValidationProblem()
    {
        var errors = new ValidationProblemDetails(new Dictionary<string, string[]>() { { "s", new string[] { "value missing" } }, { "t", new string[] { "value missing", "not 1" } } });
        return ValidationProblem(errors);
    }
}

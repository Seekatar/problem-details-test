using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace ProblemDetailsTest;

public class ProblemDetailsException : Exception
{
    public ProblemDetails ProblemDetails { get; }
    public LogLevel LogLevel { get; }

    public const string Status400Url = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1";
    public const string Status500Url = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1";

    public ProblemDetailsException(HttpStatusCode statusCode, LogLevel logLevel = LogLevel.Error) : this(new ProblemDetails
    {
        Status = (int)statusCode,
        Type = GetType((int)statusCode),
        Title = nameof(ProblemDetailsException),
    }, logLevel)
    {
    }
    public ProblemDetailsException(ProblemDetails problemDetails, LogLevel logLevel = LogLevel.Error) : base(problemDetails.Title)
    {
        ProblemDetails = problemDetails;
        LogLevel = logLevel;
    }

    public ProblemDetailsException(Exception e, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, LogLevel logLevel = LogLevel.Error) : this(new ProblemDetails
    {
        Status = (int)statusCode,
        Type = GetType((int)statusCode),
        Title = nameof(HttpStatusCode.InternalServerError),
        Detail = e.Message
    }, logLevel)
    {
    }

    public ProblemDetailsException(string title, Exception e, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, LogLevel logLevel = LogLevel.Error) : this(new ProblemDetails
        {
            Status = (int)statusCode,
            Type = GetType((int)statusCode),
            Title = title,
            Detail = e.Message
        },logLevel)
    { 
    }

    public override string ToString()
    {
        return ProblemDetailsToString(ProblemDetails) + base.ToString();
    }

    private static string ProblemDetailsToString(ProblemDetails problemDetails)
    {
        if (problemDetails is null) return "";

        var sb = new StringBuilder();
        sb.AppendLine("ProblemDetails:");
        sb.AppendLine($"  Status: {problemDetails.Status}");
        if (!string.IsNullOrWhiteSpace(problemDetails.Type) && problemDetails.Type != "about:blank")
            sb.AppendLine($"  Type: {problemDetails.Type}");
        sb.AppendLine($"  Title: {problemDetails.Title}");
        if (!string.IsNullOrWhiteSpace(problemDetails.Detail))
            sb.AppendLine($"  Detail: {problemDetails.Detail}");
        if (!string.IsNullOrWhiteSpace(problemDetails.Instance))
            sb.AppendLine($"  Instance: {problemDetails.Instance}");
        if (problemDetails.Extensions?.Count > 0)
        {
            sb.AppendLine("  Extensions:");
            foreach (var (key, value) in problemDetails.Extensions)
            {
                sb.AppendLine($"    {key}: {value}");
            }
        }
        return sb.ToString();
    }

    public static string GetType(int statusCode) => statusCode >= 500 ? Status500Url : Status400Url;
}

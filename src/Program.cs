using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsTest;
using Serilog;
using System.Net;
var builder = WebApplication.CreateBuilder(args);

#region Added for Problem Details test

ProblemDetailsEnum options = 0;
if (args.Length > 0 && Enum.TryParse<ProblemDetailsEnum>(args[0], out options))
{
    Console.WriteLine($"Args is {args[0]:X}");
}

#region Add Serilog
builder.Host.UseSerilog((ctx, loggerConfig) => loggerConfig.ReadFrom.Configuration(builder.Configuration));
#endregion

if ((options & ProblemDetailsEnum.CustomProblemDetails) != 0)
{
    // setting CustomizeProblemDetails doesn't call middleware we add
    builder.Services.AddProblemDetails(opt =>
    {
        opt.CustomizeProblemDetails = (problemDetailsCtx) =>
        {
            Console.WriteLine("Hi in custom");
            problemDetailsCtx.AdditionalMetadata?.Append(999);
            problemDetailsCtx.ProblemDetails.Type = "set in customproblemdetails";
        };
    });
}
else
{
    builder.Services.AddProblemDetails();
}
#endregion

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#region Added for Problem Details test
// add default exception handler
app.UseExceptionHandler();

// this returns problemDetails if caller sets accept to application/json for responses with status codes between 400 and 599 that do not have a body
app.UseStatusCodePages();

// add our middleware to call WriteAsync so we get contents or a ProblemDetailsException instead of 500
if ((options & ProblemDetailsEnum.UseMyMiddleware) != 0)
{
    app.UseMiddleware<ProblemDetailsMiddleware>();
}

if ((options & ProblemDetailsEnum.DeveloperExceptionPage) != 0)
    app.UseDeveloperExceptionPage(); // with this dumps all details, including stack, otherwise this just get a 500
#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// and this was added for Problem Details test
[Flags]
enum ProblemDetailsEnum
{
    Vanilla = 0,
    DeveloperExceptionPage = 1,
    CustomProblemDetails = 2,
    UseMyMiddleware = 4
}
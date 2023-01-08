using Microsoft.AspNetCore.Mvc;
using ProblemDetailsTest;
using ProblemDetailsTest.Controllers;
using Serilog;

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
// if this isn't added and ASPNETCORE_ENVIRONMENT=Development, get the UseDeveloperExceptionPage
// if this isn't added and ASPNETCORE_ENVIRONMENT=Production, get no JSON, just 500
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

#region endpoints for testing IResult in Minimal APIs
app.MapGet("/api/throw/map-get", async (HttpContext context) => {

    // try to write out problem details, but this doesn't work
    if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
    {
        // this gets into here, but still returns 200 and Hello World
        // if comment out Hello World, then it returns 200 and nothing
        Console.WriteLine("Hi in map-get");
        await problemDetailsService.WriteAsync(new ProblemDetailsContext { 
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Title = "Hi from map-get",
                Status = StatusCodes.Status400BadRequest,
                Type = ProblemDetailsException.GetType(StatusCodes.Status400BadRequest)
            }
        });
    }

    await context.Response.WriteAsync("Hello World");
});

app.MapGet("/api/problem", (HttpContext context) => {
    // take default of all but detail and extensions
    return Results.Problem("Hi from problem", extensions: ExceptionController.NewTestExtension());
});

app.MapGet("/api/validation-problem", (HttpContext context) => {
    // take default of all but detail and extensions
    var errors = new Dictionary<string, string[]>() { { "s", new string[] { "value missing" } }, { "t", new string[] { "value missing", "not 1" } } };
    return Results.ValidationProblem(errors, "Hi from validation-problem", extensions: ExceptionController.NewTestExtension());
});
#endregion

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

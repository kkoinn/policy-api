using Microsoft.AspNetCore.Diagnostics;

public static class ExceptionHandlingExtensions
{
    extension(WebApplication app)
    {
        public void UseGlobalExceptionHandler()
        {
            app.UseExceptionHandler(exceptionApp =>
                exceptionApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var ex = exceptionFeature?.Error;

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    await Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "An unexpected error occurred.",
                        detail: ex?.Message,
                        instance: context.Request.Path,
                        extensions: new Dictionary<string, object?>
                        {
                            { "traceId", context.TraceIdentifier }
                        }
                    ).ExecuteAsync(context);
                })
            );
        }
    }
}

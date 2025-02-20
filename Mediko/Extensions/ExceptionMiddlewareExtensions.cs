using Mediko.Entities.ErrorModel;
using Mediko.Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Mediko.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature is not null)
                    {
                        Console.WriteLine($" HATA OLUŞTU: {contextFeature.Error.GetType()} - {contextFeature.Error.Message}");

                        context.Response.StatusCode = contextFeature.Error switch
                        {
                            NotFoundException => StatusCodes.Status404NotFound,
                            BadRequestException => StatusCodes.Status400BadRequest,
                            ArgumentNullException => StatusCodes.Status400BadRequest,
                            _ => StatusCodes.Status500InternalServerError
                        };

                        var errorDetails = new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message
                        };

                        await context.Response.WriteAsync(errorDetails.ToString());
                    }
                    else
                    {
                        Console.WriteLine(" Beklenmeyen bir hata oluştu ancak ExceptionMiddleware yakalayamadı.");
                    }
                });
            });
        }

    }

}

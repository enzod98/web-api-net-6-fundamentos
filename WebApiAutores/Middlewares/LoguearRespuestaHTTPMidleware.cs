using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace WebApiAutores.Middlewares
{
    //Con esta extensión, podemos llamar más fácilmente al middleware desde Startup
    public static class LoguearRespuestaHTTPMidlewareExtension
    {
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMidleware>();
        }
    }

    public class LoguearRespuestaHTTPMidleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMidleware> logger;

        public LoguearRespuestaHTTPMidleware(RequestDelegate siguiente, ILogger<LoguearRespuestaHTTPMidleware> logger)
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }

        //Invoke O InvokeAsync son obligatorios para crear un Middleware
        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginalRespouesta = contexto.Response.Body;
                contexto.Response.Body = ms;

                await siguiente(contexto);   //hasta aquí se ejecuta al momento de recibir la petición
                                            //luego pasa a los demás middlewares

                //Cuando se devuelve la respuesta empieza desde aquí
                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespouesta);
                contexto.Response.Body = cuerpoOriginalRespouesta;

                logger.LogInformation(respuesta);
            }
        }
    }
}

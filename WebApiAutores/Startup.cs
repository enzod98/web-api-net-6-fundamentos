using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {

            // Add services to the container.
            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
            }).AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
            );

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("defaultConnection")
            ));

            //Cuando una clase requiera un IServicio, se le pasa una nueva instanca de ServicioA
            services.AddTransient<IServicio, ServicioA>();  //Siempre es una nueva instancia de ServicioA
            //services.AddScoped<IServicio, ServicioA>(); //El tiempo de vida aumenta, se usa la misma
                                                        //instancia dentro del contexto
                                                        //Cada petición HTTP tiene una instancia distinta

            //services.AddSingleton<IServicio, ServicioA>();  //La misma instancia para todas las peticiones HTTP

            services.AddTransient<ServicioTransient>();
            services.AddScoped<ServicioScoped>();
            services.AddSingleton<ServicioSingleton>();

            services.AddTransient<FiltroDeAccion>();    //Importar filtros personalizados
            services.AddHostedService<EscribirEnArchivo>();    //Importar filtros personalizados
            
            //caché de respuesta
            services.AddResponseCaching();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            //Añadir Middleware personalizado
            //app.UseMiddleware<LoguearRespuestaHTTPMidleware>();

            app.UseLoguearRespuestaHTTP();

            //bifurcación
            app.Map("/ruta1", app =>
            {
                //Con run podemos ejecutar un middleware y detener la ejecución de los siguientes
                app.Run(async contexto =>
                {
                    await contexto.Response.WriteAsync("Estoy interceptando la tubería");
                });
            });

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

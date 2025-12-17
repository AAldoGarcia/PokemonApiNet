using NoSeProgramarJajajLoL.Services;

namespace NoSeProgramarJajajLoL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Agregar servicios al contenedor
            builder.Services.AddControllers();

            // Registramos servicio y el HttpClient
            builder.Services.AddHttpClient<PokeServices>();

            // ---> AGREGAR ESTA LÍNEA PARA ACTIVAR LA CACHÉ <---
            builder.Services.AddMemoryCache();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ?? CONFIGURACIÓN DE CORS (CRUCIAL PARA REACT)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        // Aquí permitimos que el puerto de Vite/React (ej. 5173 o 3000) entre a la API
                        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5174")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            var app = builder.Build();

            // 2. Configurar el pipeline HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

          

            // ?? ACTIVAR CORS (Debe ir antes de UseAuthorization)
            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors("AllowReactApp");

            app.Run();
        }
    }
}
// POSTMAN (HTTP Requests) 
//    ?
//Controllers(Reciben JSON)
//    ?
//Services(Procesan lógica)
//    ?
//Models(Guardan en memoria / BD)
//    ?
//Controllers(Devuelven JSON)
//    ?
//POSTMAN(Recibe Responses)
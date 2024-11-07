using Serilog;
using StudentBlogg.Extensions;
using StudentBlogg.Health;
using StudentBlogg.Middleware;
namespace StudentBlogg;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Services
        ConfigureServices(builder);

        WebApplication app = builder.Build();
        
        // Middleware & Pipeline
        ConfigureApp(app);

        app.Run();
    }
    
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerBasicAuthentication();

        // Legg til tjenester for User, Post, og Comment
        builder.Services.AddUserServices();
        builder.Services.AddPostServices();
        builder.Services.AddCommentServices();
        
        // Database konfigurasjon
        builder.Services.AddDatabase(builder.Configuration);

        // Health Checks
        builder.Services.AddHealthChecks()
            .AddCheck<DataBaseHealthCheck>("DataBase");

        // Legg til autentisering, unntakshåndtering og validering
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureExceptionHandler();
        builder.Services.ConfigureFluentValidation();

        // Logging
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHealthChecks("/_health")
            .UseMiddleware<DatabaseConnectionMiddleware>()
            .UseExceptionHandler(_ => { }) 
            .UseMiddleware<BasicAuthentication>()
            .UseHttpsRedirection()
            .UseAuthorization();

        app.MapControllers();
    }
}
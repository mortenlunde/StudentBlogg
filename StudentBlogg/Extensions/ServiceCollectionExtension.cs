using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StudentBlogg.Common.Interfaces;
using StudentBlogg.Configurations;
using StudentBlogg.Data;
using StudentBlogg.Feature.Comments;
using StudentBlogg.Feature.Comments.Interfaces;
using StudentBlogg.Feature.Posts;
using StudentBlogg.Feature.Posts.Interfaces;
using StudentBlogg.Feature.Users;
using StudentBlogg.Feature.Users.Interfaces;
using StudentBlogg.Middleware;
using Swashbuckle.AspNetCore.Filters;

namespace StudentBlogg.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddSwaggerBasicAuthentication(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                In = ParameterLocation.Header,
                Description = "Basic Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        }
                    },
                    []
                }
            });
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header, 
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            c.OperationFilter<SecurityRequirementsOperationFilter>();
        });
    }
    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMapper<User, UserDto>, UserMapper>();
        services.AddScoped<IMapper<User, UserRegistrationDto>, UserRegMapper>();
    }

    public static void AddPostServices(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IMapper<Post, PostDto>, PostMapper>();
        services.AddScoped<IMapper<Post, PostRegDto>, PostRegMapper>();
    }

    public static void AddCommentServices(this IServiceCollection services)
    {
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IMapper<Comment, CommentDto>, CommentMapper>();
        services.AddScoped<IMapper<Comment, CommentRegDto>, CommentRegMapper>();
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<BasicAuthentication>();
        services.Configure<BasicAuthenticationOptions>(configuration.GetSection("BasicAuthenticationOptions"));
        services.AddHttpContextAccessor();
    }

    public static void ConfigureExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandling>();
    }

    public static void ConfigureFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation(options =>
            options.DisableDataAnnotationsValidation = true);
    }
    
    public static void AddDatabaseService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StudentBloggDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 33)),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(2)));
        services.AddScoped<DatabaseConnectionMiddleware>();
    }
}
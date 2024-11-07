using MySqlConnector;
using StudentBlogg.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Exception = System.Exception;

namespace StudentBlogg.Middleware;


public class DatabaseConnectionMiddleware(StudentBloggDbContext dbContext, ILogger<DatabaseConnectionMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            logger.LogInformation("Beginning to connect to database");
            await dbContext.Database.CanConnectAsync();
        }
        catch (MySqlException)
        {
            logger.LogError("Failed to connect to the database.");
            throw new DbConnectionException();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "An error occurred during database update.");
            throw new DbConnectionException();
        }
        catch (RetryLimitExceededException ex)
        {
            logger.LogError(ex, "An error occurred during database connection.");
            throw new DbConnectionException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while connecting to the database.");
            throw new DbConnectionException();
        }

        await next(context);
    }
}
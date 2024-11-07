using Microsoft.Extensions.Diagnostics.HealthChecks;
using StudentBlogg.Data;
using Exception = System.Exception;

namespace StudentBlogg.Health;

public class DataBaseHealthCheck(StudentBloggDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Healthy();
            
            return HealthCheckResult.Unhealthy("Can't connect to database");
            
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Databaseconnection failed ..!", e);
        }
    }
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentBlogg.Data;
using StudentBlogg.Feature.Users.Interfaces;
using StudentBlogg.Middleware;

namespace StudentBlogg.Feature.Users;

public class UserRepository(ILogger<UserRepository> logger, StudentBloggDbContext dbContext)
    : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (DbUpdateException)
        {
            throw new DbConnectionException();
        }
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize)
    {
        int skip = (pageNumber - 1) * pageSize;

        Task<List<User>> users = dbContext.Users
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        logger.LogInformation($"GetPagedAsync starting at {skip} of {pageSize}");
        return await users;
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    {
        return await dbContext.Users.Where(predicate).ToListAsync();
    }

    public async Task<User?> AddAsync(User entity)
    {
        if (await dbContext.Users.AnyAsync(u => u.Username == entity.Username))
        {
            throw new UsernameAlreadyExistsException(entity.Username);
        }

        if (await dbContext.Users.AnyAsync(u => u.Email == entity.Email))
        {
            throw new EmailAlreadyExistsException(entity.Email);
        }
        
              
        await dbContext.Users.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        logger.LogInformation($"User added: {entity.Username}");
        
        return entity;
    }

    public async Task<User?> UpdateByIdAsync(Guid id, User entity)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null) return null;
        
        user.Username = entity.Username;
        user.Email = entity.Email;
        user.FirstName = entity.FirstName;
        user.LastName = entity.LastName;
        user.HashedPassword = entity.HashedPassword;
        user.Updated = entity.Updated;
        
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> DeleteByIdAsync(Guid id)
    {
        User? user = await dbContext.Users.FindAsync(id);
        await dbContext.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        
        await dbContext.SaveChangesAsync();
        return user;
    }
}
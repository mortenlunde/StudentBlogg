using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentBlogg.Data;
using StudentBlogg.Feature.Comments.Interfaces;

namespace StudentBlogg.Feature.Comments;

public class CommentRepository(ILogger<CommentRepository> logger, StudentBloggDbContext dbContext)
    : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await dbContext.Comments.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetPagedAsync(int pageNumber, int pageSize)
    {
        int skip = (pageNumber - 1) * pageSize;

        var comments = dbContext.Comments
            .OrderBy(c => c.DateCommented)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        logger.LogInformation($"GetPagedAsync started for {pageSize} of {pageSize}.");
        
        return await comments;
    }

    public async Task<IEnumerable<Comment>> FindAsync(Expression<Func<Comment, bool>> predicate)
    {
        return await dbContext.Comments.Where(predicate).ToListAsync();
    }

    public async Task<Comment?> AddAsync(Comment entity)
    {
        await dbContext.Comments.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        
        return entity;
    }

    public async Task<Comment?> UpdateByIdAsync(Guid id, Comment entity)
    {
        Comment? existingComment = await dbContext.Comments.FindAsync(id);
        if (existingComment == null) return null;
        
        existingComment.Content = entity.Content;
        
        await dbContext.SaveChangesAsync();
        return existingComment;
    }

    public async Task<Comment?> DeleteByIdAsync(Guid id)
    {
        Comment? comment = await dbContext.Comments.FindAsync(id);
        await dbContext.Comments.Where(c => c.Id == id).ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
        return comment;
    }
}
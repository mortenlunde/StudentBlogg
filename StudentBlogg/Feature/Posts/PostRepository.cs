using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentBlogg.Data;
using StudentBlogg.Feature.Posts.Interfaces;

namespace StudentBlogg.Feature.Posts;

public class PostRepository(ILogger<PostRepository> logger, StudentBloggDbContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetPagedAsync([FromQuery]int pageNumber, [FromQuery]int pageSize)
    {
        int skip = (pageNumber - 1) * pageSize;

        var posts = dbContext.Posts
            .OrderBy(u => u.DatePosted)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        logger.LogInformation($"GetPagedAsync starting at {skip} of {pageSize} records");
        
        return await posts;
    }

    public async Task<IEnumerable<Post>> FindAsync(Expression<Func<Post, bool>> predicate)
    {
        return await dbContext.Posts.Where(predicate).ToListAsync();
    }

    public async Task<Post?> AddAsync(Post entity)
    {
        await dbContext.Posts.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        
        return entity;
    }

    public async Task<Post?> UpdateByIdAsync(Guid id, Post entity)
    {
        var existingPost = await dbContext.Posts.FindAsync(id);
        if (existingPost == null) return null;  // Return if post does not exist

        existingPost.Title = entity.Title;
        existingPost.Content = entity.Content;

        await dbContext.SaveChangesAsync();
        return existingPost;
    }


    public async Task<Post?> DeleteByIdAsync(Guid id)
    {
        var post = await dbContext.Posts.FindAsync(id);
        await dbContext.Posts.Where(p => p.Id == id).ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();
        return post;
    }
}
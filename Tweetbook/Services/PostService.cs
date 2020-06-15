using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Data;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext _dataContext;

        public PostService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Post>> GetPostsAsync()
        {
            return await _dataContext.Posts.Include(x => x.Tags).ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _dataContext.Posts.SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<bool> CreatePostAsync(Post post)
        {
            post.Tags?.ForEach(x => x.TagName = x.TagName.ToLower());

            //new line 
            await AddNewTags(post);
            await _dataContext.Posts.AddAsync(post);
            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> UpdatePostAsync(Post postToUpdate)
        {
            postToUpdate.Tags?.ForEach(x => x.TagName.ToLower());
            await AddNewTags(postToUpdate);

            _dataContext.Posts.Update(postToUpdate);
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }
        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await GetPostByIdAsync(postId);

            if (post == null)
                return false;

            _dataContext.Posts.Remove(post);
            var deleted = await _dataContext.SaveChangesAsync();

            return deleted > 0;
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            var post = await _dataContext.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }
            if (post.UserId != userId)
            {
                return false;
            }

            return true;
        }
        private async Task AddNewTags(Post post)
        {
            foreach (var tag in post.Tags)
            {
                var existingTag =
                    await _dataContext.Tags.SingleOrDefaultAsync(x =>
                        x.Name == tag.TagName);
                if (existingTag != null)
                    continue;

                await _dataContext.Tags.AddAsync(new Tag
                { Name = tag.TagName, CreatedOn = DateTime.UtcNow, CreatorId = post.UserId });
            }
        }
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _dataContext.Tags.AsNoTracking().ToListAsync();
        }

        public async Task<bool> CreateTagAsync(Tag newTag)
        {
            newTag.Name = newTag.Name.ToLower();
            var existingTag = await _dataContext.Tags.AsNoTracking().SingleOrDefaultAsync(x => x.Name == newTag.Name);
            if (existingTag != null)
            {
                return true;
            }
            await _dataContext.Tags.AddAsync(newTag);
            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }

        public async Task<Tag> GetTagByNameAsync(string tagName)
        {
            return await _dataContext.Tags.AsNoTracking().SingleOrDefaultAsync(x => x.Name == tagName.ToLower());
        }

        public async Task<bool> DeleteTagAsync(string tagName)
        {
            var tag = await _dataContext.Tags.AsNoTracking().SingleOrDefaultAsync(x => x.Name == tagName.ToLower());

            if (tag == null)
                return true;

            var postTags = await _dataContext.PostTags.Where(x => x.TagName == tagName.ToLower()).ToListAsync();

            _dataContext.PostTags.RemoveRange(postTags);
            _dataContext.Tags.Remove(tag);
            return await _dataContext.SaveChangesAsync() > postTags.Count;
        }
    }
}

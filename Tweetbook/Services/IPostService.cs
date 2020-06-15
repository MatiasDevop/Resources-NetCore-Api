using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsAsync();
        Task<bool> CreatePostAsync(Post post);
        Task<Post> GetPostByIdAsync(Guid postId);

        Task<bool> UpdatePostAsync(Post postUpdate);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> UserOwnsPostAsync(Guid postId, string getUserId);
        Task<List<Tag>> GetAllTagsAsync();
        Task<bool> CreateTagAsync(Tag newTag);
        Task<Tag> GetTagByNameAsync(string tagName);
        Task<bool> DeleteTagAsync(string tagName);
    }
}

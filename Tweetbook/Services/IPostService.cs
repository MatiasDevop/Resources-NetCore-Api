using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public interface IPostService
    {
        List<Post> GetPosts();
        Post GetPostById(Guid postId);

        bool UpdatePost(Post postUpdate);
    }
}

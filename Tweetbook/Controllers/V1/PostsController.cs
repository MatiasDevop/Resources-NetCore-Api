using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Response;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetPostsAsync());
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute]Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);    //SingleOrDefault(x => x.Id == postId); this is not DI

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute]Guid postId, UpdatePostRequest request)
        {
            var userOnwsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOnwsPost)
            {
                return BadRequest(new { error = "You do not own this post"});
            }
            //refactorin this
            //var post = new Post
            //{
            //    Id= postId,
            //    Name = request.Name
            //};
            //to this
            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = request.Name;
           
            var updated = await _postService.UpdatePostAsync(post);
            
            if (updated)
            {
                return Ok(post);
            }

            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute]Guid postId)
        {
            var userOnwsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOnwsPost)
            {
                return BadRequest(new { error = "You do not own this post" });
            } 

            var deleted = await _postService.DeletePostAsync(postId);

            if (deleted)
                return NoContent();
           
            return NotFound();
        }
        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            // added a new properties tags list
              var newPostId = Guid.NewGuid();
              var post = new Post
              {
                  Id = newPostId,
                  Name = postRequest.Name,
                  UserId = HttpContext.GetUserId(),
                  Tags = postRequest.Tags.Select(x => new PostTag { PostId = newPostId, TagName = x }).ToList()
              };
          
            await _postService.CreatePostAsync(post);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

            var response = new PostResponse { Id = post.Id };
            return Created(locationUri, response);
        }
    }
}

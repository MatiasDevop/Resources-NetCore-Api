using AutoMapper;
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
        private readonly IMapper _mapper;

        public PostsController(IPostService postService, IMapper mapper)
        {
            _postService = postService;
            _mapper = mapper;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetPostsAsync();
            // we're gonna refactoring this using mappers
            //var postResponses = posts.Select(post => new PostResponse
            //{
            //    Id = post.Id,
            //    Name = post.Name,
            //    UserId = post.UserId,
            //    Tags = post.Tags.Select(x => new TagResponse { Name = x.TagName }).ToList()
            //}).ToList();
            return Ok(_mapper.Map<List<PostResponse>>(posts));
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute]Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);    //SingleOrDefault(x => x.Id == postId); this is not DI

            if (post == null)
            {
                return NotFound();
            }

            //here the same we use mappers
            //return Ok(new PostResponse
            //{
            //    Id = post.Id,
            //    Name = post.Name,
            //    Tags = post.Tags.Select(x => new TagResponse { Name = x.TagName })
            //});
            return Ok(_mapper.Map<PostResponse>(post));
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
                return Ok(_mapper.Map<PostResponse>(post));
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
            // to validate we dont need ModelSate.valid we can custimize our own validators filters
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

            //var response = new PostResponse 
            //{ 
            //    Id = post.Id,
            //    Name = post.Name,
            //    UserId = post.UserId,
            //    Tags = post.Tags.Select(x => new TagResponse { Name = x.TagName })
            //}; refactoring this using mappers
            return Created(locationUri, _mapper.Map<PostResponse>(post));
        }
    }
}

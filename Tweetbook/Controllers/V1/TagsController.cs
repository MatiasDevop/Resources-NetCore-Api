using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Response;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]//, Roles = "Admin, Poster")]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;

        public TagsController(IPostService postService, IMapper mapper)
        {
            _postService = postService;
            _mapper = mapper;
        }

        /// <summary>
        /// Return all tags in the system
        /// </summary>
        /// <response code="200">Returns all the tags in the system</response>
        [HttpGet(ApiRoutes.Tags.GetAll)]
        //[Authorize(Policy = "TagViewer")] //Require this policy to access we did it on MVCINtaller
        public async Task<IActionResult> GetAll()
        {
            var tags = await _postService.GetAllTagsAsync();
           // var tagResponses = tags.Select(tag => new TagResponse { Name = tag.Name }).ToList();refactoring with mapper
            return Ok(_mapper.Map<List<TagResponse>>(tags));
        }

        [HttpGet(ApiRoutes.Tags.Get)]
        public async Task<IActionResult> Get([FromRoute]string tagName)
        {
            var tag = await _postService.GetTagByNameAsync(tagName);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TagResponse>(tag));
        }

        /// <summary>
        /// Creates a tag in the system
        /// </summary>
        /// <response code="201">Create a tag in the system</response>
        /// <response code="400">Unable to create a tag due to validtation error</response>
        [HttpPost(ApiRoutes.Tags.Create)]
        [ProducesResponseType(typeof(TagResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest tagRequest)
        {
            var newTag = new Tag
            {
                Name = tagRequest.Name,
                CreatorId = HttpContext.GetUserId(),
               CreatedOn = DateTime.Now
            };

            var created = await _postService.CreateTagAsync(newTag);
            if (!created)
            {
                return BadRequest(new ErrorResponse {Errors = new List<ErrorModel> { new ErrorModel { Message = "Unable to create tag" }}});
            }

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{tagName}", newTag.Name);

            return Created(locationUri, _mapper.Map<TagResponse>(newTag));
        }

        [HttpDelete(ApiRoutes.Tags.Delete)]
        [Authorize(Policy = "MustWorkForChapsas")]
        // [Authorize(Roles = "Admin")] // and you can alson add , Poster or add another Authorize or you can got to the MVCINstaller and addPolicy
        public async Task<IActionResult> Delete([FromRoute] string tagName)
        {
            var deleted = await _postService.DeleteTagAsync(tagName);

            if (deleted)
                return NoContent();

            return NotFound();
        }



    }
}

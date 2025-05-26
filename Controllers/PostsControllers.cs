using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for ILogger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Data; // Assuming AppDbContext is here
using Test.Models; // Assuming Post entity is here
using Test.Models.Dto; // Assuming PostDto and ResponseDto are here

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsController> _logger; // Injected ILogger

        public PostsController(AppDbContext db, IMapper mapper, ILogger<PostsController> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/posts
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> GetPosts()
        {
            var response = new ResponseDto();
            try
            {
                _logger.LogInformation("Attempting to retrieve all posts.");
                IEnumerable<Post> posts = await _db.Posts.ToListAsync();
                response.Result = _mapper.Map<IEnumerable<PostDto>>(posts);
                _logger.LogInformation("Successfully retrieved {Count} posts.", posts.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts.");
                response.IsSuccess = false;
                response.Message = "An error occurred while fetching posts.";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            return Ok(response);
        }

        // GET: api/posts/5
        [HttpGet("{id:int}")] // Added type constraint for id
        public async Task<ActionResult<ResponseDto>> GetPost(int id)
        {
            var response = new ResponseDto();
            try
            {
                _logger.LogInformation("Attempting to retrieve post with ID: {PostId}", id);
                Post? post = await _db.Posts.FindAsync(id); // Use FindAsync for primary key lookup

                if (post == null)
                {
                    _logger.LogWarning("Post with ID: {PostId} not found.", id);
                    response.IsSuccess = false;
                    response.Message = "Post not found.";
                    return NotFound(response); // HTTP 404
                }

                response.Result = _mapper.Map<PostDto>(post);
                _logger.LogInformation("Successfully retrieved post with ID: {PostId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post with ID: {PostId}", id);
                response.IsSuccess = false;
                response.Message = "An error occurred while fetching the post.";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            return Ok(response);
        }

        // POST: api/posts
        [HttpPost]
        public async Task<ActionResult<ResponseDto>> CreatePost([FromBody] PostDto postDto)
        {
            var response = new ResponseDto();
            try
            {
                if (!ModelState.IsValid) // Basic model validation
                {
                    response.IsSuccess = false;
                    response.Message = "Validation errors occurred.";
                    response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(response);
                }

                _logger.LogInformation("Attempting to create a new post with Title: {PostTitle}", postDto.Title);
                Post post = _mapper.Map<Post>(postDto);
                post.CreatedAt = DateTime.UtcNow; // Set creation timestamp

                _db.Posts.Add(post);
                await _db.SaveChangesAsync();

                response.Result = _mapper.Map<PostDto>(post); // Return the created DTO
                response.Message = "Post created successfully.";
                _logger.LogInformation("Successfully created post with ID: {PostId}", post.Id);

                // Return 201 Created with a location header to the new resource
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, response);
            }
            catch (DbUpdateException dbEx) // More specific exception for database updates
            {
                _logger.LogError(dbEx, "Database error while creating post with Title: {PostTitle}", postDto.Title);
                response.IsSuccess = false;
                response.Message = "A database error occurred while creating the post.";
                response.ErrorMessages = new List<string> { dbEx.InnerException?.Message ?? dbEx.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post with Title: {PostTitle}", postDto.Title);
                response.IsSuccess = false;
                response.Message = "An error occurred while creating the post.";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // PUT: api/posts/5
        [HttpPut("{id:int}")] // Specify ID in the route
        public async Task<ActionResult<ResponseDto>> UpdatePost(int id, [FromBody] PostDto postDto)
        {
            var response = new ResponseDto();
            try
            {
                if (id != postDto.Id && postDto.Id != 0) // Allow postDto.Id to be 0 if not sent, or ensure it matches route id
                {
                    _logger.LogWarning("Mismatched ID in UpdatePost. Route ID: {RouteId}, Body ID: {BodyId}", id, postDto.Id);
                    response.IsSuccess = false;
                    response.Message = "ID in URL must match ID in request body if provided.";
                    return BadRequest(response);
                }

                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = "Validation errors occurred.";
                    response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(response);
                }

                _logger.LogInformation("Attempting to update post with ID: {PostId}", id);
                var existingPost = await _db.Posts.FindAsync(id);

                if (existingPost == null)
                {
                    _logger.LogWarning("Post with ID: {PostId} not found for update.", id);
                    response.IsSuccess = false;
                    response.Message = "Post not found.";
                    return NotFound(response);
                }

                // Map updated fields from DTO to the existing entity
                // CreatedAt should generally not be updated here.
                _mapper.Map(postDto, existingPost);
                existingPost.Id = id; // Ensure the ID from the route is authoritative

                _db.Entry(existingPost).State = EntityState.Modified; // Explicitly set state if not using _db.Posts.Update()
                // _db.Posts.Update(existingPost); // This also works

                await _db.SaveChangesAsync();

                response.Result = _mapper.Map<PostDto>(existingPost);
                response.Message = "Post updated successfully.";
                _logger.LogInformation("Successfully updated post with ID: {PostId}", id);
                return Ok(response);
            }
            catch (DbUpdateConcurrencyException concEx)
            {
                _logger.LogError(concEx, "Concurrency error while updating post with ID: {PostId}", id);
                response.IsSuccess = false;
                response.Message = "The post was modified by another user. Please reload and try again.";
                response.ErrorMessages = new List<string> { concEx.Message };
                return Conflict(response); // HTTP 409 Conflict
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post with ID: {PostId}", id);
                response.IsSuccess = false;
                response.Message = "An error occurred while updating the post.";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // DELETE: api/posts/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ResponseDto>> DeletePost(int id)
        {
            var response = new ResponseDto();
            try
            {
                _logger.LogInformation("Attempting to delete post with ID: {PostId}", id);
                Post? post = await _db.Posts.FindAsync(id);

                if (post == null)
                {
                    _logger.LogWarning("Post with ID: {PostId} not found for deletion.", id);
                    response.IsSuccess = false;
                    response.Message = "Post not found.";
                    return NotFound(response);
                }

                _db.Posts.Remove(post);
                await _db.SaveChangesAsync();

                response.Message = "Post deleted successfully.";
                // Typically, no body is returned for a 204, but if your ResponseDto is standard:
                // response.Result = null; // Or some confirmation
                _logger.LogInformation("Successfully deleted post with ID: {PostId}", id);

                // return NoContent(); // HTTP 204 - Ideal for DELETE if no content needs to be returned
                return Ok(response); // Or HTTP 200 with a confirmation message in ResponseDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post with ID: {PostId}", id);
                response.IsSuccess = false;
                response.Message = "An error occurred while deleting the post.";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}

using Test.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.Models;
using Test.Models.Dto;
using AutoMapper;


namespace Test.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;


        public PostsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
        }

        // GET: api/posts
        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Post> posts = _db.Posts.ToList();
                _response.Result = _mapper.Map<IEnumerable<PostDto>>(posts);


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        // GET: api/posts/5
        [HttpGet("{id}")]
        public ActionResult<ResponseDto> Get(int id)
        
        {
            try
            {
                Post post = _db.Posts.First(p => p.Id == id);
                _mapper.Map<PostDto>(post);


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        // Post: api/posts
        [HttpPost]
        public ResponseDto Post([FromBody] PostDto postDto)
        {
            try
            {
                Post post = _mapper.Map<Post>(postDto);

                _db.Posts.Add(post);
                _db.SaveChanges();
                _response.Result = _mapper.Map<PostDto>(post);           }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        // Put: api/posts
        [HttpPut]
        public ResponseDto Put([FromBody] PostDto postDto)
        {
            try
            {
                
                Post post = _mapper.Map<Post>(postDto);
                

                _db.Posts.Update(post);
                _db.SaveChanges();

                _response.Result = _mapper.Map<PostDto>(post);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        // Delete: api/posts/5
        [HttpDelete("{id}")]
        
        public ResponseDto Delete(int id)
        {
            try
            {
                 Post post = _db.Posts.First(p => p.Id == id);

               

                _db.Posts.Remove(post);
                _db.SaveChanges();

                _response.Result = post;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    
        
    }
}

using Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using PostEntity = DataAccess.Data.Post;
using CustomerEntity = DataAccess.Data.Customer;
using System.Collections.Generic;
using DataAccess.Data;
using static Dapper.SqlMapper;

namespace API.Controllers.Post
{
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private BaseService<PostEntity> PostService;
        private BaseService<CustomerEntity> CustomerService;
        public PostController(BaseService<PostEntity> postService, BaseService<CustomerEntity> customerService)
        {
            PostService = postService;
            CustomerService = customerService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await PostService.GetAll());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> Create([FromBodyAttribute]  PostEntity entity)
        {
            try
            {
                var customerExists = await CustomerService.ExistsAsync(x => x.CustomerId, entity.CustomerId);
                if (!customerExists) 
                {
                    return BadRequest(new { Mensaje = $"El usuario con ID {entity.CustomerId} no existe." });
                }

                if (entity.Body.Length > 20)
                {
                    entity.Body = entity.Body.Length > 97 ? entity.Body.Substring(0, 97) + "..." : entity.Body + "...";
                }

                switch (entity.Type)
                {
                    case 1:
                        entity.Category = "Farándula";
                        break;
                    case 2:
                        entity.Category = "Política";
                        break;
                    case 3:
                        entity.Category = "Futbol";
                        break;
                    default:
                        entity.Category = string.IsNullOrEmpty(entity.Category) ? "General" : entity.Category;
                        break;
                }

                entity.PostId = 0;
                return Ok(PostService.Create(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }

        [HttpPut()]
        public async Task<IActionResult> Update([FromBodyAttribute] PostEntity entity)
        {

            try
            {
                var result = await PostService.Update(entity.PostId, entity);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }

        [HttpDelete()]
        public async Task<IActionResult> Delete([FromBodyAttribute] PostEntity entity)
        {
            try
            {
                return Ok(await PostService.Create(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }   
        }

        [HttpPost("CreateMultiple")]
        public async Task<IActionResult> CreateMultiple([FromBody] List<PostEntity> entities)
        {
            try
            {
                var postsToCreate = new List<PostEntity>();
                foreach (var post in entities)
                {
                    var customerExists = await CustomerService.ExistsAsync(x => x.CustomerId, post.CustomerId);
                    if (!customerExists)
                    {
                        return BadRequest($"El usuario con ID {post.CustomerId} no existe, revisar los valores del post {post.Title}.");
                    }

                    if (post.Body.Length > 20)
                    {
                        post.Body = post.Body.Length > 97 ? post.Body.Substring(0, 97) + "..." : post.Body + "...";
                    }

                    switch (post.Type)
                    {
                        case 1:
                            post.Category = "Farándula";
                            break;
                        case 2:
                            post.Category = "Política";
                            break;
                        case 3:
                            post.Category = "Futbol";
                            break;
                        default:
                            post.Category = string.IsNullOrEmpty(post.Category) ? "General" : post.Category;
                            break;
                    }

                    post.PostId = 0;
                    postsToCreate.Add(post);
                }
                await PostService.CreateMultipleAsync(postsToCreate);

                return Ok(new { Message = "Post creados con exitosamente.", Posts = postsToCreate });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }
    }
}

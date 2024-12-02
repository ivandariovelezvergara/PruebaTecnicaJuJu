using Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CustomerEntity = DataAccess.Data.Customer;
using PostEntity = DataAccess.Data.Post;

namespace API.Controllers.Customer
{
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private BaseService<CustomerEntity> CustomerService;
        private BaseService<PostEntity> PostService;
        public CustomerController(BaseService<CustomerEntity> customerService, BaseService<PostEntity> postService)
        {
            CustomerService = customerService;
            PostService = postService;
        }


        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await CustomerService.GetAll());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }


        [HttpPost()]
        public async Task<IActionResult> Create([FromBodyAttribute] CustomerEntity entity)
        {
            try
            {
                entity.CustomerId = 0;
                var exists = await CustomerService.ExistsAsync(x => x.Name, entity.Name);
                if (exists)
                {
                    return BadRequest(new { Mensaje = "El nombre ya existe en la base de datos." });
                }
                else
                {
                    return Ok(CustomerService.Create(entity));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }



        /// <summary>
        /// Se corrige en las diferentes capas los siguientes puntos.
        /// 1. el tipo de valor a retornar se pide que sea un IActionResult para poder retornar respuesta http mas adecuadas
        /// 2. Se valida que la entidad sea coherente
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut()]
        public async Task<IActionResult> Update(CustomerEntity entity)
        {
            if (entity.Name == null)
            {
                return BadRequest(new { Mensaje = "Datos invalidos." });
            }

            try
            {
                var result = await CustomerService.Update(entity.CustomerId, entity);
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
        public async Task<IActionResult> Delete([FromBodyAttribute] CustomerEntity entity)
        {
            try
            {
                var listPost = await PostService.GetEntityAsync(x => x.CustomerId, entity.CustomerId);
                foreach (PostEntity postEntity in listPost)
                {
                    await PostService.Delete(postEntity);
                }
                return Ok(await CustomerService.Delete(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error: {ex.Message}");
            }
        }
    }
}

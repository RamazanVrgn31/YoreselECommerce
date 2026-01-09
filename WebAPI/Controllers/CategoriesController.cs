using System.Threading.Tasks;
using Business.Handlers.Categories.Commands;
using Business.Handlers.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    // API'nin adresi: https://localhost:port/api/categories olacak
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Dependency Injection ile Mediator'ı içeri alıyoruz
        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET isteği: api/categories/getall
        [HttpGet("getall")]
        public async Task <IActionResult> GetAll() 
        {
            // 1. Business katmanındaki GetCategoriesQuery mesajını oluştur.
            // 2. Mediator ile gönder.
            // 3. Gelen cevabı (result) al.

            var result = await _mediator.Send(new GetCategoriesQuery());

            // İşlem başarılıysa 200 OK ve veriyi dön
            if ( result.Success)
            {
                  return Ok(result.Data);
            }

            // Hata varsa 400 Bad Request ve mesajı dön
            return BadRequest(result.Message);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CreateCategoryCommand createCategoryCommand)
        {
            var result = await _mediator.Send(createCategoryCommand);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryCommand updateCategoryCommand)
        {
            var result = await _mediator.Send(updateCategoryCommand);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteCategoryCommand deleteCategoryCommand)
        {
            var result = await _mediator.Send(deleteCategoryCommand);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }
    }
}

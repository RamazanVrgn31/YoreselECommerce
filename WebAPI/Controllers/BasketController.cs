using Business.Abstract;
using Core.Extensions;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpGet("get")]
        public IActionResult GetBasket()
        {
            // Token'dan UserId'yi okuyoruz (Extensions klasöründeki ClaimExtensions sayesinde)
            // Eğer test yaparken Token yoksa, elle bir ID string'i verebilirsin şimdilik.
             string userId = User.GetUserId().ToString();

            var result = _basketService.GetBasket(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult UpdateBasket([FromBody] BasketDto basketDto)
        {
            string userId = User.GetUserId().ToString();
            var result = _basketService.UpdateBasket(userId, basketDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("delete")]
        public IActionResult DeleteBasket()
        {
            string userId = User.GetUserId().ToString();
            var result = _basketService.DeleteBasket(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("additem")]
        public IActionResult AddItemToBasket([FromBody] BasketItemDto basketItemDto)
        {
            string userId = User.GetUserId().ToString();
            var result = _basketService.AddItemToBasket(userId, basketItemDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}

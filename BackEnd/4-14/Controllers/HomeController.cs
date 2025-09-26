using AllEnBackend.Models;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet("home/cards/{userId}")]
        public async Task<IActionResult> GetHomeCards(string userId)
        {
            try
            {
                var data = await _homeService.GetHomeCardsAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}

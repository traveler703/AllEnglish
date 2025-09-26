using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/ads")]
    public class AdvertisementController : ControllerBase
    {
        private readonly IAdvertisementService _adService;

        public AdvertisementController(IAdvertisementService adService)
        {
            _adService = adService;
        }

        // ��ȡ�ɹ������Դ�б�
        [HttpGet("available-ad")]
        public async Task<IActionResult> GetAvailableAd()
        {
            var Ad = await _adService.GetAvailableAdAsync();
            return Ok(new { Ad });
        }

        // ��ȡ���е���Դ
        [HttpGet("available-material")]
        public async Task<IActionResult> GetMaterial(string id)
        {
            var material = await _adService.GetMaterial(id);
            return Ok(new { material });
        }

    }
}
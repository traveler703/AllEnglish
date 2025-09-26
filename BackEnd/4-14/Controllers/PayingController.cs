using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/paying")]
    public class PayingController : ControllerBase
    {
        private readonly IPayingService _payingService;

        public PayingController(IPayingService payingService)
        {
            _payingService = payingService;
        }

        // 每日签到
        [HttpPost("daily-signin")]
        public async Task<IActionResult> DailySignIn([FromBody] SignInRequestDto request)
        {
            var result = await _payingService.DailySignInAsync(request.UserId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, coin = result.Coin });
            }

            return Ok(new { message = result.Message, coin = result.Coin });
        }

        [HttpGet("coin-balance")]
        public async Task<IActionResult> GetCoinBalance([FromQuery] string userId)
        {
            var coin = await _payingService.GetUserCoinAsync(userId);
            return Ok(new { coin });
        }

        // 购买资源
        [HttpPost("purchase-material")]
        public async Task<IActionResult> PurchaseMaterial([FromBody] PurchaseRequestDto request)
        {
            try
            {
                var result = await _payingService.PurchaseMaterialAsync(request.UserId, request.MaterialId);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message, remainingCoins = result.RemainingCoins });
                }

                return Ok(new { message = result.Message, remainingCoins = result.RemainingCoins, orderId = result.OrderId });
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                Console.WriteLine($"购买失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = $"购买失败: {ex.Message}", 
                    error = ex.StackTrace 
                });
            }
        }

        // 获取用户库存
        [HttpGet("user-inventory")]
        public async Task<IActionResult> GetUserInventory([FromQuery] string userId)
        {
            var inventory = await _payingService.GetUserInventoryAsync(userId);
            return Ok(new { inventory });
        }

        // 检查资源是否在库存中
        [HttpGet("check-inventory")]
        public async Task<IActionResult> CheckMaterialInInventory([FromQuery] string userId, [FromQuery] string materialId)
        {
            var isInInventory = await _payingService.IsMaterialInInventoryAsync(userId, materialId);
            return Ok(new { isInInventory });
        }

        // 获取可购买的资源列表
        [HttpGet("available-materials")]
        public async Task<IActionResult> GetAvailableMaterials()
        {
            var materials = await _payingService.GetAvailableMaterialsAsync();
            return Ok(new { materials });
        }

        [HttpGet("category")]
        public async Task<IActionResult> GetUserCategory([FromQuery] string userId)
        {
            var category = await _payingService.GetUserCategoryAsync(userId);
            if (category == null)
            {
                return NotFound("用户类型未找到");
            }

            return Ok(new { category });
        }

        [HttpGet("user-reinstate")]
        public async Task<IActionResult> UserReinstate([FromQuery] string userId)
        {
            var category = await _payingService.ISUserReinstate(userId);
            if (category == null)
            {
                return NotFound("用户类型未找到");
            }

            return Ok(new { category });
        }
    }
}

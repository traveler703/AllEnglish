using System.IO;
using AllEnBackend.Services;
using AllEnBackend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IArticleImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IArticleImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        [HttpPost("articles/seed")]
        public async Task<IActionResult> ImportSeedData()
        {
            try
            {
                var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "articles.json");
                var importedCount = await _importService.ImportArticlesFromJsonAsync(seedDataPath);

                return Ok(new ImportResponse
                {
                    Success = true,
                    ImportedCount = importedCount,
                    Message = $"成功导入种子数据 {importedCount} 篇文章"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入种子数据时发生错误");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"导入失败: {ex.Message}"
                });
            }
        }

        //通过 Swagger 粘贴 JSON 字符串导入
        [HttpPost("articles/json")]
        public async Task<IActionResult> ImportFromJson([FromBody] JsonElement json)
        {
            try
            {
                string jsonContent = json.GetRawText();
                var importedCount = await _importService.ImportArticlesFromJsonStringAsync(jsonContent);

                return Ok(new ImportResponse
                {
                    Success = true,
                    ImportedCount = importedCount,
                    Message = $"成功导入 {importedCount} 篇文章（来自 JSON）"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过 JSON 字符串导入文章失败");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"导入失败: {ex.Message}"
                });
            }
        }
        //支持文件上传（.json 文件）
        [HttpPost("articles/upload")]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ImportResponse
                {
                    Success = false,
                    Message = "请上传一个有效的 JSON 文件"
                });
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                string jsonContent = await reader.ReadToEndAsync();
                var importedCount = await _importService.ImportArticlesFromJsonStringAsync(jsonContent);

                return Ok(new ImportResponse
                {
                    Success = true,
                    ImportedCount = importedCount,
                    Message = $"成功导入 {importedCount} 篇文章（来自上传文件）"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件导入文章失败");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"导入失败: {ex.Message}"
                });
            }
        }


    }
}

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
                    Message = $"�ɹ������������� {importedCount} ƪ����"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������������ʱ��������");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"����ʧ��: {ex.Message}"
                });
            }
        }

        //ͨ�� Swagger ճ�� JSON �ַ�������
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
                    Message = $"�ɹ����� {importedCount} ƪ���£����� JSON��"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ͨ�� JSON �ַ�����������ʧ��");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"����ʧ��: {ex.Message}"
                });
            }
        }
        //֧���ļ��ϴ���.json �ļ���
        [HttpPost("articles/upload")]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ImportResponse
                {
                    Success = false,
                    Message = "���ϴ�һ����Ч�� JSON �ļ�"
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
                    Message = $"�ɹ����� {importedCount} ƪ���£������ϴ��ļ���"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�ϴ��ļ���������ʧ��");
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = $"����ʧ��: {ex.Message}"
                });
            }
        }


    }
}

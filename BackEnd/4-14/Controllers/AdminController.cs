using AllEnBackend.Models;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    /* URL: https://localhost:7071/api/admin/action */
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // 获取所有用户
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _adminService.SearchUserAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                // 可记录日志 ex.Message
                return StatusCode(500, "服务器内部错误: " + ex.Message);
            }
        }

        // 更新用户信息
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "用户ID不能为空" });
            }

            try
            {
                // 创建用户对象并填充数据
                var user = new User
                {
                    Id = id,
                    UserName = request.UserName,
                    Email = request.Email,
                    Category = request.Category,
                };

                var result = await _adminService.UpdateUserAsync(user);

                if (result)
                {
                    return Ok(new { message = "用户信息更新成功" });
                }

                return BadRequest(new { message = "更新用户信息失败" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            }

            // 删除用户
            [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(email);
                if (!result)
                    return BadRequest("删除失败");

                return Ok(new { message = "用户已删除" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "服务器内部错误: " + ex.Message);
            }
        }

        // 新增文件上传端点
        [HttpPost("upload-material-file")]
        public async Task<IActionResult> UploadMaterialFile()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("未上传文件");

                // 创建保存目录
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "materials");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 生成唯一文件名
                string fileExt = Path.GetExtension(file.FileName);
                string newFileName = $"{Guid.NewGuid()}{fileExt}";
                string filePath = Path.Combine(uploadsFolder, newFileName);

                // 保存文件
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 返回相对路径
                string relativePath = $"/uploads/materials/{newFileName}";
                return Ok(new { url = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"文件上传失败: {ex.Message}");
            }
        }


        // 上传视频/文章接口
        [HttpPost("material/upload")]
        public async Task<IActionResult> UploadMaterial([FromForm] MaterialUploadRequest request)
        {
            try
            {
                // 验证URL字段
                if (string.IsNullOrEmpty(request.fileUrl))
                    return BadRequest("文件URL不能为空");
                // 验证材料类型
                if (request.MaterialType != "视频" && request.MaterialType != "文章")
                    return BadRequest("材料类型必须是'视频'或'文章'");

                if (request.SkillType != "听力" && request.SkillType != "口语"
                    && request.SkillType != "阅读" && request.SkillType != "写作")
                    return BadRequest("上传材料skilltype不对");

                if (request.ExamType != "CET-4" && request.ExamType != "CET-6"
                    && request.ExamType != "托福")
                    return BadRequest("上传材料examtype不对");

                var material = new Material
                {
                    MaterialType = request.MaterialType,
                    ExamType = request.ExamType,
                    SkillType = request.SkillType,
                    Price = request.Price,
                    Description = request.Description,
                    PreviewUrl = request.PreviewUrl,
                    Url = request.fileUrl, 
                    IsActive = 1
                    
                };

                await _adminService.SaveAsync(material);

                return Ok(new
                {
                    id = material.Id,
                    url = material.Url,
                    previewUrl = material.PreviewUrl,
                    message = "材料上传成功"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 删除材料接口
        [HttpDelete("material/{id}")]
        public async Task<IActionResult> DeleteMaterial(string id)
        {
            try
            {
                // 删除服务器上的文件
                var material = await _adminService.DeleteMatAsync(id);
                if (material == false)
                    return NotFound("材料不存在");

                return Ok("材料删除成功");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 获取所有课程接口
        [HttpGet("material/list")]
        public async Task<IActionResult> GetMaterials()
        {
            try
            {
                var materials = await _adminService.GetAllMaterialsAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 更新课程状态接口
        [HttpPatch("material/{id}/status")]
        public async Task<IActionResult> UpdateMaterialStatus(string id)
        {
            try
            {
                var result = await _adminService.UpdateMaterialStatusAsync(id);
                if (!result)
                    return NotFound("课程不存在");

                return Ok("课程状态更新成功");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        [HttpPost("upload-preview-image")]
        public async Task<IActionResult> UploadPreviewImage()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("未上传文件");

                // 创建保存目录（预览图）
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "previews");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 生成唯一文件名
                string fileExt = Path.GetExtension(file.FileName);
                string newFileName = $"{Guid.NewGuid()}{fileExt}";
                string filePath = Path.Combine(uploadsFolder, newFileName);

                // 保存文件
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 返回相对路径
                string relativePath = $"/uploads/previews/{newFileName}";
                return Ok(new { url = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"预览图上传失败: {ex.Message}");
            }
        }

        // 获取广告列表
        [HttpGet("ads/list")]
        public async Task<IActionResult> GetAds()
        {
            try
            {
                var ads = await _adminService.GetAllAdsAsync();
                return Ok(ads);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 新增广告
        public class SaveAdRequest
        {
            public string? Id { get; set; }
            public string MediaUrl { get; set; } = string.Empty;
            public string TargetId { get; set; } = string.Empty;
            public string Context { get; set; } = string.Empty;
            public int Status { get; set; } = 1;
        }

        [HttpPost("ads")]
        public async Task<IActionResult> SaveAd([FromBody] SaveAdRequest req)
        {
            try
            {
                var ad = new Advertisement
                {
                    Id = req.Id ?? string.Empty,
                    MediaUrl = req.MediaUrl,
                    TargetId = req.TargetId,
                    Context = req.Context,
                    Status = req.Status
                };
                var ok = await _adminService.SaveAdAsync(ad);
                if (!ok) return BadRequest("保存失败");
                return Ok(new { message = "保存成功", id = ad.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 切换广告状态（上下架）
        [HttpPatch("ads/{id}/status")]
        public async Task<IActionResult> UpdateAdStatus(string id)
        {
            try
            {
                var ok = await _adminService.UpdateAdStatusAsync(id);
                if (!ok) return NotFound("广告不存在");
                return Ok("广告状态更新成功");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        // 删除广告
        [HttpDelete("ads/{id}")]
        public async Task<IActionResult> DeleteAd(string id)
        {
            try
            {
                var ok = await _adminService.DeleteAdAsync(id);
                if (!ok) return NotFound("广告不存在");
                return Ok("广告删除成功");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        //上传广告媒体图片/视频
        [HttpPost("upload-ad-media")]
        public async Task<IActionResult> UploadAdMedia()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("未上传文件");

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileExt = Path.GetExtension(file.FileName);
                string newFileName = $"{Guid.NewGuid()}{fileExt}";
                string filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string relativePath = $"/uploads/ads/{newFileName}";
                return Ok(new { url = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"文件上传失败: {ex.Message}");
            }
        }


    }

}

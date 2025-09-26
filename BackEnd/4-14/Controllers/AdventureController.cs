using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdventureController : ControllerBase
    {
        private readonly IAdventureService _service;

        public AdventureController(IAdventureService service)
        {
            _service = service;
        }

        // ��ȡ����ð��
        [HttpGet]
        public async Task<ActionResult<List<AdventureDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ����ð��ID��ȡð������
        [HttpGet("{id}")]
        public async Task<ActionResult<AdventureDto>> Get(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // ������ð��
        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] CreateAdventureDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(id);
        }

        // ����ð����Ϣ
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateAdventureDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        // ɾ��ð��
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
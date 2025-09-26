// AdventureTreasureController.cs
using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdventureTreasureController : ControllerBase
    {
        private readonly IAdventureTreasureService _service;

        public AdventureTreasureController(IAdventureTreasureService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var treasures = await _service.GetAllAsync();
            return Ok(treasures);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var treasure = await _service.GetByIdAsync(id);
            return treasure == null ? NotFound() : Ok(treasure);
        }

        [HttpGet("level/{level}")]
        public async Task<IActionResult> GetByLevel(int level)
        {
            var treasures = await _service.GetByLevelAsync(level);
            return Ok(treasures);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdventureTreasureDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateAdventureTreasureDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
using Library.DTOs;
using Library.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository _repository; 

        public GenreController(IGenreRepository repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeEmpty = false)
        {
            return Ok(await _repository.GetAllAsync(includeEmpty));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid genre ID");

            var genre = await _repository.GetByIdAsync(id);
            if (genre == null) return NotFound();

            return Ok(genre);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateGenreDTO genre)
        {
            if (genre == null) return BadRequest("Genre is required");

            if (await _repository.ExistsAsync(genre.Name))
                return Conflict("Genre already exists");

            var created = await _repository.CreateAsync(genre);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}

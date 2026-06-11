using Library.DTOs;
using Library.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorRepository _repository;

        public AuthorController(IAuthorRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid author ID");

            var author = await _repository.GetByIdAsync(id);
            if (author == null) return NotFound();

            return Ok(author);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAuthorDTO author)
        {
            if (author == null) return BadRequest("Author is required");

            if (await _repository.ExistsAsync(author.Name, author.Surname))
                return Conflict("Author already exists");

            var created = await _repository.CreateAsync(author);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}

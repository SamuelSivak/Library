using Library.DTOs;
using Library.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerController : ControllerBase
    {
        private readonly IReviewerRepository _repository;

        public ReviewerController(IReviewerRepository repository)
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
            if (id <= 0) return BadRequest("Invalid reviewer ID");

            var reviewer = await _repository.GetByIdAsync(id);
            if (reviewer == null) return NotFound();

            return Ok(reviewer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReviewerDTO reviewer)
        {
            if (reviewer == null) return BadRequest("Reviewer is required");

            if (await _repository.ExistsAsync(reviewer.Name))
                return Conflict("Reviewer already exists");

            var created = await _repository.CreateAsync(reviewer);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}

using Hangfire;
using Library.DTOs;
using Library.Repositories;
using Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _repository;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BookController(IBookRepository repository, IBackgroundJobClient backgroundJobClient)
        {
            _repository = repository;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var books = await _repository.GetAllAsync(search, genre, page, pageSize, sortBy);
            var totalCount = await _repository.GetTotalCountAsync(search, genre);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count");

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid book ID");

            var book = await _repository.GetByIdAsync(id);
            if (book == null) return NotFound();

            _backgroundJobClient.Enqueue<IBookAnalyticsService>(x => x.IncrementViewsAsync(id));

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookDTO book)
        {
            if (book == null) return BadRequest("Book is required");

            if (await _repository.ExistsAsync(book.Title, book.AuthorId))
                return Conflict("Book already exists");

            var created = await _repository.CreateAsync(book);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDTO book)
        {
            if (id <= 0) return BadRequest("Invalid book ID");
            if (book == null) return BadRequest("Book data is required");

            var updated = await _repository.UpdateAsync(id, book);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid book ID");

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpGet("/api/books/top-rated")]
        public async Task<IActionResult> GetTopRated([FromQuery] int limit = 10)
        {
            if (limit <= 0 || limit > 100) limit = 10;
            return Ok(await _repository.GetTopRatedAsync(limit));
        }
    }
}



using Library.DTOs;
using Library.Models;
using Library.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryRepository _repository; 

        public CountryController(ICountryRepository repository)
        {
            _repository = repository;
        }

        // 🤓: Api stuff 🤓
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok( await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid country ID");

            var country = await _repository.GetByIdAsync(id);
            if (country == null) return NotFound();

            return Ok(country);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCountryDTO country)
        {
            if (await _repository.ExistsAsync(country.Name))
                return Conflict("Country exist, you are not special ");

            var created = await _repository.CreateAsync(country);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

    }
}

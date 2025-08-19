using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.UseCases;
using MyApp.Domain.Entities;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly CreatePerson _createPerson;
        private readonly GetPersonById _getPersonById;

        public PersonsController(CreatePerson createPerson, GetPersonById getPersonById)
        {
            _createPerson = createPerson;
            _getPersonById = getPersonById;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Person>> GetById(Guid id)
        {
            var person = await _getPersonById.ExecuteAsync(id);
            if (person == null)
                return NotFound();
            return Ok(person);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Person>> GetAll()
        {
            return new List<Person>();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PersonDto dto)
        {
            await _createPerson.ExecuteAsync(dto);
            return Created();
        }
    }
}

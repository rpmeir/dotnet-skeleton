using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.UseCases;
using MyApp.Domain.Entities;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsQueryController(GetPersonById getPersonById) : ControllerBase
    {
        private readonly GetPersonById _getPersonById = getPersonById;

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
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PersonsCommandController(CreatePerson createPerson) : ControllerBase
    {
        private readonly CreatePerson _createPerson = createPerson;

        [HttpPost]
        public async Task<IActionResult> Create(PersonDto dto)
        {
            await _createPerson.ExecuteAsync(dto);
            return Created();
        }
    }
}

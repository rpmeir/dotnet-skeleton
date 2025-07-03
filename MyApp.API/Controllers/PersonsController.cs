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

        public PersonsController(CreatePerson createPerson)
        {
            _createPerson = createPerson;
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

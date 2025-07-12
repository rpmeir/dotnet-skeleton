using System;
using MyApp.Application.DTOs;
using MyApp.Application.Repository;
using MyApp.Domain.Entities;

namespace MyApp.Application.UseCases;

public class CreatePerson
{
    private readonly IPersonRepository _repo;

    public CreatePerson(IPersonRepository repo)
    {
        _repo = repo;
    }

    public async Task ExecuteAsync(PersonDto dto)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            BirthDate = dto.BirthDate
        };
        await _repo.AddAsync(person);
    }
}

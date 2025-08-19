using System;
using System.Threading.Tasks;
using MyApp.Application.Repository;
using MyApp.Domain.Entities;

namespace MyApp.Application.UseCases;

public class GetPersonById
{
    private readonly IPersonRepository _repo;

    public GetPersonById(IPersonRepository repo)
    {
        _repo = repo;
    }

    public async Task<Person?> ExecuteAsync(Guid id)
    {
        return await _repo.GetByIdAsync(id);
    }
}

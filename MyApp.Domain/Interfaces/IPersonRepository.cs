using System;
using MyApp.Domain.Entities;

namespace MyApp.Domain.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<List<Person>> GetAllAsync();
    Task AddAsync(Person person);
}

using MyApp.Domain.Entities;

namespace MyApp.Application.Repository;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<List<Person>> GetAllAsync();
    Task AddAsync(Person person);
}

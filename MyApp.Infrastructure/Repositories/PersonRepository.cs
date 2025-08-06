using System;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Repository;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Infrastructure.Repositories;

public class PersonRepository(AppDbContext context) : IPersonRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddAsync(Person person) => await _context.Persons.AddAsync(person);
    public async Task<List<Person>> GetAllAsync() => await _context.Persons.ToListAsync();
    public async Task<Person?> GetByIdAsync(Guid id) => await _context.Persons.FindAsync(id);
}

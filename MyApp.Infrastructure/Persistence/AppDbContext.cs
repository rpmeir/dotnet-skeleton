using System;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;

namespace MyApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
}

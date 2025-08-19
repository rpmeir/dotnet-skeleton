using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Application.UseCases;
using MyApp.Infrastructure.Persistence;
using MyApp.Infrastructure.Repositories;

namespace MyApp.Tests.Application.UseCases;

[TestClass]
public class CreatePersonTest
{
    private AppDbContext _context = null!;
    private PersonRepository _repo = null!;
    private CreatePerson _useCase = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
    }

    [TestMethod]
    public async Task ShouldCreatePerson()
    {
        _repo = new PersonRepository(_context);
        _useCase = new CreatePerson(_repo);

        var dto = new PersonDto
        {
            Name = "John Doe",
            BirthDate = new DateOnly(1990, 1, 1)
        };

        await _useCase.ExecuteAsync(dto);
        await _context.SaveChangesAsync();

        var persons = await _repo.GetAllAsync();
        Assert.AreEqual(1, persons.Count);
        Assert.AreEqual("John Doe", persons[0].Name);
        Assert.AreEqual(new DateOnly(1990, 1, 1), persons[0].BirthDate);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
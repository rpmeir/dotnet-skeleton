using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;
using MyApp.Infrastructure.Repositories;
using MyApp.Application.UseCases;

namespace MyApp.Tests.Application.UseCases;

[TestClass]
public class GetPersonByIdTest
{
    private AppDbContext _context = null!;
    private PersonRepository _repo = null!;
    private GetPersonById _getPersonById = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _context = new AppDbContext(options);
        _repo = new PersonRepository(_context);
        _getPersonById = new GetPersonById(_repo);
    }

    [TestMethod]
    public async Task ExecuteAsync_ReturnsPerson_WhenPersonExists()
    {
        var personId = Guid.NewGuid();
        var person = new Person { Id = personId, Name = "John Doe", BirthDate = new DateOnly(1990, 1, 1) };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        var result = await _getPersonById.ExecuteAsync(personId);

        Assert.IsNotNull(result);
        Assert.AreEqual(personId, result!.Id);
        Assert.AreEqual("John Doe", result.Name);
        Assert.AreEqual(new DateOnly(1990, 1, 1), result.BirthDate);
    }

    [TestMethod]
    public async Task ExecuteAsync_ReturnsNull_WhenPersonDoesNotExist()
    {
        var personId = Guid.NewGuid();
        var result = await _getPersonById.ExecuteAsync(personId);
        Assert.IsNull(result);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

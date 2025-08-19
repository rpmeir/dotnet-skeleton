## Step-by-step dotnet project

---

## 🧩 Solution Overview

We’ll create a solution with these **decoupled projects**:

| Project Name         | Responsibility                              |
|----------------------|----------------------------------------------|
| `MyApp.Domain`       | Domain entities, interfaces (pure DDD)       |
| `MyApp.Application`  | Application logic, use cases (hexagonal core)|
| `MyApp.Infrastructure`| DB access (PostgreSQL), external services    |
| `MyApp.API`          | Web API, controllers                         |
| `MyApp.Tests`        | Unit tests                                   |

---

## 🛠 Step-by-Step Guide

### 1. 🧱 Create the Solution

```bash
dotnet new sln -n MyApp
dotnet new classlib -n MyApp.Domain
dotnet new classlib -n MyApp.Application
dotnet new classlib -n MyApp.Infrastructure
dotnet new webapi -n MyApp.API
dotnet new mstest -n MyApp.Tests
dotnet sln add MyApp.Domain
dotnet sln add MyApp.Application
dotnet sln add MyApp.Infrastructure
dotnet sln add MyApp.API
dotnet sln add MyApp.Tests
```

---

### 2. 📦 Setup Domain Layer (`MyApp.Domain`)

```csharp
// Entities/Person.cs
public class Person {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}

// Interfaces/IPersonRepository.cs
public interface IPersonRepository {
    Task<Person> GetByIdAsync(Guid id);
    Task<List<Person>> GetAllAsync();
    Task AddAsync(Person person);
}
```

Keep it clean: no EF Core, no implementation details here.

---

### 3. 🔁 Application Layer (`MyApp.Application`)

Create DTOs

```csharp
// DTOs/PersonDto.cs
public class PersonDto {
    public string Name { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}
```

Create IPersonRepository interface:
```csharp
// Repository/IPersonRepository.cs
using MyApp.Domain.Entities;

namespace MyApp.Application.Repository;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<List<Person>> GetAllAsync();
    Task AddAsync(Person person);
}
```

Create CreatePerson use case:

```csharp
// UseCases/CreatePerson.cs
public class CreatePerson {
    private readonly IPersonRepository _repo;

    public CreatePerson(IPersonRepository repo) {
        _repo = repo;
    }

    public async Task ExecuteAsync(PersonDto dto) {
        var person = new Person { Id = Guid.NewGuid(), Name = dto.Name, BirthDate = dto.BirthDate };
        await _repo.AddAsync(person);
    }
}
```

Create GetPersonById use case:

```csharp
// UseCases/GetPersonById.cs
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
```

Inject interfaces, not implementations 👌

---

### 4. 🧩 Infrastructure Layer (`MyApp.Infrastructure`)

Use Entity Framework Core for Postgres:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Create DbContext and implementation:

```csharp
// Persistence/AppDbContext.cs
public class AppDbContext : DbContext {
    public DbSet<Person> Persons { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
}

// Repositories/PersonRepository.cs
public class PersonRepository : IPersonRepository {
    private readonly AppDbContext _context;
    public PersonRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Person person) => await _context.Persons.AddAsync(person);
    public async Task<List<Person>> GetAllAsync() => await _context.Persons.ToListAsync();
    public async Task<Person> GetByIdAsync(Guid id) => await _context.Persons.FindAsync(id);
}
```

---

### 5. 🌐 API Layer (`MyApp.API`)

Configure DI and endpoints:

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Repository;
using MyApp.Application.UseCases;
using MyApp.Infrastructure.Persistence;
using MyApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<CreatePerson>();
builder.Services.AddScoped<GetPersonById>();

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

```

Create a controller:

```csharp
// Controllers/PersonController.cs
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

```

---

### 6. 🧪 Add Unit Tests (`MyApp.Tests`)

This sets up a project ready to use MSTest attributes like `[TestMethod]`.

Make sure your test project can access the application, domain and infrastructure layers:

```bash
dotnet add MyApp.Tests reference ../MyApp.Application
dotnet add MyApp.Tests reference ../MyApp.Domain
dotnet add MyApp.Tests reference ../MyApp.Infrastructure
```

Add the required MSTest packages if needed:

```bash
dotnet add MyApp.Tests package Microsoft.NET.Test.Sdk
dotnet add MyApp.Tests package MSTest.TestAdapter
dotnet add MyApp.Tests package MSTest.TestFramework
dotnet add MyApp.Tests package Moq
dotnet add MyApp.Tests package Microsoft.EntityFrameworkCore.InMemory --version 8.0.19
dotnet add MyApp.Tests package Microsoft.AspNetCore.Mvc.Testing --version 8.0.19
```

---

Here’s how you’d create the MSTest classes:

Create the CreatePersonTest class:
```csharp
// Application/UseCases/CreatePersonTest.cs
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
    private CreatePerson _createPerson = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
        _repo = new PersonRepository(_context);
        _createPerson = new CreatePerson(_repo);
    }

    [TestMethod]
    public async Task ShouldCreatePerson()
    {
        var dto = new PersonDto
        {
            Name = "John Doe",
            BirthDate = new DateOnly(1990, 1, 1)
        };

        await _createPerson.ExecuteAsync(dto);
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
```

Create the GetPersonByIdTest test class:
```csharp
// Application/UseCases/GetPersonByIdTest.cs
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
```


Everything works the same conceptually—just with `[TestClass]` and `[TestMethod]`.

---

## 🧼 Good Practice Notes

- Use **`TestCategory`** attribute if you want to group tests.
- Stick with **naming conventions** like `MethodName_ExpectedBehavior_Scenario` for clarity.
- Keep tests **fast**, **focused**, and **isolated**—especially in a clean architecture setup.


---

## 🎯 Final Thoughts

- ✅ Use Dependency Injection everywhere.
- 🧼 Follow SOLID principles: keep responsibilities clear.
- 🪟 Use DTOs to avoid leaking domain details to the outer world.
- 🧪 Use mockable interfaces to isolate tests.

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

Create use cases and DTOs:

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

// DTOs/PersonDto.cs
public class PersonDto {
    public string Name { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
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
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<CreatePerson>();
```

Create a controller:

```csharp
// Controllers/PersonController.cs
[ApiController]
[Route("api/person")]
public class PersonController : ControllerBase {
    private readonly CreatePerson _createPerson;

    public PersonController(CreatePerson createPerson) {
        _createPerson = createPerson;
    }

    [HttpPost]
    public async Task<IActionResult> Create(PersonDto dto) {
        await _createPerson.ExecuteAsync(dto);
        return Ok();
    }
}
```

---

### 6. 🧪 Add Unit Tests (`MyApp.Tests`)

This sets up a project ready to use MSTest attributes like `[TestMethod]`.

Make sure your test project can access the application and domain layers:

```bash
dotnet add MyApp.Tests reference ../MyApp.Application
dotnet add MyApp.Tests reference ../MyApp.Domain
```

Add the required MSTest packages if needed:

```bash
dotnet add MyApp.Tests package Microsoft.NET.Test.Sdk
dotnet add MyApp.Tests package MSTest.TestAdapter
dotnet add MyApp.Tests package MSTest.TestFramework
dotnet add MyApp.Tests package Moq
```

---

Here’s how you’d create a MSTest class:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyApp.Domain;
using MyApp.Application;
using System;
using System.Threading.Tasks;

namespace MyApp.Tests;

[TestClass]
public class CreatePersonTests {
    [TestMethod]
    public async Task ShouldCreatePerson() {
        var mockRepo = new Mock<IPersonRepository>();
        var useCase = new CreatePerson(mockRepo.Object);
        var dto = new PersonDto { Name = "Alice", BirthDate = new DateOnly(1990, 1, 1) };

        await useCase.ExecuteAsync(dto);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Once);
    }
}
```

Everything works the same conceptually—just with `[TestClass]` and `[TestMethod]` instead of `[Fact]`.

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

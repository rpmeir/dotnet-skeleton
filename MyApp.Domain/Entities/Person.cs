using System;

namespace MyApp.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}

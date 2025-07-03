using System;

namespace MyApp.Application.DTOs;

public class PersonDto
{
    public string Name { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}

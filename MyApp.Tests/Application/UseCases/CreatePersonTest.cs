using Moq;
using MyApp.Application.DTOs;
using MyApp.Application.Repository;
using MyApp.Application.UseCases;

namespace MyApp.Tests.Application.UseCases;

[TestClass]
public class CreatePersonTest
{
    [TestMethod]
    public async Task ShouldCreatePerson()
    {
        var mockRepo = new Mock<IPersonRepository>();
        var useCase = new CreatePerson(mockRepo.Object);
        var dto = new PersonDto
        {
            Name = "John Doe",
            BirthDate = new DateOnly(1990, 1, 1)
        };

        await useCase.ExecuteAsync(dto);

        mockRepo.Verify(repo => repo.AddAsync(It.IsAny<MyApp.Domain.Entities.Person>()), Times.Once);
        mockRepo.VerifyNoOtherCalls();
    }
}

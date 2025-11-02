namespace readytohelpapi.Occurrence.Tests;

using Xunit;
using readytohelpapi.Common.Data;
using readytohelpapi.Occurrence.Services;
using readytohelpapi.Occurrence.Controllers;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using readytohelpapi.ResponsibleEntity.Services;
using System.Security.Claims;
using Moq;
using readytohelpapi.Occurrence.DTOs;

/// <summary>
///   Integration tests for the <see cref="OccurrenceApiController"/>.
/// </summary>
[Trait("Category", "Integration")]
public class TestOccurrenceApiController_Integration : IClassFixture<DbFixture>
{
    private readonly AppDbContext context;
    private readonly IOccurrenceRepository repo;
    private readonly IOccurrenceService service;
    private readonly OccurrenceApiController controller;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TestOccurrenceApiController_Integration"/> class.
    /// </summary>
    /// <param name="fixture">The database fixture.</param>
    public TestOccurrenceApiController_Integration(DbFixture fixture)
    {
        context = fixture.Context;
        fixture.ResetDatabase();

        repo = new OccurrenceRepository(context);

        var seededRe = new ResponsibleEntity
        {
            Name = "Integration Entity",
            Email = "entity@integration.com",
            Address = "Rua Teste",
            ContactPhone = 123456789,
            Type = ResponsibleEntityType.INEM
        };
        context.ResponsibleEntities.Add(seededRe);
        context.SaveChanges();

        var responsibleMock = new Mock<IResponsibleEntityService>();
        responsibleMock.Setup(r => r.FindResponsibleEntity(
            It.IsAny<OccurrenceType>(),
            It.IsAny<double>(),
            It.IsAny<double>()
        )).Returns(seededRe);

        service = new OccurrenceServiceImpl(repo, responsibleMock.Object);
        controller = new OccurrenceApiController(service);
    }

    private Occurrence BuildOccurrence()
    {
        return new Occurrence
        {
            Title = "Test O",
            Description = "Integration test occurrence",
            Type = OccurrenceType.FLOOD,
            Priority = PriorityLevel.HIGH,
            Location = new GeoPoint.Models.GeoPoint
            {
                Latitude = 40.0,
                Longitude = -8.0
            },
            ReportCount = 1,
            CreationDateTime = DateTime.UtcNow,
            Status = OccurrenceStatus.WAITING,
            ProximityRadius = 50
        };
    }

    [Fact]
    public void Create_SavesToDatabase()
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, "ADMIN"),
        new Claim(ClaimTypes.NameIdentifier, "1")
    };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var input = BuildOccurrence();

        var result = controller.Create(input);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var obj = Assert.IsType<Occurrence>(created.Value);

        Assert.True(obj.Id > 0);
        Assert.True(context.Occurrences.Any(o => o.Id == obj.Id));
    }


    [Fact]
    public void GetById_ReturnsCorrectOccurrence()
    {
        var occ = repo.Create(BuildOccurrence());

        var result = controller.GetById(occ.Id);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<OccurrenceDetailsDto>(ok.Value);

        Assert.Equal(occ.Id, dto.Id);
        Assert.Equal(occ.Title, dto.Title);
    }

    [Fact]
    public void Update_ChangesPersist()
    {
        var occ = repo.Create(BuildOccurrence());
        occ.Title = "Atualizado";

        var result = controller.Update(occ);
        Assert.IsType<OkObjectResult>(result);

        var db = context.Occurrences.First(x => x.Id == occ.Id);
        Assert.Equal("Atualizado", db.Title);
    }

    [Fact]
    public void Delete_RemovesFromDatabase()
    {
        var occ = repo.Create(BuildOccurrence());

        var result = controller.Delete(occ.Id);
        Assert.IsType<OkObjectResult>(result);

        Assert.False(context.Occurrences.Any(o => o.Id == occ.Id));
    }

    [Fact]
    public void GetAll_ReturnsList()
    {
        repo.Create(BuildOccurrence());
        repo.Create(BuildOccurrence());

        var result = controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<Occurrence>>(ok.Value);

        Assert.True(list.Count >= 2);
    }

    [Fact]
    public void GetAllActive_ReturnsFilteredResults()
    {
        var occ = BuildOccurrence();
        occ.Status = OccurrenceStatus.ACTIVE;
        repo.Create(occ);

        var result = controller.GetAllActive();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<OccurrenceMapDto>>(ok.Value);

        Assert.True(list.Count > 0);
        Assert.All(list, x => Assert.Equal(OccurrenceStatus.ACTIVE, x.Status));
    }
}

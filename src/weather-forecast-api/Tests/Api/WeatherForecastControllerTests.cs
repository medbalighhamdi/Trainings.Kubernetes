using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using WeatherForecast.Api.Controllers;
using WeatherForecast.Application.Services.Interfaces;
using DomainWeatherForecast = WeatherForecast.Domain.AggregateModel.WeatherAggregate.WeatherForecast;

namespace WeatherForecast.Tests.Api.Controllers;

[TestFixture]
public class WeatherForecastControllerTests
{
    private Fixture _fixture;
    private Mock<IWeatherForecastService> _serviceMock;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _serviceMock = new Mock<IWeatherForecastService>();
    }

    [Test]
    public async Task GetAll_ShouldReturnOk_WithMappedDtos_WhenServiceReturnsData()
    {
        // Arrange
        var forecasts = _fixture.CreateMany<DomainWeatherForecast>(3).ToList();

        _serviceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(forecasts);

        var sut = new WeatherForecastController(_serviceMock.Object);
        var ct = new CancellationToken();

        // Act
        var result = await sut.GetAll(ct);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.ShouldNotBeNull();
        okResult.StatusCode.ShouldBe(200);

        var dtos = okResult.Value as IEnumerable<object>;
        dtos.ShouldNotBeNull();
        dtos.Count().ShouldBe(forecasts.Count);

        _serviceMock.Verify(s => s.GetAll(ct), Times.Once);
    }

    [Test]
    public async Task GetAll_ShouldReturnNoContent_WhenServiceReturnsEmptyCollection()
    {
        // Arrange
        var emptyForecasts = Enumerable.Empty<DomainWeatherForecast>();

        _serviceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyForecasts);

        var sut = new WeatherForecastController(_serviceMock.Object);
        var ct = new CancellationToken();

        // Act
        var result = await sut.GetAll(ct);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.ShouldNotBeNull();
        noContentResult.StatusCode.ShouldBe(204);

        _serviceMock.Verify(s => s.GetAll(ct), Times.Once);
    }

    [Test]
    public async Task GetAll_ShouldPropagateCancellationToken_ToService()
    {
        // Arrange
        var forecasts = _fixture.CreateMany<DomainWeatherForecast>(2);
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _serviceMock
            .Setup(s => s.GetAll(ct))
            .ReturnsAsync(forecasts);

        var sut = new WeatherForecastController(_serviceMock.Object);

        // Act
        var result = await sut.GetAll(ct);

        // Assert
        _serviceMock.Verify(s => s.GetAll(ct), Times.Once);
    }
}

using AutoFixture;
using Moq;
using Shouldly;
using WeatherForecast.Application;
using WeatherForecast.Domain.Ports.Adapters.Database;
using DomainWeatherForecast = WeatherForecast.Domain.AggregateModel.WeatherAggregate.WeatherForecast;

namespace WeatherForecast.Tests.Application;

[TestFixture]
public class WeatherForecastServiceTests
{
    private Fixture _fixture;
    private Mock<IWeatherForecastDbAdapter> _adapterMock;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _adapterMock = new Mock<IWeatherForecastDbAdapter>();
    }

    [Test]
    public async Task GetAll_Should_Call_Adapter_And_Return_Results()
    {
        // Arrange
        var expected = _fixture.CreateMany<DomainWeatherForecast>(3);

        _adapterMock
            .Setup(a => a.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new WeatherForecastService(_adapterMock.Object);
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await sut.GetAll(ct);

        // Assert
        _adapterMock.Verify(a => a.GetAll(ct), Times.Once);
        result.ShouldBe(expected);
    }

    [Test]
    public async Task GetAll_Should_Propagate_CancellationToken()
    {
        // Arrange
        var expectedResult = _fixture.CreateMany<DomainWeatherForecast>(2);
        var ct = new CancellationTokenSource().Token;

        _adapterMock
            .Setup(a => a.GetAll(ct))
            .ReturnsAsync(expectedResult);

        var sut = new WeatherForecastService(_adapterMock.Object);

        // Act
        var result = await sut.GetAll(ct);

        // Assert
        _adapterMock.Verify(a => a.GetAll(ct), Times.Once);
        result.ShouldBe(expectedResult);
    }
}

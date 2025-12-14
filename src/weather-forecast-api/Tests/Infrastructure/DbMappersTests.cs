using Shouldly;
using AutoFixture;
using StackExchange.Redis;
using System.Text.Json;
using WeatherForecast.Infrastructure.Adapters.Database.Extensions;
using WeatherForecast.Infrastructure.Adapters.Database.Models;
using WeatherForecast.Domain.AggregateModel.WeatherAggregate.Enums;
using DomainModel = WeatherForecast.Domain.AggregateModel.WeatherAggregate;

namespace WeatherForecast.Tests.Infrastructure.Adapters.Database.Extensions;

[TestFixture]
public class DbMappersTests
{
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ToDbObject_FromDomain_ShouldMapCorrectly()
    {
        // Arrange
        var domain = _fixture.Create<DomainModel.WeatherForecast>();

        // Act
        var result = domain.ToDbObject();

        // Assert
        result.Temperature.ShouldBe(domain.Temperature);
        result.Scale.ShouldBe(domain.Scale.ToString());
        result.Summary.ShouldBe(domain.Summary);
    }

    [Test]
    public void ToJson_ShouldSerializeWithEnumAsString()
    {
        // Arrange
        var domain = _fixture.Create<DomainModel.WeatherForecast>();

        // Act
        var json = domain.ToJson();

        // Assert
        json.ShouldContain($"\"Scale\":\"{domain.Scale}\"");
        json.ShouldContain($"\"Temperature\":{domain.Temperature}");
        json.ShouldContain($"\"Summary\":\"{domain.Summary}\"");
    }

    [Test]
    public void ToDbObject_FromRedisValue_ShouldDeserializeCorrectly()
    {
        // Arrange
        var dbObject = _fixture.Create<DbWeatherForecast>();

        string json = JsonSerializer.Serialize(dbObject);
        RedisValue redisValue = json;

        // Act
        var result = redisValue.ToDbObject();

        // Assert
        result.Temperature.ShouldBe(dbObject.Temperature);
        result.Scale.ShouldBe(dbObject.Scale);
        result.Summary.ShouldBe(dbObject.Summary);
    }

    [Test]
    public void ToDomainObject_ShouldMapBackToDomainCorrectly()
    {
        // Arrange
        var dbForecast = _fixture.Build<DbWeatherForecast>()
                            .With(dbWf => dbWf.Scale, MeasurementScale.Celsius.ToString())
                            .Create();
        var city = _fixture.Create<string>();

        // Act
        var result = dbForecast.ToDomainObject(city);

        // Assert
        result.City.ShouldBe(city);
        result.Temperature.ShouldBe(dbForecast.Temperature);
        result.Scale.ShouldBe(Enum.Parse<MeasurementScale>(dbForecast.Scale));
        result.Summary.ShouldBe(dbForecast.Summary);
    }
}


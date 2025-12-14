using Shouldly;
using StackExchange.Redis;
using WeatherForecast.Infrastructure.Adapters.Database.Factories;

namespace WeatherForecast.Tests.Infrastructure.Adapters.Database.Factories;

[TestFixture]
public class RedisKeyUtilsTests
{
    [Test]
    public void CreateWeatherForecastKey_ShouldReturnExpectedKey()
    {
        // Arrange
        var city = "Paris";

        // Act
        var result = RedisKeyUtils.CreateWeatherForecastKey(city);

        // Assert
        result.ShouldBe("WeatherForecast:Paris");
    }

    [Test]
    public void CreateWeatherForecastKey_ShouldHandleEmptyCity()
    {
        // Arrange
        var city = string.Empty;

        // Act
        var result = RedisKeyUtils.CreateWeatherForecastKey(city);

        // Assert
        result.ShouldBe("WeatherForecast:");
    }

    [Test]
    public void ResolveWeatherForecastCity_ShouldReturnCityName()
    {
        // Arrange
        RedisKey redisKey = "WeatherForecast:London";

        // Act
        var result = RedisKeyUtils.ResolveWeatherForecastCity(redisKey);

        // Assert
        result.ShouldBe("London");
    }

    [Test]
    public void ResolveWeatherForecastCity_ShouldReturnFullSuffixIfNoCityProvided()
    {
        // Arrange
        RedisKey redisKey = "WeatherForecast:";

        // Act
        var result = RedisKeyUtils.ResolveWeatherForecastCity(redisKey);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Test]
    public void ResolveWeatherForecastCity_ShouldHandleUnexpectedPrefixes()
    {
        // Arrange
        RedisKey redisKey = "WeatherForecast:Berlin:Extra";

        // Act
        var result = RedisKeyUtils.ResolveWeatherForecastCity(redisKey);

        // Assert
        result.ShouldBe("Berlin:Extra");
    }
}


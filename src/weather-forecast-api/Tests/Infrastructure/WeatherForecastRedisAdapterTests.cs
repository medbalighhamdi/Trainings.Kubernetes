using AutoFixture;
using Moq;
using Shouldly;
using StackExchange.Redis;
using WeatherForecast.Api.Domain.Ports;
using WeatherForecast.Domain.AggregateModel.WeatherAggregate.Enums;
using WeatherForecast.Infrastructure.Adapters.Database;
using WeatherForecast.Infrastructure.Adapters.Database.Extensions;
using WeatherForecast.Infrastructure.Adapters.Database.Factories;
using WeatherForecast.Infrastructure.Adapters.Database.Models;
using DomainModel = WeatherForecast.Domain.AggregateModel.WeatherAggregate;

namespace WeatherForecast.Tests.Infrastructure.Adapters.Database;

[TestFixture]
public class WeatherForecastRedisDbAdapterTests
{
    private Fixture _fixture;
    private Mock<IRedisConnector> _redisConnectorMock;
    private Mock<IDatabase> _readDbMock;
    private Mock<IDatabase> _writeDbMock;
    private Mock<IServer> _serverMock;
    private Mock<IBatch> _batchMock;
    private WeatherForecastRedisDbAdapter _adapter;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _redisConnectorMock = new Mock<IRedisConnector>();
        _readDbMock = new Mock<IDatabase>();
        _writeDbMock = new Mock<IDatabase>();
        _serverMock = new Mock<IServer>();
        _batchMock = new Mock<IBatch>();

        _redisConnectorMock.Setup(r => r.ReadDb).Returns(_readDbMock.Object);
        _redisConnectorMock.Setup(r => r.WriteDb).Returns(_writeDbMock.Object);
        _redisConnectorMock.Setup(r => r.ReadServer).Returns(_serverMock.Object);

        _readDbMock.Setup(db => db.CreateBatch(It.IsAny<object>())).Returns(_batchMock.Object);

        _adapter = new WeatherForecastRedisDbAdapter(_redisConnectorMock.Object);
    }

    // -------------------------------------------------------------------
    //  ADD OR UPDATE
    // -------------------------------------------------------------------
    [Test]
    public async Task AddOrUpdate_ShouldWriteJsonToCorrectRedisKey()
    {
        // Arrange
        var wf = _fixture.Create<DomainModel.WeatherForecast>();
        string expectedKey = RedisKeyUtils.CreateWeatherForecastKey(wf.City);
        string expectedJson = wf.ToJson();

        _writeDbMock
            .Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _adapter.AddOrUpdate(wf, CancellationToken.None);

        // Assert
        _writeDbMock.Verify(db =>
            db.StringSetAsync(
                expectedKey,
                expectedJson,
                It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    // -------------------------------------------------------------------
    //  GET ALL
    // -------------------------------------------------------------------
    [Test]
    public async Task GetAll_ShouldReturnMappedDomainObjects()
    {
        // Arrange
        var redisKeys = new List<RedisKey>
            {
                (RedisKey)"WeatherForecast:Paris",
                (RedisKey)"WeatherForecast:London"
            };

        _serverMock
            .Setup(s => s.Keys(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Returns(redisKeys);

        // Create two DbWeatherForecast objects
        var db1 = _fixture.Build<DbWeatherForecast>()
                          .With(d => d.Scale, MeasurementScale.Celsius.ToString())
                          .Create();

        var db2 = _fixture.Build<DbWeatherForecast>()
                          .With(d => d.Scale, MeasurementScale.Fahrenheit.ToString())
                          .Create();

        var json1 = System.Text.Json.JsonSerializer.Serialize(db1);
        var json2 = System.Text.Json.JsonSerializer.Serialize(db2);

        // Mock async batch results
        var task1 = Task.FromResult((RedisValue)json1);
        var task2 = Task.FromResult((RedisValue)json2);

        var taskList = new List<Task<RedisValue>> { task1, task2 };

        int index = 0;

        _batchMock
            .Setup(b => b.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns((RedisKey key, CommandFlags f) => taskList[index++]);

        _batchMock.Setup(b => b.Execute());

        // Act
        var result = (await _adapter.GetAll(CancellationToken.None)).ToList();

        // Assert
        result.Count.ShouldBe(2);

        result[0].City.ShouldBe("Paris");
        result[0].Temperature.ShouldBe(db1.Temperature);
        result[0].Scale.ShouldBe(Enum.Parse<MeasurementScale>(db1.Scale));
        result[0].Summary.ShouldBe(db1.Summary);

        result[1].City.ShouldBe("London");
        result[1].Temperature.ShouldBe(db2.Temperature);
        result[1].Scale.ShouldBe(Enum.Parse<MeasurementScale>(db2.Scale));
        result[1].Summary.ShouldBe(db2.Summary);
    }
}

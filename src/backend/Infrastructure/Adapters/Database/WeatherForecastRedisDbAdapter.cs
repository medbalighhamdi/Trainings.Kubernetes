using DomaiNWeatherForecast = WeatherForecast.Domain.AggregateModel.WeatherAggregate.WeatherForecast;
using WeatherForecast.Api.Domain.Ports;
using WeatherForecast.Domain.Ports.Adapters.Database;
using WeatherForecast.Infrastructure.Adapters.Database.Extensions;
using WeatherForecast.Infrastructure.Adapters.Database.Factories;
using WeatherForecast.Infrastructure.Adapters.Database.Models;

namespace WeatherForecast.Infrastructure.Adapters.Database;

/// <summary>
/// Redis implmentation of the weather forecast db adapter
/// </summary>
public class WeatherForecastRedisDbAdapter : IWeatherForecastDbAdapter
{
    private const int BatchSize = 1000;
    private readonly IRedisConnector _redisConnector;

    public WeatherForecastRedisDbAdapter(IRedisConnector redisConnector)
    {
        _redisConnector = redisConnector;
    }

    #region implementations

    /// </<inheritdoc/>
    public async Task AddOrUpdate(DomaiNWeatherForecast weatherForecast, CancellationToken cancellationToken)
        => await _redisConnector.WriteDb.StringSetAsync(
                RedisKeyUtils.CreateWeatherForecastKey(weatherForecast.City),
                weatherForecast.ToJson()
                );

    /// <summary>
    /// </<inheritdoc/>
    /// Gets all redis data (keys and values)
    /// Implementation is optimized using stack exchange's <see cref="IBatch"/> 
    /// Similar to a select * from a relational database. Please use carefully.
    /// </summary>
    /// <returns>All stored redis key values</returns>
    public async Task<IEnumerable<DomaiNWeatherForecast>> GetAll(CancellationToken cancellationToken)
    {
        var redisKeys = _redisConnector.ReadServer.Keys(pattern: "*")
                // very important to materialize the lazy keys using ToList() or ToArray()
                // this will prevent mulitple scans and cursor resets when matrialization is performed later in code
                .ToList();

        // use stack exchange batch method for extreme optimization
        var batch = _redisConnector.ReadDb.CreateBatch();

        var byKeyTasks = redisKeys.Select(key =>
        {
            var task = batch.StringGetAsync(key); // call GetAsync from the instanciated batch above
            return new DbWeatherForecastKeyValue(key.ToString(), task);
        }).ToList();

        // execute operations that are configured on the batch processor
        batch.Execute();

        var redisValues = await Task.WhenAll(byKeyTasks.Select(t => t.Value)).WaitAsync(cancellationToken);

        return byKeyTasks
            .Select(kv => kv.Value.Result
                // maps the redis value to a db object
                .ToDbObject()
                // and then maps the db object to a domain object
                // and passing the city (which is the redis key) as parameter
                .ToDomainObject(RedisKeyUtils.ResolveWeatherForecastCity(kv.Key)))
            .ToList();
    }

    #endregion
}

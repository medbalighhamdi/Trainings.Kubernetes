using System.Reflection;
using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using StackExchange.Redis;
using WeatherForecast.Domain.Ports.Config;
using WeatherForecast.Infrastructure.Redis;

namespace WeatherForecast.Tests.Infrastructure.Redis
{
    [TestFixture]
    public class RedisConnectorLazyTests
    {
        private Fixture _fixture;
        private Mock<IOptionsMonitor<WeatherForecastConfig>> _optionsMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _optionsMock = new Mock<IOptionsMonitor<WeatherForecastConfig>>();
        }

        private RedisConnector CreateConnectorWithConfig(string readConn, string writeConn)
        {
            var cfg = new WeatherForecastConfig
            {
                Redis = new RedisConfig
                {
                    ReadDbConnection = readConn,
                    WriteDbConnection = writeConn
                }
            };

            _optionsMock.Setup(o => o.CurrentValue).Returns(cfg);
            return new RedisConnector(_optionsMock.Object);
        }

        // ---------------------------
        // 1) Inspect lazy factory closure
        // ---------------------------
        [Test]
        public void Constructor_ShouldCreate_LazyFactories_WithExpectedConnectionStrings_InClosure()
        {
            // Arrange
            var readConn = "redis://read-host:6379/0";
            var writeConn = "redis://write-host:6380/0";

            var connector = CreateConnectorWithConfig(readConn, writeConn);

            // Act: reflect into private Lazy<ConnectionMultiplexer> fields
            var type = typeof(RedisConnector);
            var readField = type.GetField("_readConnection", BindingFlags.Instance | BindingFlags.NonPublic);
            var writeField = type.GetField("_writeConnection", BindingFlags.Instance | BindingFlags.NonPublic);

            readField.ShouldNotBeNull();
            writeField.ShouldNotBeNull();

            var readLazy = readField.GetValue(connector);
            var writeLazy = writeField.GetValue(connector);

            readLazy.ShouldNotBeNull();
            writeLazy.ShouldNotBeNull();

            // Lazy<T> stores its factory in a private field. Common names:
            // - "m_valueFactory" (most runtimes)
            // - "valueFactory" (some)
            // We'll try both.
            var lazyType = readLazy.GetType(); // Lazy<ConnectionMultiplexer>
            var factoryField = lazyType
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(Delegate)) || f.FieldType == typeof(Delegate) || typeof(Delegate).IsAssignableFrom(f.FieldType));

            factoryField.ShouldNotBeNull("Could not find the private delegate field inside Lazy<>. The runtime layout might differ.");

            // Get delegate instances
            var readFactoryDelegate = factoryField.GetValue(readLazy) as Delegate;
            var writeFactoryDelegate = factoryField.GetValue(writeLazy) as Delegate;

            readFactoryDelegate.ShouldNotBeNull();
            writeFactoryDelegate.ShouldNotBeNull();

            // Inspect the closure target (the object that holds captured variables).
            // The Target may be null for static delegates, but in our code it's a lambda capturing a string, so Target should be an object.
            var readClosure = readFactoryDelegate.Target;
            var writeClosure = writeFactoryDelegate.Target;

            readClosure.ShouldNotBeNull();
            writeClosure.ShouldNotBeNull();

            // Find any string fields inside closure object and assert they contain the expected connection string
            bool readFound = readClosure
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(fi => fi.GetValue(readClosure))
                .OfType<string>()
                .Any(s => string.Equals(s, readConn, StringComparison.Ordinal));

            bool writeFound = writeClosure
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(fi => fi.GetValue(writeClosure))
                .OfType<string>()
                .Any(s => string.Equals(s, writeConn, StringComparison.Ordinal));

            // Assert
            readFound.ShouldBeTrue($"Could not find the expected read connection string '{readConn}' inside the lazy factory closure. Closure fields: {string.Join(",", readClosure.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(f => $"{f.Name}"))}");
            writeFound.ShouldBeTrue($"Could not find the expected write connection string '{writeConn}' inside the lazy factory closure. Closure fields: {string.Join(",", writeClosure.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(f => $"{f.Name}"))}");
        }
    }
}

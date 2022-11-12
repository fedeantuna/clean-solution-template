using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using CleanSolutionTemplate.Domain.Common.Attributes;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace CleanSolutionTemplate.Api.SerilogPolicies;

public class SensitiveInformationDestructuringPolicy : IDestructuringPolicy
{
    internal const string Mask = "#####";

    private static readonly ConcurrentDictionary<Type, IEnumerable<Property>> Cache = new();

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result)
    {
        if (value is IEnumerable or Delegate)
        {
            result = new ScalarValue(null);
            return false;
        }

        var properties = GetProperties(value);

        var logEventProperties = GetLogEventProperties(properties,
            propertyValueFactory);

        result = new StructureValue(logEventProperties);

        return true;
    }

    private static IEnumerable<Property> GetProperties(object instance)
    {
        var type = instance.GetType();

        return Cache.GetOrAdd(type, type
            .GetProperties()
            .Where(pi => pi.CanRead)
            .Select(pi => new Property
            {
                Name = pi.Name,
                Value = GetPropertyValueSafely(instance, pi),
                IsSensitiveData = pi.GetCustomAttribute<SensitiveInformationAttribute>() is not null
            }));
    }

    private static IEnumerable<LogEventProperty> GetLogEventProperties(IEnumerable<Property> properties,
        ILogEventPropertyValueFactory propertyValueFactory)
    {

        var logEventProperties = new List<LogEventProperty>();
        properties
            .ToList()
            .ForEach(property =>
            {
                var propertyValue = property.IsSensitiveData ? Mask : property.Value;
                var logEventPropertyValue = propertyValue is null
                    ? new ScalarValue(null)
                    : propertyValueFactory.CreatePropertyValue(propertyValue, true);

                logEventProperties.Add(new LogEventProperty(property.Name, logEventPropertyValue));
            });

        return logEventProperties;
    }

    private static object? GetPropertyValueSafely(object instance, PropertyInfo propertyInfo)
    {
        try
        {
            return propertyInfo.GetValue(instance);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("Property Accessor {0} throws Exception {1}", propertyInfo, ex);
            return $"Property Accessor throws {ex.InnerException?.GetType().Name ?? ex.GetType().Name}";
        }
    }
}

public record Property
{
    public string Name { get; init; } = null!;

    public object? Value { get; init; }

    public bool IsSensitiveData { get; init; }
}

public static class SensitiveInformationDestructuringPolicyExtensions
{
    public static LoggerConfiguration UseSensitiveDataMasking(this LoggerDestructuringConfiguration configuration) =>
        configuration.With<SensitiveInformationDestructuringPolicy>();
}

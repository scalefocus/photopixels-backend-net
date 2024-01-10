using System.Globalization;

namespace SF.PhotoPixels.Domain.Entities;

public class ApplicationConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Dictionary<string, object?> Data { get; set; } = new();

    public T? GetValue<T>(string key)
    {
        if (!Data.ContainsKey(key))
        {
            return default!;
        }

        var value = Data[key];

        if (value is null)
        {
            return default;
        }

        var paramType = typeof(T);

        if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            paramType = Nullable.GetUnderlyingType(paramType);
        }

        return (T)Convert.ChangeType(value, paramType!, CultureInfo.InvariantCulture);
    }

    public void SetValue<T>(string key, T value)
    {
        if (value is not null)
        {
            Data[key] = value;
        }
    }
}
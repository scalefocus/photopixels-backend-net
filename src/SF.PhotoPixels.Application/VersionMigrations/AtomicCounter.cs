namespace SF.PhotoPixels.Application.VersionMigrations;

public class AtomicCounter
{
    private int _value;

    /// <summary>
    ///     Retrieves the current value of the counter
    /// </summary>
    public int Current => _value;

    /// <summary>
    ///     Creates an instance of an AtomicCounter.
    /// </summary>
    /// <param name="initialValue">The initial value of this counter.</param>
    public AtomicCounter(int initialValue)
    {
        _value = initialValue;
    }

    /// <summary>
    ///     Creates an instance of an AtomicCounter with a starting value of 0.
    /// </summary>
    public AtomicCounter()
    {
    }

    /// <summary>
    ///     Increments the counter and returns the next value
    /// </summary>
    public int Next()
    {
        return Interlocked.Increment(ref _value);
    }

    /// <summary>
    ///     Decrements the counter and returns the next value
    /// </summary>
    public int Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _value, 0);
    }
}
namespace JobDrop.Domain.Common;
public static class Guard
{
    public static T NotNull<T>(T? value, string name) where T : class
        => value ?? throw new ArgumentNullException(name);
    public static string NotNullOrWhiteSpace(string? value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.", name) : value;
    public static int Positive(int v, string name)
        => v <= 0 ? throw new ArgumentOutOfRangeException(name, $"{name} must be > 0.") : v;
    public static void Against(bool condition, string message)
    { if (condition) throw new InvalidOperationException(message); }
}
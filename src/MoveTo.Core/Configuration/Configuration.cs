using System.Collections.ObjectModel;

namespace MoveTo.Core.Configuration;

public sealed class Configuration
{
    private readonly ReadOnlyCollection<Destination> _destinations;

    public Configuration(IEnumerable<Destination> destinations)
    {
        _destinations = destinations?.ToList().AsReadOnly() ?? new List<Destination>().AsReadOnly();
    }

    public IReadOnlyList<Destination> GetDestinations() => _destinations;

    public int GetDestinationCount() => _destinations.Count;
}

public sealed class Destination
{
    public Destination(string displayName, string path)
    {
        DisplayName = displayName;
        Path = path;
    }

    public string DisplayName { get; }

    public string Path { get; }
}

using System.Text.Json;

namespace MoveTo.Core.Configuration;

public sealed class ConfigurationRepository
{
    private const int MaxDestinations = 10;
    private readonly string _configPath;
    private readonly Action<string>? _log;

    public ConfigurationRepository(string? configPath = null, Action<string>? log = null)
    {
        _configPath = string.IsNullOrWhiteSpace(configPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoveTo", "config.json")
            : configPath;
        _log = log;
    }

    public Configuration LoadConfiguration()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return Empty();
            }

            var json = File.ReadAllText(_configPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Empty();
            }

            var root = JsonSerializer.Deserialize<ConfigurationDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (root?.Destinations == null)
            {
                return Empty();
            }

            var list = new List<Destination>(MaxDestinations);
            foreach (var dto in root.Destinations)
            {
                if (list.Count >= MaxDestinations)
                {
                    break;
                }

                if (dto is null)
                {
                    continue;
                }

                var name = dto.DisplayName?.Trim();
                var path = dto.Path?.Trim();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                list.Add(new Destination(name, path));
            }

            return new Configuration(list);
        }
        catch (JsonException ex)
        {
            _log?.Invoke($"Invalid JSON in configuration file: {ex.Message}");
            return Empty();
        }
        catch (Exception ex)
        {
            _log?.Invoke($"Failed to load configuration: {ex.Message}");
            return Empty();
        }
    }

    private static Configuration Empty() => new Configuration(Array.Empty<Destination>());

    private sealed class ConfigurationDto
    {
        public List<DestinationDto>? Destinations { get; set; }
    }

    private sealed class DestinationDto
    {
        public string? DisplayName { get; set; }
        public string? Path { get; set; }
    }
}

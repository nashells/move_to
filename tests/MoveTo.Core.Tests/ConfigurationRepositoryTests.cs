using System.Text;
using MoveTo.Core.Configuration;
using Xunit;

namespace MoveTo.Core.Tests;

public class ConfigurationRepositoryTests
{
    [Fact]
    public void LoadConfiguration_ReturnsDestinations()
    {
        var path = CreateTempConfig(
            """
            {
              "destinations": [
                { "displayName": "Temp", "path": "C:\\Temp" },
                { "displayName": "Docs", "path": "C:\\Docs" },
                { "displayName": "Work", "path": "D:\\Work" }
              ]
            }
            """
        );

        var repository = new ConfigurationRepository(path);
        var config = repository.LoadConfiguration();

        Assert.Equal(3, config.GetDestinationCount());
        Assert.Collection(
            config.GetDestinations(),
            d => AssertDestination(d, "Temp", "C:\\Temp"),
            d => AssertDestination(d, "Docs", "C:\\Docs"),
            d => AssertDestination(d, "Work", "D:\\Work")
        );

        Cleanup(path);
    }

    [Fact]
    public void LoadConfiguration_TruncatesToTen()
    {
        var content = BuildDestinationsJson(11);
        var path = CreateTempConfig(content);

        var repository = new ConfigurationRepository(path);
        var config = repository.LoadConfiguration();

        Assert.Equal(10, config.GetDestinationCount());
        Assert.Equal("Name0", config.GetDestinations().First().DisplayName);
        Assert.Equal("Name9", config.GetDestinations().Last().DisplayName);

        Cleanup(path);
    }

    [Fact]
    public void LoadConfiguration_MissingFile_ReturnsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

        var repository = new ConfigurationRepository(path);
        var config = repository.LoadConfiguration();

        Assert.Empty(config.GetDestinations());
    }

    [Fact]
    public void LoadConfiguration_InvalidJson_ReturnsEmpty()
    {
        var path = CreateTempConfig("{ invalid json }");

        var repository = new ConfigurationRepository(path);
        var config = repository.LoadConfiguration();

        Assert.Empty(config.GetDestinations());

        Cleanup(path);
    }

    [Fact]
    public void LoadConfiguration_SkipsInvalidEntries()
    {
        var path = CreateTempConfig(
            """
            {
              "destinations": [
                { "displayName": "", "path": "C:\\Temp" },
                { "displayName": "Valid", "path": "" },
                { "displayName": "Good", "path": "D:\\Good" }
              ]
            }
            """
        );

        var repository = new ConfigurationRepository(path);
        var config = repository.LoadConfiguration();

        var destinations = config.GetDestinations();
        Assert.Single(destinations);
        AssertDestination(destinations.First(), "Good", "D:\\Good");

        Cleanup(path);
    }

    private static string BuildDestinationsJson(int count)
    {
        var sb = new StringBuilder();
        sb.Append("{\"destinations\":[");
        for (var i = 0; i < count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('{');
            sb.Append("\"displayName\":\"Name").Append(i).Append("\",");
            sb.Append("\"path\":\"C:\\\\Path").Append(i).Append("\"");
            sb.Append('}');
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static string CreateTempConfig(string content)
    {
        var directory = Path.Combine(Path.GetTempPath(), "move_to_config_tests");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, content);
        return path;
    }

    private static void Cleanup(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void AssertDestination(Destination destination, string expectedName, string expectedPath)
    {
        Assert.Equal(expectedName, destination.DisplayName);
        Assert.Equal(expectedPath, destination.Path);
    }
}

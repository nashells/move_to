using MoveTo.Core.Configuration;
using MoveTo.Core.ContextMenu;
using MoveTo.Core.FileMover;
using MoveTo.Core.ConflictResolution;
using Xunit;

namespace MoveTo.Core.Tests;

public class ContextMenuTests
{
    [Fact]
    public void BuildMenu_UsesConfigurationDestinations()
    {
        var provider = new FakeConfigurationProvider(new[]
        {
            new Destination("Temp", "C:/Temp"),
            new Destination("Docs", "C:/Docs")
        });

        var handler = CreateHandler(provider: provider);
        var menu = handler.BuildMenu(new SelectionContext(new[] { new FileSystemItem("C:/file.txt", ItemType.File) }));

        Assert.Equal(2, menu.Items.Count);
        Assert.Equal("Temp", menu.Items[0].DisplayName);
        Assert.Equal("Docs", menu.Items[1].DisplayName);
    }

    [Fact]
    public void BuildMenu_EmptyDestinations_ReturnsEmptyMenu()
    {
        var provider = new FakeConfigurationProvider(Array.Empty<Destination>());
        var handler = CreateHandler(provider: provider);

        var menu = handler.BuildMenu(new SelectionContext(new[] { new FileSystemItem("C:/file.txt", ItemType.File) }));

        Assert.Empty(menu.Items);
    }

    [Fact]
    public void OnDestinationSelected_MovesSelectedItems()
    {
        var provider = new FakeConfigurationProvider(new[] { new Destination("Temp", "C:/Temp") });
        var mover = new FakeMover();
        var handler = CreateHandler(provider: provider, mover: mover);

        var selection = new SelectionContext(new[]
        {
            new FileSystemItem("C:/Src/a.txt", ItemType.File),
            new FileSystemItem("C:/Src/b.txt", ItemType.File)
        });

        var result = handler.OnDestinationSelected(provider.Configuration.GetDestinations().First(), selection);

        Assert.True(result.Success);
        Assert.Equal(2, mover.LastSources.Count);
        Assert.NotNull(mover.LastDestination);
        Assert.Equal(Norm("C:/Temp"), Norm(mover.LastDestination!.Path));
    }

    [Fact]
    public void OnDestinationSelected_WhenSelectionEmpty_Noop()
    {
        var provider = new FakeConfigurationProvider(new[] { new Destination("Temp", "C:/Temp") });
        var handler = CreateHandler(provider: provider);

        var result = handler.OnDestinationSelected(provider.Configuration.GetDestinations().First(), new SelectionContext(Array.Empty<FileSystemItem>()));

        Assert.True(result.Success);
        Assert.Empty(result.MovedItems);
    }

    private static ContextMenuHandler CreateHandler(FakeConfigurationProvider? provider = null, FakeMover? mover = null)
    {
        provider ??= new FakeConfigurationProvider(new[] { new Destination("Temp", "C:/Temp") });
        mover ??= new FakeMover();
        var conflict = new ConflictResolver(new StubConflictPresenter());
        var fileMover = new FileMoverService(conflict, mover, new StubErrorPresenter());
        var menuBuilder = new DefaultMenuBuilder();
        return new ContextMenuHandler(provider, menuBuilder, fileMover);
    }

    private static string Norm(string path) => path.Replace('\\', '/');

    private sealed class FakeConfigurationProvider : ConfigurationProvider
    {
        public Configuration.Configuration Configuration { get; }

        public FakeConfigurationProvider(IEnumerable<Destination> destinations)
        {
            Configuration = new Configuration.Configuration(destinations);
        }

        public Configuration.Configuration Load() => Configuration;
    }

    private sealed class FakeMover : FileSystemPort
    {
        public List<SourcePath> LastSources { get; } = new();
        public DestinationFolder? LastDestination { get; private set; }

        public bool Exists(string path) => true;
        public bool HasPermission(string path) => true;
        public bool FileExistsInDestination(string fileName, string folder) => false;
        public void Move(string source, string destination, bool overwrite)
        {
            LastSources.Add(new SourcePath(source));
            LastDestination = new DestinationFolder(Path.GetDirectoryName(destination) ?? destination);
        }
    }

    private sealed class StubErrorPresenter : ErrorPresenter
    {
        public void ShowFolderNotFoundError(string path) { }
        public void ShowAccessDeniedError(string path) { }
    }

    private sealed class StubConflictPresenter : ConflictDialogPresenter
    {
        public ConflictAction ShowConflictDialog(FileConflict conflict) => ConflictAction.Overwrite;
        public FileName? ShowRenameDialog(FileName defaultName) => null;
    }
}

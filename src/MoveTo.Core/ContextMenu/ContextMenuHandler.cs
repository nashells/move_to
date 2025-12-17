using MoveTo.Core.Configuration;
using MoveTo.Core.FileMover;

namespace MoveTo.Core.ContextMenu;

public sealed class ContextMenuHandler
{
    private readonly ConfigurationProvider _configurationProvider;
    private readonly MenuBuilder _menuBuilder;
    private readonly FileMoverService _fileMoverService;

    public ContextMenuHandler(ConfigurationProvider configurationProvider, MenuBuilder menuBuilder, FileMoverService fileMoverService)
    {
        _configurationProvider = configurationProvider;
        _menuBuilder = menuBuilder;
        _fileMoverService = fileMoverService;
    }

    public Menu BuildMenu(SelectionContext context)
    {
        var config = _configurationProvider.Load();
        var destinations = config.GetDestinations();
        return _menuBuilder.BuildCascadeMenu(destinations);
    }

    public MoveResult OnDestinationSelected(Destination destination, SelectionContext context)
    {
        if (context.IsEmpty())
        {
            return MoveResult.Completed(Array.Empty<MovedItem>());
        }

        var sources = context.GetSelectedItems()
            .Select(i => new SourcePath(i.Path))
            .ToList();

        var destFolder = new DestinationFolder(destination.Path);
        return _fileMoverService.Move(sources, destFolder);
    }
}

public sealed class RepositoryConfigurationProvider : ConfigurationProvider
{
    private readonly ConfigurationRepository _repository;

    public RepositoryConfigurationProvider(ConfigurationRepository repository)
    {
        _repository = repository;
    }

    public Configuration.Configuration Load() => _repository.LoadConfiguration();
}

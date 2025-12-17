using MoveTo.Core.Configuration;

namespace MoveTo.Core.ContextMenu;

public sealed class DefaultMenuBuilder : MenuBuilder
{
    public Menu BuildCascadeMenu(IEnumerable<Destination> destinations)
    {
        var items = destinations
            .Take(10)
            .Select(d => new MenuItem(d.DisplayName, d))
            .ToList();
        return new Menu(items);
    }
}

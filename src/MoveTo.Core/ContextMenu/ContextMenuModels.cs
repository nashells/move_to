using MoveTo.Core.Configuration;

namespace MoveTo.Core.ContextMenu;

public enum ItemType
{
    File,
    Folder
}

public sealed class FileSystemItem
{
    public FileSystemItem(string path, ItemType itemType)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        ItemType = itemType;
    }

    public string Path { get; }
    public ItemType ItemType { get; }
    public bool IsFile() => ItemType == ItemType.File;
    public bool IsFolder() => ItemType == ItemType.Folder;
}

public sealed class SelectionContext
{
    private readonly IReadOnlyList<FileSystemItem> _selectedItems;

    public SelectionContext(IEnumerable<FileSystemItem> selectedItems)
    {
        _selectedItems = selectedItems?.ToList() ?? new List<FileSystemItem>();
    }

    public IReadOnlyList<FileSystemItem> GetSelectedItems() => _selectedItems;
    public int GetItemCount() => _selectedItems.Count;
    public bool IsEmpty() => _selectedItems.Count == 0;
}

public sealed class MenuItem
{
    public MenuItem(string displayName, Destination destination)
    {
        DisplayName = displayName;
        Destination = destination;
    }

    public string DisplayName { get; }
    public Destination Destination { get; }
}

public sealed class Menu
{
    public Menu(IEnumerable<MenuItem> items)
    {
        Items = items?.ToList() ?? new List<MenuItem>();
    }

    public IReadOnlyList<MenuItem> Items { get; }
}

public interface MenuBuilder
{
    Menu BuildCascadeMenu(IEnumerable<Destination> destinations);
}

public interface ConfigurationProvider
{
    Configuration.Configuration Load();
}

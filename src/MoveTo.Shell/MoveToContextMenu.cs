using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MoveTo.Core.ConflictResolution;
using MoveTo.Core.Configuration;
using MoveTo.Core.ContextMenu;
using MoveTo.Core.FileMover;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace MoveTo.Shell;

[ComVisible(true)]
[COMServerAssociation(AssociationType.AllFiles)]
[ProgId("Nashells.MoveTo.ContextMenu")]
[Guid("D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B")]
public class MoveToContextMenu : SharpContextMenu
{
    private readonly ConfigurationRepository _configRepo;

    public MoveToContextMenu()
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoveTo", "config.json");
        _configRepo = new ConfigurationRepository(configPath, log: msg => System.Diagnostics.Debug.WriteLine(msg));
    }

    protected override bool CanShowMenu()
    {
        return SelectedItemPaths != null && SelectedItemPaths.Any();
    }

    protected override ContextMenuStrip CreateMenu()
    {
        var configProvider = new RepositoryConfigurationProvider(_configRepo);
        var menuBuilder = new DefaultMenuBuilder();
        var conflictPresenter = new ShellConflictPresenter();
        var fileSystem = new ShellFileSystem();
        var errorPresenter = new ShellErrorPresenter();
        var conflictResolver = new ConflictResolver(conflictPresenter);
        var mover = new FileMoverService(conflictResolver, fileSystem, errorPresenter);
        var handler = new ContextMenuHandler(configProvider, menuBuilder, mover);

        var context = new SelectionContext(GetSelectedItems());
        var menuModel = handler.BuildMenu(context);

        var menu = new ContextMenuStrip();
        var root = new ToolStripMenuItem("move to");
        foreach (var item in menuModel.Items)
        {
            var destination = item.Destination;
            var child = new ToolStripMenuItem(item.DisplayName);
            child.Click += (_, _) => handler.OnDestinationSelected(destination, context);
            root.DropDownItems.Add(child);
        }

        if (menuModel.Items.Count == 0)
        {
            var none = new ToolStripMenuItem("(設定なし)") { Enabled = false };
            root.DropDownItems.Add(none);
        }

        menu.Items.Add(root);
        return menu;
    }

    private IEnumerable<FileSystemItem> GetSelectedItems()
    {
        foreach (var path in SelectedItemPaths)
        {
            var type = Directory.Exists(path) ? ItemType.Folder : ItemType.File;
            yield return new FileSystemItem(path, type);
        }
    }
}

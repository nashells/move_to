using MoveTo.Core.ConflictResolution;
using MoveTo.Core.FileMover;
using Xunit;

namespace MoveTo.Core.Tests;

public class FileMoverTests
{
    [Fact]
    public void Move_SingleFile_Succeeds()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt");
        var mover = CreateMover(fs);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.True(result.Success);
        Assert.Single(result.MovedItems);
        Assert.Contains("C:/Dest/file.txt", fs.Files);
    }

    [Fact]
    public void Move_DestinationMissing_ShowsError()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt");
        fs.DestinationExists = false;
        var presenter = new FakeErrorPresenter();
        var mover = CreateMover(fs, presenter: presenter);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.False(result.Success);
        Assert.Contains(ErrorType.FolderNotFound, result.Errors.Select(e => e.ErrorType));
        Assert.True(presenter.FolderNotFoundShown);
    }

    [Fact]
    public void Move_AccessDenied_ShowsError()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt");
        fs.HasAccess = false;
        var presenter = new FakeErrorPresenter();
        var mover = CreateMover(fs, presenter: presenter);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.False(result.Success);
        Assert.Contains(ErrorType.AccessDenied, result.Errors.Select(e => e.ErrorType));
        Assert.True(presenter.AccessDeniedShown);
    }

    [Fact]
    public void Move_SourceMissing_AddsError()
    {
        var fs = FakeFs.WithFiles();
        var mover = CreateMover(fs);

        var result = mover.Move(new[] { new SourcePath("C:/Src/missing.txt") }, new DestinationFolder("C:/Dest"));

        Assert.False(result.Success);
        Assert.Contains(ErrorType.SourceNotFound, result.Errors.Select(e => e.ErrorType));
    }

    [Fact]
    public void Move_Conflict_Rename()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt", "C:/Dest/file.txt");
        var conflict = new FakeConflictPresenter(
            actions: new[] { ConflictAction.Rename },
            renameResponses: new[] { FileName.Parse("file_new.txt") }
        );
        var mover = CreateMover(fs, conflict: conflict);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.True(result.Success);
        Assert.Contains("C:/Dest/file_new.txt", fs.Files);
    }

    [Fact]
    public void Move_Conflict_Cancel_Stops()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt", "C:/Dest/file.txt");
        var conflict = new FakeConflictPresenter(actions: new[] { ConflictAction.Cancel }, renameResponses: Array.Empty<FileName?>());
        var mover = CreateMover(fs, conflict: conflict);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.False(result.Success);
        Assert.Contains(ErrorType.Conflict, result.Errors.Select(e => e.ErrorType));
        Assert.Contains("C:/Src/file.txt", fs.Files); // not moved
    }

    [Fact]
    public void Move_Conflict_Overwrite_ReplacesFile()
    {
        var fs = FakeFs.WithFiles("C:/Src/file.txt", "C:/Dest/file.txt");
        var conflict = new FakeConflictPresenter(actions: new[] { ConflictAction.Overwrite }, renameResponses: Array.Empty<FileName?>());
        var mover = CreateMover(fs, conflict: conflict);

        var result = mover.Move(new[] { new SourcePath("C:/Src/file.txt") }, new DestinationFolder("C:/Dest"));

        Assert.True(result.Success);
        Assert.DoesNotContain("C:/Src/file.txt", fs.Files);
        Assert.Contains("C:/Dest/file.txt", fs.Files);
    }

    [Fact]
    public void Move_Multiple_Files_Some_Missing()
    {
        var fs = FakeFs.WithFiles("C:/Src/a.txt");
        var mover = CreateMover(fs);

        var result = mover.Move(
            new[] { new SourcePath("C:/Src/a.txt"), new SourcePath("C:/Src/b.txt") },
            new DestinationFolder("C:/Dest")
        );

        Assert.False(result.Success);
        Assert.Contains(ErrorType.SourceNotFound, result.Errors.Select(e => e.ErrorType));
        Assert.Contains("C:/Dest/a.txt", fs.Files);
    }

    private static FileMoverService CreateMover(FakeFs fs, FakeErrorPresenter? presenter = null, FakeConflictPresenter? conflict = null)
    {
        presenter ??= new FakeErrorPresenter();
        conflict ??= new FakeConflictPresenter(new[] { ConflictAction.Overwrite }, Array.Empty<FileName?>());
        return new FileMoverService(new ConflictResolver(conflict), fs, presenter);
    }

    private sealed class FakeFs : FileSystemPort
    {
        public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
        public bool DestinationExists { get; set; } = true;
        public bool HasAccess { get; set; } = true;

        public static FakeFs WithFiles(params string[] files)
        {
            var fs = new FakeFs();
            foreach (var f in files) fs.Files.Add(Normalize(f));
            return fs;
        }

        public bool Exists(string path) => Files.Contains(Normalize(path)) || (DestinationExists && IsDestination(path));

        public bool HasPermission(string path) => HasAccess;

        public bool FileExistsInDestination(string fileName, string folder)
        {
            var target = Normalize(System.IO.Path.Combine(folder, fileName));
            return Files.Contains(target);
        }

        public void Move(string source, string destination, bool overwrite)
        {
            var src = Normalize(source);
            var dest = Normalize(destination);
            if (!Files.Contains(src)) throw new FileNotFoundException(src);

            if (Files.Contains(dest) && !overwrite)
            {
                throw new IOException("Destination exists");
            }

            Files.Remove(src);
            Files.Add(dest);
        }

        private static string Normalize(string path) => path.Replace('\\', '/');

        private static bool IsDestination(string path)
        {
            var p = Normalize(path);
            return p.Equals("C:/Dest", StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FakeErrorPresenter : ErrorPresenter
    {
        public bool FolderNotFoundShown { get; private set; }
        public bool AccessDeniedShown { get; private set; }

        public void ShowFolderNotFoundError(string path) => FolderNotFoundShown = true;
        public void ShowAccessDeniedError(string path) => AccessDeniedShown = true;
    }

    private sealed class FakeConflictPresenter : ConflictDialogPresenter
    {
        private readonly Queue<ConflictAction> _actions;
        private readonly Queue<FileName?> _renameResponses;

        public FakeConflictPresenter(IEnumerable<ConflictAction> actions, IEnumerable<FileName?> renameResponses)
        {
            _actions = new Queue<ConflictAction>(actions);
            _renameResponses = new Queue<FileName?>(renameResponses);
        }

        public ConflictAction ShowConflictDialog(FileConflict conflict)
        {
            return _actions.Count > 0 ? _actions.Dequeue() : ConflictAction.Cancel;
        }

        public FileName? ShowRenameDialog(FileName defaultName)
        {
            return _renameResponses.Count > 0 ? _renameResponses.Dequeue() : null;
        }
    }
}

using MoveTo.Core.ConflictResolution;
using Xunit;

namespace MoveTo.Core.Tests;

public class ConflictResolverTests
{
    [Fact]
    public void Resolve_Overwrite()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        var presenter = PresenterScript.FromActions(ConflictAction.Overwrite);
        var resolver = new ConflictResolver(presenter);

        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Overwrite, result.Action);
        Assert.Null(result.NewFileName);
    }

    [Fact]
    public void Resolve_Skip()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        var presenter = PresenterScript.FromActions(ConflictAction.Skip);
        var resolver = new ConflictResolver(presenter);

        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Skip, result.Action);
    }

    [Fact]
    public void Resolve_Cancel()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        var presenter = PresenterScript.FromActions(ConflictAction.Cancel);
        var resolver = new ConflictResolver(presenter);

        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Cancel, result.Action);
    }

    [Fact]
    public void Resolve_Rename_Flow()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        var presenter = PresenterScript.FromRename("example_copy.txt", "example_new.txt");
        var resolver = new ConflictResolver(presenter);

        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Rename, result.Action);
        Assert.Equal("example_new.txt", result.NewFileName!.GetFullName());
        Assert.Equal("example_copy.txt", presenter.LastDefaultRename);
    }

    [Fact]
    public void Resolve_Rename_Cancel_ReturnsToConflictDialog_ThenCancel()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        // First show conflict: Rename, then rename dialog returns null (cancel), then conflict dialog returns Cancel
        var presenter = PresenterScript.FromSequence(
            conflictActions: new[] { ConflictAction.Rename, ConflictAction.Cancel },
            renameResponses: new FileName?[] { null }
        );

        var resolver = new ConflictResolver(presenter);

        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Cancel, result.Action);
    }

    [Fact]
    public void Resolve_Rename_Rejects_Invalid_Name_Then_Takes_Second_Name()
    {
        var conflict = new FileConflict(FileName.Parse("example.txt"), "C:/Dest");
        // First rename returns invalid name "" -> should loop back to conflict
        // Second conflict returns Rename again, rename dialog returns valid name
        var presenter = PresenterScript.FromSequence(
            conflictActions: new[] { ConflictAction.Rename, ConflictAction.Rename },
            renameResponses: new FileName?[] { FileName.Parse("invalid|name"), FileName.Parse("valid.txt") }
        );

        var resolver = new ConflictResolver(presenter);
        var result = resolver.Resolve(conflict);

        Assert.Equal(ConflictAction.Rename, result.Action);
        Assert.Equal("valid.txt", result.NewFileName!.GetFullName());
    }

    [Fact]
    public void FileNameGenerator_Generates_Copy_With_Extension()
    {
        var gen = new FileNameGenerator();
        var copy = gen.GenerateCopyName(FileName.Parse("document.pdf"));
        Assert.Equal("document_copy.pdf", copy.GetFullName());
    }

    [Fact]
    public void FileNameGenerator_Generates_Copy_Without_Extension()
    {
        var gen = new FileNameGenerator();
        var copy = gen.GenerateCopyName(FileName.Parse("archive"));
        Assert.Equal("archive_copy", copy.GetFullName());
    }

    private sealed class PresenterScript : ConflictDialogPresenter
    {
        private readonly Queue<ConflictAction> _conflictActions;
        private readonly Queue<FileName?> _renameResponses;

        public string? LastDefaultRename { get; private set; }

        private PresenterScript(Queue<ConflictAction> conflictActions, Queue<FileName?> renameResponses)
        {
            _conflictActions = conflictActions;
            _renameResponses = renameResponses;
        }

        public ConflictAction ShowConflictDialog(FileConflict conflict)
        {
            return _conflictActions.Count > 0 ? _conflictActions.Dequeue() : ConflictAction.Cancel;
        }

        public FileName? ShowRenameDialog(FileName defaultName)
        {
            LastDefaultRename = defaultName.GetFullName();
            return _renameResponses.Count > 0 ? _renameResponses.Dequeue() : null;
        }

        public static PresenterScript FromActions(params ConflictAction[] actions)
        {
            return new PresenterScript(new Queue<ConflictAction>(actions), new Queue<FileName?>());
        }

        public static PresenterScript FromRename(string expectedDefault, string newName)
        {
            return new PresenterScript(
                new Queue<ConflictAction>(new[] { ConflictAction.Rename }),
                new Queue<FileName?>(new[] { FileName.Parse(newName) })
            )
            {
                LastDefaultRename = expectedDefault
            };
        }

        public static PresenterScript FromSequence(ConflictAction[] conflictActions, FileName?[] renameResponses)
        {
            return new PresenterScript(new Queue<ConflictAction>(conflictActions), new Queue<FileName?>(renameResponses));
        }
    }
}

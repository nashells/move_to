namespace MoveTo.Core.ConflictResolution;

public enum ConflictAction
{
    Overwrite,
    Skip,
    Rename,
    Cancel
}

public sealed class ConflictResult
{
    public ConflictResult(ConflictAction action, FileName? newFileName = null)
    {
        Action = action;
        NewFileName = newFileName;
    }

    public ConflictAction Action { get; }

    public FileName? NewFileName { get; }

    public bool IsOverwrite() => Action == ConflictAction.Overwrite;
    public bool IsSkip() => Action == ConflictAction.Skip;
    public bool IsRename() => Action == ConflictAction.Rename;
    public bool IsCancel() => Action == ConflictAction.Cancel;
}

public sealed class FileConflict
{
    public FileConflict(FileName sourceFileName, string destinationFolder)
    {
        SourceFileName = sourceFileName;
        DestinationFolder = destinationFolder;
    }

    public FileName SourceFileName { get; }

    public string DestinationFolder { get; }

    public string GetConflictingFilePath()
        => Path.Combine(DestinationFolder, SourceFileName.GetFullName());
}

public interface ConflictDialogPresenter
{
    ConflictAction ShowConflictDialog(FileConflict conflict);
    FileName? ShowRenameDialog(FileName defaultName);
}

public sealed class FileNameGenerator
{
    public FileName GenerateCopyName(FileName original)
    {
        var newBase = original.HasExtension()
            ? original.BaseName
            : original.BaseName;

        var copyBase = string.Concat(newBase, "_copy");
        return new FileName(copyBase, original.Extension);
    }

    public bool IsValidFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
}

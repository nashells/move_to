namespace MoveTo.Core.FileMover;

public sealed class SourcePath
{
    public SourcePath(string path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public string Path { get; }

    public string GetFileName() => System.IO.Path.GetFileName(Path);
}

public sealed class DestinationFolder
{
    public DestinationFolder(string path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public string Path { get; }
}

public sealed class MoveResult
{
    public MoveResult(bool success, IReadOnlyList<MovedItem> movedItems, IReadOnlyList<MoveError> errors)
    {
        Success = success;
        MovedItems = movedItems;
        Errors = errors;
    }

    public bool Success { get; }
    public IReadOnlyList<MovedItem> MovedItems { get; }
    public IReadOnlyList<MoveError> Errors { get; }

    public static MoveResult Completed(IEnumerable<MovedItem> items)
        => new MoveResult(true, items.ToList(), Array.Empty<MoveError>());

    public static MoveResult Failed(IEnumerable<MoveError> errors, IEnumerable<MovedItem>? moved = null)
        => new MoveResult(false, moved?.ToList() ?? new List<MovedItem>(), errors.ToList());
}

public sealed class MovedItem
{
    public MovedItem(string sourcePath, string destinationPath)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }

    public string SourcePath { get; }
    public string DestinationPath { get; }
}

public enum ErrorType
{
    None,
    FolderNotFound,
    AccessDenied,
    Conflict,
    SourceNotFound,
    Unknown
}

public sealed class MoveError
{
    public MoveError(ErrorType errorType, string message, string path)
    {
        ErrorType = errorType;
        Message = message;
        Path = path;
    }

    public ErrorType ErrorType { get; }
    public string Message { get; }
    public string Path { get; }
}

public interface FileSystemPort
{
    bool Exists(string path);
    bool HasPermission(string path);
    void Move(string source, string destination, bool overwrite);
    bool FileExistsInDestination(string fileName, string folder);
}

public interface ErrorPresenter
{
    void ShowFolderNotFoundError(string path);
    void ShowAccessDeniedError(string path);
}

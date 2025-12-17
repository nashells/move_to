using MoveTo.Core.ConflictResolution;

namespace MoveTo.Core.FileMover;

public sealed class FileMoverService
{
    private readonly ConflictResolver _conflictResolver;
    private readonly FileSystemPort _fileSystem;
    private readonly ErrorPresenter _errorPresenter;

    public FileMoverService(ConflictResolver conflictResolver, FileSystemPort fileSystem, ErrorPresenter errorPresenter)
    {
        _conflictResolver = conflictResolver;
        _fileSystem = fileSystem;
        _errorPresenter = errorPresenter;
    }

    public MoveResult Move(IEnumerable<SourcePath> sources, DestinationFolder destination)
    {
        if (!_fileSystem.Exists(destination.Path))
        {
            _errorPresenter.ShowFolderNotFoundError(destination.Path);
            return MoveResult.Failed(new[]
            {
                new MoveError(ErrorType.FolderNotFound, "Destination folder not found.", destination.Path)
            });
        }

        if (!_fileSystem.HasPermission(destination.Path))
        {
            _errorPresenter.ShowAccessDeniedError(destination.Path);
            return MoveResult.Failed(new[]
            {
                new MoveError(ErrorType.AccessDenied, "Access denied to destination folder.", destination.Path)
            });
        }

        var moved = new List<MovedItem>();
        var errors = new List<MoveError>();

        foreach (var source in sources)
        {
            if (!_fileSystem.Exists(source.Path))
            {
                errors.Add(new MoveError(ErrorType.SourceNotFound, "Source not found.", source.Path));
                continue;
            }

            var targetPath = Path.Combine(destination.Path, source.GetFileName());

            try
            {
                if (_fileSystem.FileExistsInDestination(source.GetFileName(), destination.Path))
                {
                    var conflict = new FileConflict(FileName.Parse(source.GetFileName()), destination.Path);
                    var result = _conflictResolver.Resolve(conflict);

                    if (result.IsCancel())
                    {
                        return MoveResult.Failed(new[]
                        {
                            new MoveError(ErrorType.Conflict, "User cancelled due to conflict.", targetPath)
                        }, moved);
                    }

                    if (result.IsSkip())
                    {
                        continue;
                    }

                    if (result.IsRename() && result.NewFileName != null)
                    {
                        targetPath = Path.Combine(destination.Path, result.NewFileName.GetFullName());
                        _fileSystem.Move(source.Path, targetPath, overwrite: false);
                        moved.Add(new MovedItem(source.Path, targetPath));
                        continue;
                    }

                    if (result.IsOverwrite())
                    {
                        _fileSystem.Move(source.Path, targetPath, overwrite: true);
                        moved.Add(new MovedItem(source.Path, targetPath));
                        continue;
                    }
                }
                else
                {
                    _fileSystem.Move(source.Path, targetPath, overwrite: false);
                    moved.Add(new MovedItem(source.Path, targetPath));
                }
            }
            catch (Exception ex)
            {
                errors.Add(new MoveError(ErrorType.Unknown, ex.Message, source.Path));
            }
        }

        return errors.Count == 0
            ? MoveResult.Completed(moved)
            : MoveResult.Failed(errors, moved);
    }
}

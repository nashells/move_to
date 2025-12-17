namespace MoveTo.Core.ConflictResolution;

public sealed class ConflictResolver
{
    private readonly ConflictDialogPresenter _presenter;
    private readonly FileNameGenerator _nameGenerator;

    public ConflictResolver(ConflictDialogPresenter presenter, FileNameGenerator? nameGenerator = null)
    {
        _presenter = presenter;
        _nameGenerator = nameGenerator ?? new FileNameGenerator();
    }

    public ConflictResult Resolve(FileConflict conflict)
    {
        while (true)
        {
            var action = _presenter.ShowConflictDialog(conflict);
            switch (action)
            {
                case ConflictAction.Overwrite:
                case ConflictAction.Skip:
                case ConflictAction.Cancel:
                    return new ConflictResult(action);

                case ConflictAction.Rename:
                    var defaultName = _nameGenerator.GenerateCopyName(conflict.SourceFileName);
                    var newName = _presenter.ShowRenameDialog(defaultName);

                    if (newName == null)
                    {
                        // ユーザーがリネームをキャンセル → 確認ダイアログに戻る
                        continue;
                    }

                    var fullName = newName.GetFullName();
                    if (!_nameGenerator.IsValidFileName(fullName))
                    {
                        // 無効なファイル名 → 確認ダイアログに戻る
                        continue;
                    }

                    if (string.Equals(fullName, conflict.SourceFileName.GetFullName(), StringComparison.OrdinalIgnoreCase))
                    {
                        // 元と同名は競合解消にならないため戻る
                        continue;
                    }

                    return new ConflictResult(ConflictAction.Rename, newName);

                default:
                    throw new InvalidOperationException($"Unknown conflict action: {action}");
            }
        }
    }
}

using System.Windows.Forms;
using MoveTo.Core.ConflictResolution;
using MoveTo.Core.FileMover;

namespace MoveTo.Shell;

internal sealed class ShellErrorPresenter : ErrorPresenter
{
    public void ShowFolderNotFoundError(string path)
    {
        MessageBox.Show($"移動先フォルダーが見つかりません:\n{path}", "MoveTo", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowAccessDeniedError(string path)
    {
        MessageBox.Show($"移動先フォルダーへのアクセスが拒否されました:\n{path}", "MoveTo", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

internal sealed class ShellConflictPresenter : ConflictDialogPresenter
{
    public ConflictAction ShowConflictDialog(FileConflict conflict)
    {
        using var dialog = new ConflictDialog(conflict);
        var result = dialog.ShowDialog();
        return result switch
        {
            DialogResult.Yes => ConflictAction.Overwrite,
            DialogResult.No => ConflictAction.Skip,
            DialogResult.Retry => ConflictAction.Rename,
            DialogResult.Cancel => ConflictAction.Cancel,
            _ => ConflictAction.Cancel
        };
    }

    public FileName? ShowRenameDialog(FileName defaultName)
    {
        using var dialog = new RenameDialog(defaultName.GetFullName());
        return dialog.ShowDialog() == DialogResult.OK
            ? FileName.Parse(dialog.NewFileName)
            : null;
    }
}

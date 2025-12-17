using System.Windows.Forms;
using MoveTo.Core.ConflictResolution;

namespace MoveTo.Shell;

internal sealed class ConflictDialog : Form
{
    public ConflictDialog(FileConflict conflict)
    {
        Text = "ファイルの競合";
        Width = 420;
        Height = 220;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var label = new Label
        {
            Text = $"移動先に同名のファイル/フォルダーが存在します。\n{conflict.GetConflictingFilePath()}",
            AutoSize = true,
            Top = 20,
            Left = 20
        };

        var overwrite = new Button { Text = "上書き", DialogResult = DialogResult.Yes, Left = 20, Width = 80, Top = 110 };
        var skip = new Button { Text = "スキップ", DialogResult = DialogResult.No, Left = 110, Width = 80, Top = 110 };
        var rename = new Button { Text = "リネーム", DialogResult = DialogResult.Retry, Left = 200, Width = 80, Top = 110 };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, Left = 290, Width = 80, Top = 110 };

        Controls.Add(label);
        Controls.Add(overwrite);
        Controls.Add(skip);
        Controls.Add(rename);
        Controls.Add(cancel);

        AcceptButton = overwrite;
        CancelButton = cancel;
    }
}

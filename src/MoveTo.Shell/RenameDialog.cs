using System.Windows.Forms;

namespace MoveTo.Shell;

internal sealed class RenameDialog : Form
{
    private readonly TextBox _textBox;

    public string NewFileName => _textBox.Text.Trim();

    public RenameDialog(string defaultName)
    {
        Text = "ファイル名の変更";
        Width = 400;
        Height = 160;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var label = new Label
        {
            Text = "新しいファイル名を入力してください:",
            AutoSize = true,
            Left = 20,
            Top = 15
        };

        _textBox = new TextBox
        {
            Left = 20,
            Top = 40,
            Width = 340,
            Text = defaultName
        };

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 190, Width = 80, Top = 75 };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, Left = 280, Width = 80, Top = 75 };

        Controls.Add(label);
        Controls.Add(_textBox);
        Controls.Add(ok);
        Controls.Add(cancel);

        AcceptButton = ok;
        CancelButton = cancel;
    }
}

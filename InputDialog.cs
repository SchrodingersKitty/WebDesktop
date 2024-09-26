using System.Drawing;
using System.Windows.Forms;

namespace WebDesktop;

public static class InputDialog
{
    public static string Show(IWin32Window? owner, string? caption, string value = "")
    {
        var form = new InputForm
        {
            Text = caption,
            Result = value
        };
        var dialogResult = form.ShowDialog(owner);
        return dialogResult == DialogResult.OK ? form.Result : value;
    }

    private class InputForm : Form
    {
        readonly TextBox textBox = new()
        {
            Width = 400,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        readonly Button okButton = new()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Width = 100,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        readonly Button cancelButton = new()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 100,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        public string Result
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        public InputForm()
        {
            ClientSize = new Size(textBox.Width, textBox.Height + okButton.Height);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            DialogResult = DialogResult.None;
            AcceptButton = okButton;
            CancelButton = cancelButton;
            okButton.Location = new Point(0, textBox.Height);
            cancelButton.Location = new Point(textBox.Width - cancelButton.Width, textBox.Height);
            Controls.Add(textBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
        }
    }
}
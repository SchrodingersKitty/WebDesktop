using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebDesktop;

public class DesktopWindow : Form
{
    public enum DisplayMode
    {
        InFrontOfIcons = 0,
        BehindIcons = 1
    }

    readonly WebView2 webView;
    DisplayMode mode;

    public DesktopWindow()
    {
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;

        webView = new WebView2()
        {
            Dock = DockStyle.Fill,
            CreationProperties = new CoreWebView2CreationProperties()
            {
                IsInPrivateModeEnabled = true
            }
        };
        Controls.Add(webView);
        UpdateClientArea();
    }

    protected unsafe override void WndProc(ref Message m)
    {
        WinDesktopUtils.BackgroundWndProc(ref m);
        base.WndProc(ref m);
    }

    void UpdateClientArea()
    {
        var area = Screen.GetWorkingArea(this);
        Location = area.Location;
        ClientSize = area.Size;
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        this.mode = mode;
        switch(mode)
        {
            case DisplayMode.InFrontOfIcons:
                WinDesktopUtils.SetAsDesktopWindow(this, false);
                UpdateClientArea();
                break;
            case DisplayMode.BehindIcons:
                WinDesktopUtils.SetAsDesktopWindow(this, true);
                UpdateClientArea();
                break;
        }
    }

    public void SetURL(string url)
    {
        webView.Source = new System.Uri(url);
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            WinDesktopUtils.SetAsDesktopWindow(this, false);
        }
        base.Dispose(disposing);
    }
}
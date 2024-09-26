using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebDesktop;

public class MainMenu : IDisposable
{
    readonly NotifyIcon icon;
    readonly DesktopWindow desktopWindow;
    public DesktopWindow DesktopWindow
    {
        get => desktopWindow;
    }

    public MainMenu()
    {
        icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = CreateMenuStrip()
        };
        desktopWindow = new DesktopWindow();
        desktopWindow.SetDisplayMode((DesktopWindow.DisplayMode)Settings.Main.DisplayMode);
        desktopWindow.SetURL(Settings.Main.URL);
    }

    ~MainMenu()
    {
        Dispose(false);
    }

    private ContextMenuStrip CreateMenuStrip()
    {
        var menu = new ContextMenuStrip();
        var items = menu.Items;
        items.Add(new ToolStripMenuItem("Set URL...", null, OnClick_SetURL));
        var submenu = new ToolStripMenuItem("Display");
        submenu.DropDownItems.Add(new ToolStripMenuItem("In front of icons", null, OnClick_Display));
        submenu.DropDownItems.Add(new ToolStripMenuItem("Behind icons", null, OnClick_Display));
        (submenu.DropDownItems[Settings.Main.DisplayMode] as ToolStripMenuItem)!.Checked = true;
        items.Add(submenu);
        items.Add(new ToolStripMenuItem("Exit", null, OnClick_Exit));
        return menu;
    }

    private void OnClick_Display(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.OwnerItem is not ToolStripMenuItem owner) return;
        foreach(ToolStripMenuItem it in owner.DropDownItems)
        {
            it.Checked = false;
        }
        item.Checked = true;
        var index = owner.DropDownItems.IndexOf(item);
        Settings.Main.DisplayMode = index;
        desktopWindow.SetDisplayMode((DesktopWindow.DisplayMode)index);
    }

    private void OnClick_SetURL(object? sender, EventArgs e)
    {
        var url = InputDialog.Show(icon.ContextMenuStrip, "WebView URL", Settings.Main.URL);
        Settings.Main.URL = url;
        desktopWindow.SetURL(url);
    }

    private void OnClick_Exit(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    protected virtual void Dispose(bool disposing)
    {
        if(disposing)
        {
            Settings.Main.Save();
            icon.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
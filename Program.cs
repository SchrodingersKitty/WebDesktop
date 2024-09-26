using System;
using System.Windows.Forms;

namespace WebDesktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var menu = new MainMenu();
        using(menu)
        {
            Application.Run(menu.DesktopWindow);
        }
    }
}
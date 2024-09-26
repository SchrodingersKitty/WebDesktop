using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using SMTO = Windows.Win32.UI.WindowsAndMessaging.SEND_MESSAGE_TIMEOUT_FLAGS;

namespace WebDesktop;

public static class WinDesktopUtils
{
    const string PROGMAN = "Progman";
    const string WORKERW = "WorkerW";
    const string DEFVIEW = "SHELLDLL_DefView";

    /// <summary>
    /// Undocumented Progman message. Causes WorkerW window to be spawned on the layer behind desktop icons.
    /// </summary>
    /// <remarks>
    /// Has extra hidden functionality for managing WorkerW.
    /// wParam is a bit flag, lParam is some kind of argument
    /// probed wParam values:
    /// 0x0 ~ 0x4, 0xE - seem to do nothing
    /// 0x5, 0x6, 0x8, 0x9, 0xA, 0xC - seem to crash Progman
    /// 0xF or higher - returns an explicit error
    /// 0x6 - sometimes returns a different error
    /// 0x7, 0xB - seem to broadcast a RegisterWindowMessage("ForwardMessage") with different values based on lParam, unknown purpose
    /// 0xD - causes WorkerW to repaint if lParam != 0, seems to cause WM_PAINT if lParam = 0 which is ignored by WorkerW
    /// </remarks>
    const uint WM_PROGMAN_WORKERW = 0x052C;
    const int PW_REPAINT = 0xD;

    static readonly HWND hDesktopWorker = HWND.Null;

    static WinDesktopUtils()
    {
        if(CreateDesktopWorker())
        {
            hDesktopWorker = FindDesktopWorker();
        }
    }

    unsafe static bool CreateDesktopWorker()
    {
        // Get handle of Progman
        var hProgman = PInvoke.FindWindow(PROGMAN, null);
        if (hProgman == HWND.Null) return false;

        // Use undocumented message to request a new desktop-layer WorkerW
        var result = PInvoke.SendMessageTimeout(hProgman, WM_PROGMAN_WORKERW, 0, 0, SMTO.SMTO_NORMAL, 1000);
        return result != 0;
    }

    static HWND FindDesktopWorker()
    {
        // We're looking for the next sibling of a WorkerW that hosts SHELLDLL_DefView
        // Yeah, not confusing at all. Surely there's a better way?
        HWND hWorker = new(0);
        do
        {
            hWorker = PInvoke.FindWindowEx(HWND.Null, hWorker, WORKERW, null);
            if(hWorker != 0)
            {
                var hDefView = PInvoke.FindWindowEx(hWorker, HWND.Null, DEFVIEW, null);
                if(hDefView != 0)
                {
                    // Find next sibling
                    hWorker = PInvoke.FindWindowEx(HWND.Null, hWorker, WORKERW, null);
                    break;
                }
            }
        }
        while (hWorker != 0);
        return hWorker;
    }

    public static bool CanDrawOnDesktop
    {
        get => hDesktopWorker != HWND.Null;
    }

    public static void RepaintDesktop()
    {
        var hProgman = PInvoke.FindWindow(PROGMAN, null);
        PInvoke.PostMessage(hProgman, WM_PROGMAN_WORKERW, PW_REPAINT, 1);
    }

    public static void SetAsDesktopWindow(Form form, bool desktopLayer)
    {
        PInvoke.SetParent((HWND)form.Handle, desktopLayer ? hDesktopWorker : HWND.Null);
        RepaintDesktop();
    }

    public unsafe static void BackgroundWndProc(ref Message m)
    {
        if(m.Msg == PInvoke.WM_WINDOWPOSCHANGING)
        {
            var wp = (WINDOWPOS*)m.LParam;
            // Window sticks to the bottom of Z-order
            wp->hwndInsertAfter = (HWND)1;
        }
    }
}
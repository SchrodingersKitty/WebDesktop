## What is this?
A small proof of concept app for Windows that places a webpage as a desktop background.

## How to use?
### Install dependencies
 - [.Net 8](https://aka.ms/get-dotnet-8)
 - [WebView2](https://go.microsoft.com/fwlink/p/?LinkId=2124703)

Running the app initially will produce a black background and a system tray icon.
Right click the icon to navigate to a URL or switch between displaying in front of, or behind desktop icons.

The app retains the URL used as part of its settings. Settings are stored in:
```
%LOCALAPPDATA%\WebDesktop
```

Internet connection is required to load pages.
There is no offline mode, but you may use a `file:///` URL to try loading locally saved html pages.

## How does it work?
By default, Windows places new windows at the top of the Z-order.
Likewise, any window that receives input focus will be moved back on top of all other windows.

To circumvent this, we can use a tiny bit of Win32 API interop to override our window's WndProc and intercept `WM_WINDOWPOSCHANGING` messages.
By writing to the `WINDOWPOS` struct supplied in lParam we can force every Z-order change back to the bottom of the stack.

That is how the app works in its default draw mode (in front of desktop icons) and it is by far the least hacky way to accomplish something like this. It is, however, not good enough.
No amount of Z-order manipulation will place our window behind the desktop icons as the wallpaper background.
Furthermore, pressing the desktop shortcut hides all windows, including ours, as it's clearly not part of the desktop.

Setting aside the question of whether such behaviour is useful or desirable, I would like to nonetheless be able to draw something on the wallpaper layer.
To that effect I have consulted an old CodeProject article[^1] which provided an interesting workaround.

I have linked the full article below but the main takeaway is that we can send an undocumented message `0x052C` to the `Progman` window, which seems to request a `WorkerW` window be created between the icons list and the desktop wallpaper.
The message doesn't return a `HWND` and we don't own the parent process, so it takes a bit of an awkward search to find this new `WorkerW`, but once found we can simply set it as the parent of our window, and utilize it as our drawing layer.

All of this is, admittedly, a very awkward hack. Besides the obvious concern that we've parented a window to a separate process, and that we're depending on hidden undocumented APIs, there are some minor annoyances.
The window behind desktop icons is blocked from receiving input events (this could be solved by sending input messages to our window directly) and the parent window does not seem to invalidate itself automatically.

After sequentially probing combinations of `wParam` and `lParam` values for message `0x052C` on Windows 10 and crashing Progman a bunch in the process, there seem to be options to the command.

`wParam` - appears to be some kind of bit flag
|wParam| |
|-|-|
|`0x0` to `0x4`|are valid, create WorkerW|
|`0x5`, `0x6`, `0x8`, `0x9`, `0xA`, `0xC`|crash Progman|
|`0x6`, `0xF` and higher|fail with an error|
|`0x7`, `0xB`|cause a broadcast of a registered message `ForwardMessage`, likely another part of undocumented functionality|
|`0xD`|if lParam = 0, causes WM_PAINT to be sent to WorkerW, which is then ignored; if lParam > 0, causes WorkerW to immediately repaint|

Short of doing proper reverse-engineering, this seems a useful enough find. Sending `msg=0x052C, wParam=0xD, lParam=1` can be used to repaint the WorkerW window and clean up after ourselves.

## References
[^1]: [Draw Behind Desktop Icons in Windows 8+](https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus)

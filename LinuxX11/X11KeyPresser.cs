#if _LINUX

namespace Typo.Linux;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public sealed class X11KeyPresser : IKeyPresserInterface
{
    private static readonly Dictionary<string, string> NameToKeysym = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KEY_BACK"] = "BackSpace",
        ["KEY_BACKSPACE"] = "BackSpace",
        ["KEY_TAB"] = "Tab",
        ["KEY_CLEAR"] = "Clear",
        ["KEY_RETURN"] = "Return",
        ["KEY_ENTER"] = "Return",
        ["KEY_SHIFT"] = "Shift_L",
        ["KEY_LSHIFT"] = "Shift_L",
        ["KEY_RSHIFT"] = "Shift_R",
        ["KEY_CONTROL"] = "Control_L",
        ["KEY_LCONTROL"] = "Control_L",
        ["KEY_RCONTROL"] = "Control_R",
        ["KEY_ALT"] = "Alt_L",
        ["KEY_LALT"] = "Alt_L",
        ["KEY_RALT"] = "Alt_R",
        ["KEY_MENU"] = "Menu",
        ["KEY_PAUSE"] = "Pause",
        ["KEY_CAPS_LOCK"] = "Caps_Lock",
        ["KEY_ESCAPE"] = "Escape",
        ["KEY_SPACE"] = "space",
        ["KEY_PAGE_UP"] = "Prior",
        ["KEY_PRIOR"] = "Prior",
        ["KEY_PAGE_DOWN"] = "Next",
        ["KEY_NEXT"] = "Next",
        ["KEY_END"] = "End",
        ["KEY_HOME"] = "Home",
        ["KEY_LEFT"] = "Left",
        ["KEY_UP"] = "Up",
        ["KEY_RIGHT"] = "Right",
        ["KEY_DOWN"] = "Down",
        ["KEY_SELECT"] = "Select",
        ["KEY_PRINT"] = "Print",
        ["KEY_SNAPSHOT"] = "Print",
        ["KEY_INSERT"] = "Insert",
        ["KEY_DELETE"] = "Delete",
        ["KEY_HELP"] = "Help",
        ["KEY_APOSTROPHE"] = "apostrophe",
        ["KEY_COMMA"] = "comma",
        ["KEY_PERIOD"] = "period",
        ["KEY_LEFT_BRACKET"] = "bracketleft",
        ["KEY_RIGHT_BRACKET"] = "bracketright",
        ["KEY_SEMICOLON"] = "semicolon",
        ["KEY_EQUALS"] = "equal",
        ["KEY_DASH"] = "minus",
        ["KEY_SLASH"] = "slash",
        ["KEY_BACKSLASH"] = "backslash",
        ["KEY_GRAVE"] = "grave",
        ["KEY_LWIN"] = "Super_L",
        ["KEY_RWIN"] = "Super_R",
        ["KEY_APPS"] = "Menu",
        ["KEY_SLEEP"] = "XF86Sleep",
        ["KEY_NUMPAD0"] = "KP_0",
        ["KEY_NUMPAD1"] = "KP_1",
        ["KEY_NUMPAD2"] = "KP_2",
        ["KEY_NUMPAD3"] = "KP_3",
        ["KEY_NUMPAD4"] = "KP_4",
        ["KEY_NUMPAD5"] = "KP_5",
        ["KEY_NUMPAD6"] = "KP_6",
        ["KEY_NUMPAD7"] = "KP_7",
        ["KEY_NUMPAD8"] = "KP_8",
        ["KEY_NUMPAD9"] = "KP_9",
        ["KEY_MULTIPLY"] = "KP_Multiply",
        ["KEY_ADD"] = "KP_Add",
        ["KEY_SEPARATOR"] = "KP_Separator",
        ["KEY_SUBTRACT"] = "KP_Subtract",
        ["KEY_DECIMAL"] = "KP_Decimal",
        ["KEY_DIVIDE"] = "KP_Divide",
        ["KEY_NUMLOCK"] = "Num_Lock",
        ["KEY_SCROLL"] = "Scroll_Lock"
    };

    private readonly IntPtr _display;

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XStringToKeysym(string @string);

    [DllImport("libX11.so.6")]
    private static extern byte XKeysymToKeycode(IntPtr display, IntPtr keysym);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);

    [DllImport("libXtst.so.6")]
    private static extern int XTestFakeKeyEvent(IntPtr display, uint keycode, bool isPress, ulong delay);

    public static bool IsSupported()
    {
        try
        {
            IntPtr display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
            {
                return false;
            }

            XCloseDisplay(display);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public X11KeyPresser()
    {
        _display = XOpenDisplay(IntPtr.Zero);
    }

    public void PressKey(string key)
    {
        SendKey(key, true);
    }

    public void ReleaseKey(string key)
    {
        SendKey(key, false);
    }

    private void SendKey(string key, bool isPress)
    {
        if (_display == IntPtr.Zero)
        {
            return;
        }

        byte keycode = ResolveKeycode(key);
        if (keycode == 0)
        {
            return;
        }

        XTestFakeKeyEvent(_display, keycode, isPress, 0);
        XFlush(_display);
    }

    private byte ResolveKeycode(string key)
    {
        string keysymName = GetKeysymName(key);
        IntPtr keysym = XStringToKeysym(keysymName);
        if (keysym == IntPtr.Zero)
        {
            return 0;
        }

        return XKeysymToKeycode(_display, keysym);
    }

    private static string GetKeysymName(string key)
    {
        if (NameToKeysym.TryGetValue(key, out string? mapped))
        {
            return mapped;
        }

        if (key.StartsWith("KEY_", StringComparison.OrdinalIgnoreCase))
        {
            string keyName = key["KEY_".Length..];

            if (keyName.Length == 1)
            {
                return keyName.ToLowerInvariant();
            }

            if (keyName.StartsWith("F", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(keyName[1..], out int functionKey)
                && functionKey is >= 1 and <= 35)
            {
                return $"F{functionKey}";
            }
        }

        return key;
    }

    ~X11KeyPresser()
    {
        if (_display != IntPtr.Zero)
        {
            XCloseDisplay(_display);
        }
    }
}

#endif

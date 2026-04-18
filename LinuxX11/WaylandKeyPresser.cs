#if _LINUX

namespace Typo.Linux;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class WaylandKeyPresser : IKeyPresserInterface
{
    private static readonly Dictionary<string, string> NameToWtypeKey = new(StringComparer.OrdinalIgnoreCase)
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

    private static readonly Dictionary<string, int> NameToYdotoolCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KEY_BACK"] = 14,
        ["KEY_BACKSPACE"] = 14,
        ["KEY_TAB"] = 15,
        ["KEY_ENTER"] = 28,
        ["KEY_RETURN"] = 28,
        ["KEY_CONTROL"] = 29,
        ["KEY_LCONTROL"] = 29,
        ["KEY_RCONTROL"] = 97,
        ["KEY_SHIFT"] = 42,
        ["KEY_LSHIFT"] = 42,
        ["KEY_RSHIFT"] = 54,
        ["KEY_ALT"] = 56,
        ["KEY_LALT"] = 56,
        ["KEY_RALT"] = 100,
        ["KEY_SPACE"] = 57,
        ["KEY_CAPS_LOCK"] = 58,
        ["KEY_F1"] = 59,
        ["KEY_F2"] = 60,
        ["KEY_F3"] = 61,
        ["KEY_F4"] = 62,
        ["KEY_F5"] = 63,
        ["KEY_F6"] = 64,
        ["KEY_F7"] = 65,
        ["KEY_F8"] = 66,
        ["KEY_F9"] = 67,
        ["KEY_F10"] = 68,
        ["KEY_NUMLOCK"] = 69,
        ["KEY_SCROLL"] = 70,
        ["KEY_HOME"] = 102,
        ["KEY_UP"] = 103,
        ["KEY_PAGE_UP"] = 104,
        ["KEY_PRIOR"] = 104,
        ["KEY_LEFT"] = 105,
        ["KEY_RIGHT"] = 106,
        ["KEY_END"] = 107,
        ["KEY_DOWN"] = 108,
        ["KEY_PAGE_DOWN"] = 109,
        ["KEY_NEXT"] = 109,
        ["KEY_INSERT"] = 110,
        ["KEY_DELETE"] = 111,
        ["KEY_PAUSE"] = 119,
        ["KEY_LEFT_BRACKET"] = 26,
        ["KEY_RIGHT_BRACKET"] = 27,
        ["KEY_APOSTROPHE"] = 40,
        ["KEY_GRAVE"] = 41,
        ["KEY_BACKSLASH"] = 43,
        ["KEY_COMMA"] = 51,
        ["KEY_PERIOD"] = 52,
        ["KEY_SLASH"] = 53,
        ["KEY_LWIN"] = 125,
        ["KEY_RWIN"] = 126,
        ["KEY_MENU"] = 127,
        ["KEY_APPS"] = 127,
        ["KEY_NUMPAD0"] = 82,
        ["KEY_NUMPAD1"] = 79,
        ["KEY_NUMPAD2"] = 80,
        ["KEY_NUMPAD3"] = 81,
        ["KEY_NUMPAD4"] = 75,
        ["KEY_NUMPAD5"] = 76,
        ["KEY_NUMPAD6"] = 77,
        ["KEY_NUMPAD7"] = 71,
        ["KEY_NUMPAD8"] = 72,
        ["KEY_NUMPAD9"] = 73,
        ["KEY_MULTIPLY"] = 55,
        ["KEY_SUBTRACT"] = 74,
        ["KEY_ADD"] = 78,
        ["KEY_DECIMAL"] = 83,
        ["KEY_DIVIDE"] = 98,
        ["KEY_F11"] = 87,
        ["KEY_F12"] = 88,
        ["KEY_SEMICOLON"] = 39,
        ["KEY_EQUALS"] = 13,
        ["KEY_DASH"] = 12,
        ["KEY_ESCAPE"] = 1
    };

    private readonly string? _wtypePath;
    private readonly string? _ydotoolPath;

    private static readonly Dictionary<char, int> LetterToYdotoolCode = new()
    {
        ['A'] = 30,
        ['B'] = 48,
        ['C'] = 46,
        ['D'] = 32,
        ['E'] = 18,
        ['F'] = 33,
        ['G'] = 34,
        ['H'] = 35,
        ['I'] = 23,
        ['J'] = 36,
        ['K'] = 37,
        ['L'] = 38,
        ['M'] = 50,
        ['N'] = 49,
        ['O'] = 24,
        ['P'] = 25,
        ['Q'] = 16,
        ['R'] = 19,
        ['S'] = 31,
        ['T'] = 20,
        ['U'] = 22,
        ['V'] = 47,
        ['W'] = 17,
        ['X'] = 45,
        ['Y'] = 21,
        ['Z'] = 44
    };

    private static readonly Dictionary<char, int> DigitToYdotoolCode = new()
    {
        ['1'] = 2,
        ['2'] = 3,
        ['3'] = 4,
        ['4'] = 5,
        ['5'] = 6,
        ['6'] = 7,
        ['7'] = 8,
        ['8'] = 9,
        ['9'] = 10,
        ['0'] = 11
    };

    public static bool IsSupported()
    {
        return FindExecutable("wtype") != null || FindExecutable("ydotool") != null;
    }

    public WaylandKeyPresser()
    {
        _wtypePath = FindExecutable("wtype");
        _ydotoolPath = FindExecutable("ydotool");
    }

    public void PressKey(string key)
    {
        if (TrySendWithWtype(key, isPress: true))
        {
            return;
        }

        TrySendWithYdotool(key, isPress: true);
    }

    public void ReleaseKey(string key)
    {
        if (TrySendWithWtype(key, isPress: false))
        {
            return;
        }

        TrySendWithYdotool(key, isPress: false);
    }

    private bool TrySendWithWtype(string key, bool isPress)
    {
        if (_wtypePath == null)
        {
            return false;
        }

        string? mappedKey = GetWtypeKey(key);
        if (mappedKey == null)
        {
            return false;
        }

        string actionFlag = isPress ? "-P" : "-p";
        return RunProcess(_wtypePath, $"{actionFlag} {mappedKey}");
    }

    private bool TrySendWithYdotool(string key, bool isPress)
    {
        if (_ydotoolPath == null)
        {
            return false;
        }

        int? keyCode = GetYdotoolCode(key);
        if (keyCode == null)
        {
            return false;
        }

        int state = isPress ? 1 : 0;
        return RunProcess(_ydotoolPath, $"key {keyCode}:{state}");
    }

    private static string? GetWtypeKey(string key)
    {
        if (NameToWtypeKey.TryGetValue(key, out string? mapped))
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

        return null;
    }

    private static int? GetYdotoolCode(string key)
    {
        if (NameToYdotoolCode.TryGetValue(key, out int mapped))
        {
            return mapped;
        }

        if (key.StartsWith("KEY_", StringComparison.OrdinalIgnoreCase))
        {
            string keyName = key["KEY_".Length..];

            if (keyName.Length == 1)
            {
                char c = char.ToUpperInvariant(keyName[0]);
                if (LetterToYdotoolCode.TryGetValue(c, out int letterCode))
                {
                    return letterCode;
                }

                if (DigitToYdotoolCode.TryGetValue(c, out int digitCode))
                {
                    return digitCode;
                }
            }
        }

        return null;
    }

    private static bool RunProcess(string fileName, string arguments)
    {
        try
        {
            using Process process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            })!;

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static string? FindExecutable(string command)
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = Path.Combine(directory, command);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}

#endif

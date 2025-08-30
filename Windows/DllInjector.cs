using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace Typo.Windows;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static class Injector
{
    #region WinAPI Structs and Constants

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public LUID_AND_ATTRIBUTES[] Privileges;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MODULEENTRY32W
    {
        public int dwSize;
        public int th32ModuleID;
        public int th32ProcessID;
        public int GlblcntUsage;
        public int ProccntUsage;
        public IntPtr modBaseAddr;
        public int modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;
    }

    private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private const uint TOKEN_QUERY = 0x0008;
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    private const string SE_DEBUG_NAME = "SeDebugPrivilege";

    private const uint PROCESS_QUERY_INFORMATION = 0x0400;
    private const uint PROCESS_CREATE_THREAD = 0x0002;
    private const uint PROCESS_VM_OPERATION = 0x0008;
    private const uint PROCESS_VM_WRITE = 0x0020;

    private const uint MEM_COMMIT = 0x00001000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint INFINITE = 0xFFFFFFFF;

    private const uint TH32CS_SNAPMODULE = 0x00000008;

    #endregion

    #region WinAPI Imports

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(
        IntPtr ProcessHandle,
        uint DesiredAccess,
        out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeValue(
        string lpSystemName,
        string lpName,
        out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(
        IntPtr TokenHandle,
        [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
        ref TOKEN_PRIVILEGES NewState,
        uint BufferLength,
        IntPtr PreviousState,
        IntPtr ReturnLength);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
        uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        uint flAllocationType,
        uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFreeEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        uint nSize,
        out uint lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        out uint lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Module32FirstW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Module32NextW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, int th32ProcessID);

    #endregion

    public static bool TakeDebugPrivilege()
    {
        IntPtr process = GetCurrentProcess();
        if (!OpenProcessToken(process, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr token))
        {
            Debug.WriteLine("Failed to open process token");
            return false;
        }

        if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out LUID luid))
        {
            CloseHandle(token);
            Debug.WriteLine("Failed to lookup privilege value");
            return false;
        }

        TOKEN_PRIVILEGES priv = new TOKEN_PRIVILEGES
        {
            PrivilegeCount = 1,
            Privileges = new LUID_AND_ATTRIBUTES[1]
        };
        priv.Privileges[0] = new LUID_AND_ATTRIBUTES
        {
            Luid = luid,
            Attributes = SE_PRIVILEGE_ENABLED
        };

        if (!AdjustTokenPrivileges(token, false, ref priv, 1, IntPtr.Zero, IntPtr.Zero))
        {
            Debug.WriteLine("Failed to adjust token privileges");
        }

        CloseHandle(token);
        CloseHandle(process);
        return true;
    }

    public static bool Inject(int pid, string libPath)
    {
        IntPtr process = IntPtr.Zero;
        IntPtr allocAddress = IntPtr.Zero;
        IntPtr remoteThread = IntPtr.Zero;

        try
        {
            uint accessRights = PROCESS_QUERY_INFORMATION | PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE;
            process = OpenProcess(accessRights, false, pid);
            if (process == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to open process");
                return false;
            }

            int pathSize = (libPath.Length + 1) * 2;
            allocAddress = VirtualAllocEx(process, IntPtr.Zero, (uint)pathSize, MEM_COMMIT, PAGE_READWRITE);
            if (allocAddress == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to allocate memory in target process");
                return false;
            }

            byte[] libPathBytes = Encoding.Unicode.GetBytes(libPath + "\0");
            if (!WriteProcessMemory(process, allocAddress, libPathBytes, (uint)libPathBytes.Length, out _))
            {
                Debug.WriteLine("Failed to write memory in target process");
                return false;
            }

            IntPtr kernel32Handle = GetModuleHandle("Kernel32.dll");
            if (kernel32Handle == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to get Kernel32 handle");
                return false;
            }

            IntPtr loadLibraryAddr = GetProcAddress(kernel32Handle, "LoadLibraryW");
            if (loadLibraryAddr == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to get LoadLibraryW address");
                return false;
            }

            remoteThread = CreateRemoteThread(process, IntPtr.Zero, 0, loadLibraryAddr, allocAddress, 0, out _);
            if (remoteThread == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to create remote thread");
                return false;
            }

            WaitForSingleObject(remoteThread, INFINITE);
            Debug.WriteLine("Successfully injected");
            return true;
        }
        catch (Win32Exception ex)
        {
            Debug.WriteLine($"Win32 error: {ex.Message}");
            return false;
        }
        finally
        {
            if (remoteThread != IntPtr.Zero)
                CloseHandle(remoteThread);
            if (allocAddress != IntPtr.Zero)
                VirtualFreeEx(process, allocAddress, 0, MEM_RELEASE);
            if (process != IntPtr.Zero)
                CloseHandle(process);
        }
    }

    public static void InjectDllInto(string dllPath, string processName, List<string> windowTitles)
    {
        // Convert titles to null-delimited format
        var sb = new StringBuilder();
        foreach (var title in windowTitles) {
            sb.Append(title);
            sb.Append('\0');  // Null delimiter
        }
        sb.Append('\0');  // Double-null termination
        
        // Write to shared memory
        using (var mmf = MemoryMappedFile.CreateOrOpen("Typo_WindowTitles_Mapping", 4096))
        using (var accessor = mmf.CreateViewAccessor(0, 4096))
        {
            byte[] titleBytes = Encoding.ASCII.GetBytes(sb.ToString());
            accessor.WriteArray(0, titleBytes, 0, titleBytes.Length);
            accessor.Write(titleBytes.Length, (byte)0); // Ensure termination
        }
        

        if (!File.Exists(dllPath))
        {
            Debug.WriteLine($"DLL not found: {dllPath}");
            return;
        }

        int pid = PidByName(processName);
        if (pid == 0)
        {
            Debug.WriteLine($"Process not found: {processName}");
            return;
        }

        if (!TakeDebugPrivilege())
        {
            Debug.WriteLine("Failed to take debug privilege");
            return;
        }

        if (Inject(pid, dllPath))
        {
            Debug.WriteLine($"Successfully injected {Path.GetFileName(dllPath)} into {processName}");
        }
    }

    private static int PidByName(string processName)
    {
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                string moduleName = Path.GetFileName(process.MainModule.FileName);
                if (moduleName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    return process.Id;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing process {process.Id}: {ex.Message}");
            }
        }
        return 0;
    }
}

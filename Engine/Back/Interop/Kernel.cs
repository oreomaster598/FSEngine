using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Windows
{
    #region Structs/Enums
    /// <summary>
    /// Flags to set console mode
    /// </summary>
    [Flags]
    public enum ConsoleFlag
    {
        /// <summary>
        /// 
        /// </summary>
        ENABLE_PROCESSED_OUTPUT = 0x0001,
        /// <summary>
        /// 
        /// </summary>
        ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
        /// <summary>
        /// 
        /// </summary>
        VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        /// <summary>
        /// 
        /// </summary>
        DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
        /// <summary>
        /// 
        /// </summary>
        ENABLE_LVB_GRID_WORLDWIDE = 0x0010
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FontInfo
    {
        internal int cbSize;
        internal int FontIndex;
        internal short FontWidth;
        /// <summary>
        /// 
        /// </summary>
        public short FontSize;
        /// <summary>
        /// 
        /// </summary>
        public int FontFamily;
        /// <summary>
        /// 
        /// </summary>
        public int FontWeight;
        /// <summary>
        /// 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FontName;
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        internal short X;
        internal short Y;

        internal COORD(short x, short y)
        {
            X = x;
            Y = y;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT
    {
        internal short Left;
        internal short Top;
        internal short Right;
        internal short Bottom;
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct COLORREF
    {
        internal COLORREF(byte r, byte g, byte b)
        {
            this.Value = 0;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        internal COLORREF(uint value)
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
            this.Value = value & 0x00FFFFFF;
        }

        [FieldOffset(0)]
        internal byte R;
        [FieldOffset(1)]
        internal byte G;
        [FieldOffset(2)]
        internal byte B;

        [FieldOffset(0)]
        internal uint Value;
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SCREEN_BUFFER_INFO_EX
    {
        internal int cbSize;
        internal COORD dwSize;
        internal COORD dwCursorPosition;
        internal ushort wAttributes;
        internal SMALL_RECT srWindow;
        internal COORD dwMaximumWindowSize;
        internal ushort wPopupAttributes;
        internal bool bFullscreenSupported;
        internal COLORREF black;
        internal COLORREF darkBlue;
        internal COLORREF darkGreen;
        internal COLORREF darkCyan;
        internal COLORREF darkRed;
        internal COLORREF darkMagenta;
        internal COLORREF darkYellow;
        internal COLORREF gray;
        internal COLORREF darkGray;
        internal COLORREF blue;
        internal COLORREF green;
        internal COLORREF cyan;
        internal COLORREF red;
        internal COLORREF magenta;
        internal COLORREF yellow;
        internal COLORREF white;
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CONSOLE_FONT_INFO_EX
    {
        internal uint cbSize;
        internal uint nFont;
        internal COORD dwFontSize;
        internal int FontFamily;
        internal int FontWeight;
        internal fixed char FaceName[32];
    }
    #endregion
    public static class Kernel
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int AllocConsole();        
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int FreeConsole();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool ReadConsoleW(SafeFileHandle hConsoleInput, byte* lpBuffer, int nNumberOfCharsToRead, out int lpNumberOfCharsRead, IntPtr pInputControl);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool WriteConsoleW(SafeFileHandle hConsoleOutput, byte* lpBuffer, int nNumberOfCharsToWrite, out int lpNumberOfCharsWritten, IntPtr lpReservedMustBeNull);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleFlag mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out ConsoleFlag mode);
        [DllImport("kernel32")]
        public static extern bool SetConsoleIcon(IntPtr hIcon);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [DllImport("kernel32.dll",
            EntryPoint = "CreateFileW",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateFileW(
              string lpFileName,
              UInt32 dwDesiredAccess,
              UInt32 dwShareMode,
              IntPtr lpSecurityAttributes,
              UInt32 dwCreationDisposition,
              UInt32 dwFlagsAndAttributes,
              IntPtr hTemplateFile
            ); 
        [DllImport("kernel32.dll",
            EntryPoint = "AttachConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 AttachConsole(UInt32 dwProcessId);

        public const UInt32 GENERIC_WRITE = 0x40000000;
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 FILE_SHARE_READ = 0x00000001;
        public const UInt32 FILE_SHARE_WRITE = 0x00000002;
        public const UInt32 OPEN_EXISTING = 0x00000003;
        public const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        public const UInt32 ERROR_ACCESS_DENIED = 5;

        public const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;
    }
}

/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lode {
    /// <summary>Provides system for advanced console output creation.</summary>
    public class Graphics {
        /// <summary>Represents user-friendly color names.</summary>
        public enum Color {
            TextDark = 0,
            TextLight,
            Blue,
            LightBlue,
            Green,
            DarkGreen,
            DarkRed,
            LightRed,
            Ocean,
            Sky,
            Yellow,
            Brown,
            Gray,
            DarkGray,
            Smoke,
            Gold,
            Silver,
            Transparent
        }

        /// <summary>Converts user-friendly color name to combination of foreground and background console colors.</summary>
        private static (ConsoleColor front, ConsoleColor back) GetColorConsoleDefinition(Color color) {
            switch (color) {
                case Color.TextDark:
                    return (ConsoleColor.White, ConsoleColor.Black);
                case Color.TextLight:
                    return (ConsoleColor.Black, ConsoleColor.White);

                case Color.Blue:
                    return (ConsoleColor.Blue, ConsoleColor.Black);
                case Color.LightBlue:
                    return (ConsoleColor.Cyan, ConsoleColor.Black);
                case Color.Ocean:
                    return (ConsoleColor.DarkBlue, ConsoleColor.Blue);
                case Color.Sky:
                    return (ConsoleColor.Black, ConsoleColor.Cyan);

                case Color.Green:
                    return (ConsoleColor.Black, ConsoleColor.Green);
                case Color.DarkGreen:
                    return (ConsoleColor.Green, ConsoleColor.DarkGreen);
                case Color.Yellow:
                    return (ConsoleColor.DarkYellow, ConsoleColor.Yellow);

                case Color.DarkRed:
                    return (ConsoleColor.DarkRed, ConsoleColor.Black);
                case Color.LightRed:
                    return (ConsoleColor.Black, ConsoleColor.Red);
                case Color.Brown:
                    return (ConsoleColor.Yellow, ConsoleColor.DarkYellow);

                case Color.Gray:
                    return (ConsoleColor.DarkGray, ConsoleColor.Gray);
                case Color.DarkGray:
                    return (ConsoleColor.Gray, ConsoleColor.DarkGray);
                case Color.Smoke:
                    return (ConsoleColor.DarkGray, ConsoleColor.Black);

                case Color.Gold:
                    return (ConsoleColor.Yellow, ConsoleColor.Black);
                case Color.Silver:
                    return (ConsoleColor.Gray, ConsoleColor.Black);

                default:
                    return (ConsoleColor.White, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// The serializable struct for wrapping printable content and his color.
        /// For space saving(to not duplicate color for many characters) uses string instead of char.
        /// From words string + pixel = sixel.
        /// </summary>
        [Serializable]
        public struct Sixel {
            public string Content = "";
            public Color Color = 0;

            public Sixel(string content, Color color = 0) {
                Content = content;
                Color = color;
            }

            /// <summary>Sixel lazy constructor.</summary>
            /// <param name="len">Count of <paramref name="symbol"/>.</param>
            /// <param name="color">Color of sixel.</param>
            /// <param name="symbol">Symbol to create content of. By default ' '.</param>
            public Sixel(int len, Color color = 0, char symbol = ' ') {
                Content = new string(symbol, len);
                Color = color;
            }

            /// <summary>Represents content length.</summary>
            public int Length => Content.Length;

            /// <summary>Returns substring of sixel content.</summary>
            public string Substring(int startIndex) => Content.Substring(startIndex);
            /// <summary>Returns substring of sixel content.</summary>
            public string Substring(int startIndex, int length) => Content.Substring(startIndex, length);

            /// <summary>Concats two sixels content.</summary>
            public static Sixel operator +(Sixel a, Sixel b) {
                a.Content += b.Content;
                return a;
            }

            /// <summary>Returns true if sixels have same color and content.</summary>
            public static bool operator ==(Sixel a, Sixel b) {
                return
                    (a.Color == b.Color)
                    && (a.Content.Equals(b.Content))
                    ;
            }
            /// <summary>Returns false if sixels have same color and content.</summary>
            public static bool operator !=(Sixel a, Sixel b) => !(a == b);

            /// <summary>Returns true if sixels have same color and content.</summary>
            public override bool Equals([NotNullWhen(true)] object? obj) {
                if (obj != null && obj.GetType().IsAssignableTo(typeof(Sixel))) {
                    return this == (Sixel)obj;
                }
                return false;
            }
            /// <summary>Returns content hashcode.</summary>
            public override int GetHashCode() => Content.GetHashCode();
        }



        /// <summary>Provides user-friendly functionality arround System.Console and Graphics.</summary>
        public class Console {
            /// <summary>Shortcut to System.Console.WindowWidth.</summary>
            public static int Width => System.Console.WindowWidth;

            /// <summary>Shortcut to System.Console.WindowHeight - 1.</summary>
            public static int Height => System.Console.WindowHeight - 1;

            /// <summary>
            /// Collection of 2D Sixel layers to print. 
            /// Default capacity is 8.
            /// </summary>
            public static List<List<List<Sixel>>> Buffer = new List<List<List<Sixel>>>(8);

            /// <summary>The last printed rendered buffer.</summary>
            private static List<List<Sixel>> oldBaked = new List<List<Sixel>> { };

            /// <summary>Flag signalizes to clear output before next printing.</summary>
            private static bool clearNextDraw = false;



            /// <summary>Prepare console window output before use.</summary>
            public static void Init() {
#if win32
                // Intalling own fonts not works. Console need some more registers to have, not only font in memory.
                // IntPtr x = Unsafe.Fonts.AddFont(Fonts.PressStart2P_Regular);

                Unsafe.Fonts.SetCurrentFont(Shared.Env.ConsoleFontName, new Unsafe.COORD(Shared.Env.ConsoleFontSize.width, Shared.Env.ConsoleFontSize.height));
#endif
                System.Console.CursorVisible = false;
                Clear();
            }

            /// <summary>Clears console window output.</summary>
            public static void Clear() {
                SetColor(0);
                System.Console.Clear();
            }

            public static void ClearNextDraw() => clearNextDraw = true;

            private static void AdjustWindowResolution() {
#if win32
                if (Width < Shared.Env.Width)
                    System.Console.SetWindowSize(Shared.Env.Width, Height);
                if (Height < Shared.Env.Height)
                    System.Console.SetWindowSize(Width, Shared.Env.Height);
#endif
            }

            private static void AddFPSCounterLayer() {
                while (Buffer[Buffer.Count - 1].Count < Height)
                    Buffer[Buffer.Count - 1].Add(new List<Sixel>(0));
                Buffer[Buffer.Count - 1][Height - 1].Add(new($"FPS: {Shared.Env.Lps}"));
            }

            private static void BakeBuffer(out List<List<Sixel>> baked) {
                baked = Buffer[0];
                for (int layer = 1; layer < Buffer.Count; layer++) {
                    for (int row = 0; row < Buffer[layer].Count; row++) {
                        if (row == baked.Count) {
                            baked.Add(Buffer[layer][row]);
                            continue;
                        }

                        for (int col = 0; col < Buffer[layer][row].Count; col++) {
                            int len = (col < baked[row].Count ? baked[row][col].Length : 0);

                            if (Buffer[layer][row][col].Length < len) {
                                Sixel leftover = new(baked[row][col].Substring(Buffer[layer][row][col].Length), baked[row][col].Color);
                                baked[row].Insert(col + 1, leftover);
                                switch (Buffer[layer][row][col].Color) {
                                    case Color.Transparent:
                                        baked[row][col] = new(baked[row][col].Substring(0, Buffer[layer][row][col].Length), baked[row][col].Color);
                                        break;
                                    default:
                                        baked[row][col] = Buffer[layer][row][col];
                                        break;
                                }

                            } else if (Buffer[layer][row][col].Length > len) {
                                if (col >= baked[row].Count)
                                    baked[row].Add(Buffer[layer][row][col]);
                                else {
                                    switch (Buffer[layer][row][col].Color) {
                                        case Color.Transparent:
                                            Buffer[layer][row].Insert(col, new(len));
                                            Buffer[layer][row][col + 1] = new(Buffer[layer][row][col + 1].Length - len, Buffer[layer][row][col + 1].Color);
                                            break;
                                        default:
                                            int count = 1;
                                            while (col + count < baked[row].Count - 1 && Buffer[layer][row][col].Length > baked[row][col + count].Length) {
                                                baked[row][col + count] = new(baked[row][col + count - 1].Content + baked[row][col + count].Content, baked[row][col + count].Color);
                                                count++;
                                            }
                                            baked[row].RemoveRange(col, count);
                                            col--;
                                            break;
                                    }
                                }

                            } else if (Buffer[layer][row][col].Color != Color.Transparent) {
                                baked[row][col] = Buffer[layer][row][col];
                            }
                        }
                    }
                }
            }

            private static bool CanPrintThatBuffer(List<List<Sixel>> buffer) {
                bool print = false;

                if (!Shared.Env.PrintSameBuffer) {
                    if (buffer.Count != oldBaked.Count) {
                        print = true;

                    } else {
                        for (int row = 0; row < buffer.Count && !print; row++) {
                            if (buffer[row].Count != oldBaked[row].Count) {
                                print = true;

                            } else {
                                for (int col = 0; col < buffer[row].Count; col++)
                                    if (buffer[row][col] != oldBaked[row][col]) {
                                        print = true;
                                        break;
                                    }
                            }
                        }
                    }

                    oldBaked = buffer;
                } else {
                    print = true;
                }

                return print;
            }

            private static void CompressBuffer(List<List<Sixel>> buffer) {
                for (int row = 0; row < buffer.Count; row++)
                    // StringBuilder is too complex for more often small purposes
                    for (int col = 0; col < buffer[row].Count - 1; col++)
                        if (buffer[row][col].Color == buffer[row][col + 1].Color) {
                            buffer[row][col] += buffer[row][col + 1];
                            buffer[row].RemoveAt(col + 1);
                            col--;
                        }
            }

            private static void PrintBuffer(List<List<Sixel>> buffer) {
                for (int row = 0; row < buffer.Count; row++) {
                    int colwriten = 0;
                    for (int col = 0; col < buffer[row].Count; col++) {
                        switch (buffer[row][col].Color) {
                            case Color.Transparent:
                                break;
                            default:
                                SetColor(buffer[row][col].Color);
                                SetCursorPosition(colwriten, row);
                                System.Console.Write(buffer[row][col].Content);
                                break;
                        }
                        colwriten += buffer[row][col].Length;
                    }
                }
            }

            /// <summary>
            /// Updates console output buffer by moving cursor and replacing output.
            /// On start sets cursor visibility and window size on windows.
            /// </summary>
            public static void Update() {
                AdjustWindowResolution();

                if (Shared.Sett.FpsCounter)
                    AddFPSCounterLayer();

                BakeBuffer(out List<List<Sixel>> baked);

                if (clearNextDraw) {
                    clearNextDraw = false;
                    Clear();
                }

                if (CanPrintThatBuffer(baked)) {
                    if (Shared.Env.SixelCompression)
                        CompressBuffer(baked);

                    PrintBuffer(baked); // 80ms
                }

                Buffer.Clear();
            }

            /// <summary>Sets console output color.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetColor(Color c) {
                var colors = GetColorConsoleDefinition(c);
                System.Console.ForegroundColor = colors.front;
                System.Console.BackgroundColor = colors.back;
            }

            /// <summary>Sets console cursor position.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetCursorPosition(int x, int y) {
                System.Console.SetCursorPosition(
                    Math.Clamp(x, 0, System.Console.WindowWidth),
                    Math.Clamp(y, 0, System.Console.WindowHeight)
                );
            }

#if win32
            /// <summary>Represents unsafe functions available only for Console class.</summary>
            private static class Unsafe {
                /// <summary>Returns size of struct <typeparamref name="T"/>.</summary>
                private static uint GetStructSize<T>() => (uint)Marshal.SizeOf<T>();

                /// <summary>Represents win32 Point-like struct.</summary>
                [StructLayout(LayoutKind.Sequential)]
                public struct COORD {
                    internal short X;
                    internal short Y;

                    internal COORD(short size) {
                        X = Y = size;
                    }
                    internal COORD(short x, short y) {
                        X = x;
                        Y = y;
                    }
                }

                /// <summary>Represents win32 API application window managing.</summary>
                private static class WindowManager {
                    private const IntPtr invalidHandleValue = -1;
                    private const int stdOutputHandle = -11;

                    public static readonly IntPtr ConsoleOutputHandle = GetConsoleHandle();

                    [DllImport("kernel32.dll", SetLastError = true)]
                    private static extern IntPtr GetStdHandle(int nStdHandle);

                    public static bool IsValidHandle(IntPtr h) => (h != invalidHandleValue);
                    public static IntPtr GetConsoleHandle() => GetStdHandle(stdOutputHandle);
                }

                /// <summary>Represents win32 API messages.</summary>
                private static class Messages {
                    public const int WM_FONTCHANGE = 0x001D;
                    public const int HWND_BROADCAST = 0xffff;

                    [DllImport("user32.dll")]
                    public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
                }

                /// <summary>Represents fonts manager class.</summary>
                public static class Fonts {
                    private const int FixedWidthTrueType = 54;

                    /// <summary>Represents win32 API font info struct.</summary>
                    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
                    public unsafe struct FontInfo {
                        private uint StructSize;
                        internal uint FontIndex;
                        public COORD FontSize;
                        public int FontFamily;
                        public int FontWeight;
                        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                        public string? FontName;

                        public FontInfo() {
                            StructSize = GetStructSize<FontInfo>();
                        }
                        public FontInfo(uint index, COORD size, int family, int weight, string name) {
                            StructSize = GetStructSize<FontInfo>();
                            FontIndex = index;
                            FontSize = size;
                            FontFamily = family;
                            FontWeight = weight;
                            FontName = name;
                        }
                    }


                    [return: MarshalAs(UnmanagedType.Bool)]
                    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                    private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

                    // TODO GetConsoleDisplayMode()
                    public static bool GetCurrentConsoleFont(ref FontInfo font) => GetCurrentConsoleFontEx(WindowManager.ConsoleOutputHandle, false, ref font);


                    [return: MarshalAs(UnmanagedType.Bool)]
                    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                    private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);
                    // TODO GetConsoleDisplayMode()
                    public static bool SetCurrentConsoleFont(ref FontInfo font) => SetCurrentConsoleFontEx(WindowManager.ConsoleOutputHandle, false, ref font);

                    public static FontInfo[] SetCurrentFont(string fontName, COORD fontSize, int fontWeight = 400) {
                        FontInfo before = new();
                        if (GetCurrentConsoleFont(ref before)) {
                            COORD setFontSize = new COORD(
                                fontSize.X > 0 ? fontSize.X : before.FontSize.X,
                                fontSize.Y > 0 ? fontSize.Y : before.FontSize.Y
                                );
                            FontInfo set = new FontInfo {
                                FontIndex = 0,
                                FontFamily = FixedWidthTrueType,
                                FontName = fontName,
                                FontWeight = fontWeight,
                                FontSize = setFontSize,
                            };

                            if (!SetCurrentConsoleFont(ref set)) {
                                var ex = Marshal.GetLastWin32Error();
                                throw new System.ComponentModel.Win32Exception(ex);
                            }

                            FontInfo after = new();
                            GetCurrentConsoleFont(ref after);
                            return new[] { before, set, after };
                        } else {
                            var er = Marshal.GetLastWin32Error();
                            throw new System.ComponentModel.Win32Exception(er);
                        }
                    }


                    [DllImport("gdi32.dll", SetLastError = true)]
                    private static extern IntPtr AddFontMemResourceEx(IntPtr pFileView, uint cjSize, IntPtr pvResrved, [In] ref uint pNumFonts);

                    public static IntPtr AddFont(byte[] fontResource, uint fontsCount = 1) {
                        IntPtr result;
                        unsafe {
                            fixed (byte* p = fontResource) {
                                result = AddFontMemResourceEx((IntPtr)p, (uint)fontResource.Length, 0, ref fontsCount);
                            }
                        }
                        Messages.SendMessage(Messages.HWND_BROADCAST, Messages.WM_FONTCHANGE, 0, 0);
                        return result;
                    }
                }
            }
#endif
        }
    }
}

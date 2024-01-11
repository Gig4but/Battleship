/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using System.Text;

namespace Lode.Shared {
    /// <summary>Represent template for const (T from, T to).</summary>
    public struct Range<T> {
        public T From { get; private set; }
        public T To { get; private set; }

        public Range(T from, T to) {
            From = from;
            To = to;
        }
    }



    /// <summary>Represent namespace of shared Environment variables.</summary>
    public class Env {
        /// <summary>If true, main loop will safely print exception message.</summary>
        public static readonly bool DoNotThrow = true;

        /// <summary>FPS limit.</summary>
        public static readonly uint FpsLimit = 60;
        /// <summary>Minimal delay between ticks.</summary>
        public static readonly long TickDelay = TimeSpan.TicksPerSecond / FpsLimit;


        /// <summary>Flag to decide if print same buffer.</summary>
        public static readonly bool PrintSameBuffer = false;
        /// <summary>Flag to decide if concat sixels with same color.</summary>
        public static readonly bool SixelCompression = true;
        /// <summary>Exact width of console window.</summary>
        public static readonly int Width = 120;
        /// <summary>Exact height of console window.</summary>
        public static readonly int Height = 30;


        /// <summary>Flag decides if main program loop running.</summary>
        public static bool Run = true;
        /// <summary>Actual and old system time in ticks.</summary>
        public static (long Actual, long Old) Timer = (0, 0);
        /// <summary>Count of loops in actual second.</summary>
        public static long Loop = 0;
        /// <summary>Count of miliseconds in actual second.</summary>
        public static long Sec = 0;
        /// <summary>Loops per second counter.</summary>
        public static long Lps = FpsLimit;
        /// <summary>Actual delta between ticks in miliseconds.</summary>
        public static long TickDelta = 0;
        /// <summary>Flag decides if last keypress cleared bindings.</summary>
        public static bool ResetControls = false;


        /// <summary>Console font name.</summary>
        public static readonly string ConsoleFontName = "Terminal";
        /// <summary>Console font size.</summary>
        public static readonly (short width, short height) ConsoleFontSize = (12, 16);


        /// <summary>The instance to random generator.</summary>
        public static readonly Random Rnd = new Random(DateTime.UtcNow.Millisecond);

        /// <summary>The port for network communication.</summary>
        public static readonly int Port = 25555;
        /// <summary>The main(wrapper) crypto key for network communication (needs to be 256bits).</summary>
        public static readonly byte[] MainCryptoKey = Encoding.ASCII.GetBytes("OlegPetrunyMFFCourseProgramAC#23");
        /// <summary>Send try count for network communication before drop client.</summary>
        public static readonly int MaxTryCount = 3;
        /// <summary>The length of firt key for communication inititation.</summary>
        public static readonly int InviteKeyLength = 4;
    }



    /// <summary>Represent namespace of shared Settings variables.</summary>
    public class Sett {
        /// <summary>Random finest multiplier.</summary>
        //public static readonly float RandomFinest = 1f;

        /// <summary>Maximal player count.</summary>
        public static readonly int MaxPlayerCount = 5;


        /// <summary>Actual player map size.</summary>
        public static (int x, int y) MapSize = (15, 15);

        /// <summary>Min and Max player map size.</summary>
        public static Range<(int x, int y)> MapSizeRange = new((11, 11), (20, 20));

        /// <summary>Actual speed of AI cursor animation.</summary>
        public static int AISpeed = 3;

        /// <summary>Min and Max speed of AI cursor animation.</summary>
        public static Range<int> AISpeedRange = new Range<int>(1, 10);


        /// <summary>Flag decides if show FPS counter.</summary>
        public static bool FpsCounter = true;
    }



    /// <summary>Represent namespace of shared Functions.</summary>
    public class Func {
        /// <summary>Breaks program main loop.</summary>
        public static void ExitApplication() {
            Env.Run = false;
        }

        /// <summary>Returns variation of from symbols.</summary>
        /// <param name="symbols">Collection of symbols to create variation.</param>
        /// <param name="length">The length of output.</param>
        public static string RandomCharVariation(char[] symbols, int length) {
            if (length < 8) {
                string result = "";
                for (int i = 0; i < length; i++) {
                    result += symbols[Env.Rnd.Next(0, symbols.Length)];
                }
                return result;
            }

            StringBuilder sb = new(length);
            for (int i = 0; i < length; i++) {
                sb.Append(symbols[Env.Rnd.Next(0, symbols.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>Creates copy of any serializable data.</summary>
        // https://stackoverflow.com/questions/27208411/how-to-clone-multidimensional-array-without-reference-correctly
        [Obsolete("This is slow (avg. 800ns), for sprites use CopySprite instead (avg. 10ns)")]
        public static T? CreateSerializedCopy<T>(T oRecordToCopy) {
            // Exceptions are handled by the caller

            if (oRecordToCopy == null) {
                return default(T);
            }

            if (!oRecordToCopy.GetType().IsSerializable) {
                throw new ArgumentException(oRecordToCopy.GetType().ToString() + " is not serializable");
            }

            var oFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using (var oStream = new MemoryStream()) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                oFormatter.Serialize(oStream, oRecordToCopy);
                oStream.Position = 0;
                return (T)oFormatter.Deserialize(oStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
        }

        public static List<List<Graphics.Sixel>> CopySprite(List<List<Graphics.Sixel>> original) {
            List<List<Graphics.Sixel>> copy = new(original.Count);
            for (int i = 0; i < original.Count; i++) {
                copy.Add(new(original[i].Count));
                for (int j = 0; j < original[i].Count; j++) {
                    copy[i].Add(new Graphics.Sixel(new(original[i][j].Content), original[i][j].Color));
                }
            }
            return copy;
        }
    }
}

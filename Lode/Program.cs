/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]

namespace Lode {
    /// <summary>Represents main program class.</summary>
    internal class Program {
        /// <summary>Represents main program function.</summary>
        static void Main() {
            try {
                /* init *********************************/
                Graphics.Console.Init();
                Scenes.Scene.Set(new Scenes.Home());

                /* main loop ****************************/
                while (Shared.Env.Run) {
                    /* stabilization ************************/
                    Shared.Env.Timer = (DateTime.UtcNow.Ticks, Shared.Env.Timer.Actual); // UTCNow is more faster, no timezone needed
                    Shared.Env.TickDelta = Shared.Env.Timer.Actual - Shared.Env.Timer.Old;

                    if (Shared.Env.TickDelta < Shared.Env.TickDelay)
                        Thread.Sleep((int)((Shared.Env.TickDelay - Shared.Env.TickDelta) / TimeSpan.TicksPerMillisecond));

                    Shared.Env.Sec += Shared.Env.TickDelay;
                    Shared.Env.Loop++;

                    if (Shared.Env.Sec > TimeSpan.TicksPerSecond) {
                        Shared.Env.Lps = Shared.Env.Loop;
                        Shared.Env.Loop = 0;
                        Shared.Env.Sec -= TimeSpan.TicksPerSecond;
                    }

                    /* input ********************************/
                    Controls.ProcessInput();

                    /* graphics *****************************/
                    Scenes.Scene.Actual?.Draw(Graphics.Console.Buffer);
                    Graphics.Console.Update();
                }
            } catch (Exception ex) {
                if (!Shared.Env.DoNotThrow)
                    throw;
                Console.WriteLine($"Error has occured!\n{ex.Message}");
            }
        }
    }
}
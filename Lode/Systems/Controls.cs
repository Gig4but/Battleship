/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

namespace Lode {
    /// <summary>Provides binding of keyboard key to void() delegate.</summary>
    class Controls {
        /// <summary>Represents wraper for (long Border, long Value).</summary>
        public struct Counter {
            public long Border, Value = 0;
            public Counter(long border, long value = 0) {
                Border = border;
                Value = value;
            }
        }


        /// <summary>Default delay to next reaction to key press.</summary>
        private const uint defaultDelay = 100;

        /// <summary>Collection of all keys to actions bindings with delays.</summary>
        private static Dictionary<ConsoleKey, (Counter delay, List<Action> actions)> bindings = new Dictionary<ConsoleKey, (Counter, List<Action>)>();


        /// <summary>Binds function with key and sets new key delay.</summary>
        /// <param name="key">Key to bind.</param>
        /// <param name="action">Function to bind.</param>
        /// <param name="delay">New delay for key press.</param>
        public static void Bind(ConsoleKey key, Action action, uint delay = defaultDelay) {
            if (bindings.ContainsKey(key)) {
                bindings[key].actions.Add(action);
                bindings[key] = (new Counter(delay), bindings[key].actions);
            } else {
                bindings.Add(key, (new Counter(delay), new List<Action>() { action }));
            }
        }

        /// <summary>Clears binding collection and sets <c>Shared.Env.ResetControls</c> flag to true.</summary>
        public static void RemoveAllBinds() {
            bindings.Clear();
            Shared.Env.ResetControls = true;
        }

        /// <summary>
        /// Tries to catch and process key press.
        /// If <c>Shared.Env.ResetControls</c> is true, sets to false and do nothing.
        /// </summary>
        public static void ProcessInput() {
            if (Shared.Env.ResetControls) { // skip one tick
                Shared.Env.ResetControls = false;
                return;
            }

            foreach (KeyValuePair<ConsoleKey, (Counter delay, List<Action> func)> bind in bindings) {
                bindings[bind.Key] = (new Counter(bind.Value.delay.Border, bind.Value.delay.Value + Shared.Env.TickDelta), bind.Value.func);
            }

            if (Console.KeyAvailable) {
                ConsoleKey key = Console.ReadKey(true).Key;

                if (bindings.ContainsKey(key) && bindings[key].delay.Border < bindings[key].delay.Value) {
                    bindings[key] = (new Counter(bindings[key].delay.Border), bindings[key].actions);

                    for (int i = 0; i < bindings[key].actions.Count(); i++) {
                        bindings[key].actions[i].Invoke();

                        if (Shared.Env.ResetControls) {
                            Shared.Env.ResetControls = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}

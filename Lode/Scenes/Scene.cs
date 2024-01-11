/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using static Lode.Graphics;

namespace Lode.Scenes {
    /// <summary>Represents abstract scene class.</summary>
    public abstract class Scene {
        /// <summary>Actual scene instance.</summary>
        public static Scene? Actual { get; private set; }

        /// <summary>Sets scene without any transition.</summary>
        public static bool Set(Scene newScene) {
            Actual = newScene;
            return true;
        }

        /// <summary>Abstract into-buffer draw method.</summary>
        /// <param name="buffer">Reference to buffer to write.</param>
        public abstract void Draw(List<List<List<Sixel>>> buffer);

        /// <summary>Base constructor removes all controls binds and clears next frame.</summary>
        public Scene() {
            Controls.RemoveAllBinds();
            Graphics.Console.ClearNextDraw();
        }

        /// <summary>Allows custom change transitions.</summary>
        public virtual bool Change(Scene newScene) => Set(newScene);

        /// <summary>Custom object for passing any data to new scene.</summary>
        protected object? AfterchangeData = null;
    }
}

/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using static Lode.Graphics;

namespace Lode.Scenes {
    /// <summary>Represents scene with game instance.</summary>
    public class Battleships : Scene {
        /// <summary>Control picker.</summary>
        private enum Control {
            Menu = 0,
            Game,
        }
        private static readonly int _controlCount = Enum.GetNames(typeof(Control)).Length;



        /// <summary>Actual control.</summary>
        private Control control = Control.Game;

        /// <summary>Menu cursor position.</summary>
        private int cursor = 0;

        /// <summary>Collection of menu buttons.</summary>
        private readonly string[] buttons = { "New game", "Exit" };

        /// <summary>Game instance.</summary>
        private Lode game;



        /// <summary>Game scene constructor.</summary>
        public Battleships() {
            game = new Lode();

            Controls.Bind(ConsoleKey.UpArrow, CursorUp);
            Controls.Bind(ConsoleKey.DownArrow, CursorDown);
            Controls.Bind(ConsoleKey.LeftArrow, CursorLeft);
            Controls.Bind(ConsoleKey.RightArrow, CursorRight);
            Controls.Bind(ConsoleKey.Tab, NextControl);
            Controls.Bind(ConsoleKey.Spacebar, Action, 200);
            Controls.Bind(ConsoleKey.Enter, Enter, 300);

            Controls.Bind(ConsoleKey.Q, Q);
            Controls.Bind(ConsoleKey.W, W);
            Controls.Bind(ConsoleKey.E, E);
            Controls.Bind(ConsoleKey.R, R);

            Controls.Bind(ConsoleKey.D1, D1);
            Controls.Bind(ConsoleKey.D2, D2);
            Controls.Bind(ConsoleKey.D3, D3);
            Controls.Bind(ConsoleKey.D4, D4);
            Controls.Bind(ConsoleKey.D5, D5);
            Controls.Bind(ConsoleKey.D6, D6);
            Controls.Bind(ConsoleKey.D7, D7);
            Controls.Bind(ConsoleKey.D8, D8);
            Controls.Bind(ConsoleKey.D9, D9);
            Controls.Bind(ConsoleKey.D0, D0);
            Controls.Bind(ConsoleKey.NumPad1, D1);
            Controls.Bind(ConsoleKey.NumPad2, D2);
            Controls.Bind(ConsoleKey.NumPad3, D3);
            Controls.Bind(ConsoleKey.NumPad4, D4);
            Controls.Bind(ConsoleKey.NumPad5, D5);
            Controls.Bind(ConsoleKey.NumPad6, D6);
            Controls.Bind(ConsoleKey.NumPad7, D7);
            Controls.Bind(ConsoleKey.NumPad8, D8);
            Controls.Bind(ConsoleKey.NumPad9, D9);
            Controls.Bind(ConsoleKey.NumPad0, D0);

            Controls.Bind(ConsoleKey.OemPeriod, Dot);
            Controls.Bind(ConsoleKey.Decimal, Dot);

            Controls.Bind(ConsoleKey.Backspace, Backspace);
        }

        /// <summary>Abstract into-buffer draw method.</summary>
        /// <param name="buffer">Reference to buffer to write.</param>
        public override void Draw(List<List<List<Sixel>>> buffer) {
            game.Draw(buffer);

            int layer = buffer.Count;
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            for (int i = buffer[layer].Count; i < Graphics.Console.Height - 3; i++)
                buffer[layer].Add(new List<Sixel>(0));

            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { new("#", Color.Blue), new("TIP: Use [TAB] for switch between controls") },
                new List<Sixel> { new(Graphics.Console.Width, Color.Blue, '#') },
                new List<Sixel> { new(8, Color.Blue, '#') }
            });

            for (int i = 0; i < buttons.Length; i++) {
                if (control == Control.Menu && cursor == i)
                    buffer[layer][buffer[layer].Count - 1].AddRange(new List<Sixel> { new("#", Color.Blue), new($"<{buttons[i]}>", Color.TextLight), new("#", Color.Blue) });
                else
                    buffer[layer][buffer[layer].Count - 1].AddRange(new List<Sixel> { new("#", Color.Blue), new($"<{buttons[i]}>"), new("#", Color.Blue) });
            }
        }

        /// <summary>"Clicks" on button.</summary>
        private void Enter() {
            switch (control) {
                case Control.Menu:
                    switch (cursor) {
                        case 0:
                            game.Dispose();
                            Change(new Battleships());
                            break;
                        case 1:
                            game.Dispose();
                            Change(new Home());
                            break;
                    }
                    break;
                case Control.Game:
                    game.ConfirmSettings();
                    break;
            }
        }

        /// <summary>Moves cursor up.</summary>
        private void CursorUp() {
            switch (control) {
                case Control.Game:
                    game.CursorUp();
                    break;
            }
        }

        /// <summary>Moves cursor down.</summary>
        private void CursorDown() {
            switch (control) {
                case Control.Game:
                    game.CursorDown();
                    break;
            }
        }

        /// <summary>Moves cursor left.</summary>
        private void CursorLeft() {
            switch (control) {
                case Control.Menu:
                    if (cursor > 0)
                        cursor--;
                    else
                        cursor = buttons.Count() - 1;
                    break;
                case Control.Game:
                    game.CursorLeft();
                    break;
            }
        }

        /// <summary>Moves cursor right.</summary>
        private void CursorRight() {
            switch (control) {
                case Control.Menu:
                    if (cursor < buttons.Count() - 1)
                        cursor++;
                    else
                        cursor = 0;
                    break;
                case Control.Game:
                    game.CursorRight();
                    break;
            }
        }

        /// <summary>Change actual control.</summary>
        private void NextControl() {
            control = (Control)(((int)control + 1) % _controlCount);
            if (control == Control.Game)
                game.ShowCursor = true;
            else
                game.ShowCursor = false;
        }

        /// <summary>Perform in-game action.</summary>
        private void Action() {
            game.Space();
        }

        private void Q() => game.CharKey('Q');
        private void W() => game.CharKey('W');
        private void E() => game.CharKey('E');
        private void R() => game.CharKey('R');

        private void D1() => game.CharKey('1');
        private void D2() => game.CharKey('2');
        private void D3() => game.CharKey('3');
        private void D4() => game.CharKey('4');
        private void D5() => game.CharKey('5');
        private void D6() => game.CharKey('6');
        private void D7() => game.CharKey('7');
        private void D8() => game.CharKey('8');
        private void D9() => game.CharKey('9');
        private void D0() => game.CharKey('0');

        private void Dot() => game.CharKey('.');

        private void Backspace() => game.CharKey('\0');

    }
}

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
    /// <summary>Represents home menu of game.</summary>
    public class Home : Scene {
        /// <summary>Cursor position.</summary>
        private int cursor = 0;

        /// <summary>Collection of menu buttons.</summary>
        private readonly string[] buttons = { "PLAY", "OPTIONS", "EXIT" };



        /// <summary>Home menu constructor.</summary>
        public Home() {
            Controls.Bind(ConsoleKey.UpArrow, CursorUp);
            Controls.Bind(ConsoleKey.DownArrow, CursorDown);
            Controls.Bind(ConsoleKey.Enter, Enter, 200);
        }



        /// <summary>PerTick ship sprite animation counter.</summary>
        private int _shipState = 0;

        /// <summary>Border where to change ship frame id.</summary>
        private const int _shipStateBorder = 30;

        /// <summary>The id of ship frame.</summary>
        private int _shipFrame = 0;

        /// <summary>Abstract into-buffer draw method.</summary>
        /// <param name="buffer">Reference to buffer to write.</param>
        public override void Draw(List<List<List<Sixel>>> buffer) {
            int layer = buffer.Count;

            //print border
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            buffer[layer].Add(new List<Sixel> { new(Graphics.Console.Width, Color.Blue, '#') });
            for (int i = buffer[layer].Count; i < Graphics.Console.Height - 1; i++)
                buffer[layer].Add(new List<Sixel> { new("#", Color.Blue), new(Graphics.Console.Width - 2, Color.Sky), new("#", Color.Blue) });
            buffer[layer].Add(new List<Sixel> { new(Graphics.Console.Width, Color.Blue, '#') });

            //print waves
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            layer++;
            for (int i = buffer[layer].Count; i < Graphics.Console.Height - 6; i++)
                buffer[layer].Add(new List<Sixel>(0));

            //List<Sixel> upperWave = new List<Sixel> { new("    ", Color.Transparent), new("= = =", Color.Ocean), new("    ", Color.Transparent) };
            //List<Sixel> bottomWave = new List<Sixel> { new("= =       = =", Color.Ocean) };

            for (int i = buffer[layer].Count; i < Graphics.Console.Height - 1; i++)
                buffer[layer].Add(new List<Sixel> { new(1, Color.Transparent), new(Graphics.Console.Width - 2, Color.Ocean), new(1, Color.Transparent) });

            //print tree
            buffer.Add(Shared.Func.CopySprite(Sprites.Tree));

            //print ship
            layer++;
            List<List<Sixel>> shipCopy = Shared.Func.CopySprite(Sprites.Ship[_shipFrame]);
            for (int i = 0; i < shipCopy.Count; i++) {
                int row = Graphics.Console.Height - _shipFrame - 10 + i;
                int len = 0;
                for (int j = 0; j < buffer[layer][row].Count; j++)
                    len += buffer[layer][row][j].Length;
                buffer[layer][row].Add(new(65 - len, Color.Transparent));
                buffer[layer][row].AddRange(shipCopy[i]);
            }
            _shipState++;
            if (_shipState >= _shipStateBorder) {
                _shipState = 0;
                _shipFrame++;
                if (_shipFrame >= Sprites.Ship.Length)
                    _shipFrame = 0;
            }



            //print buttons
            layer++;
            Graphics.Console.Buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            for (int i = buffer[layer].Count; i < 8; i++)
                buffer[layer].Add(new List<Sixel>(0));

            for (int i = 0; i < buttons.Length; i++) {
                if (i == cursor) {
                    buffer[layer].AddRange(new List<List<Sixel>> {
                        new List<Sixel> { new(28, Color.Transparent), new($"/{new string('#', 8)}\\", Color.TextLight) },
                        new List<Sixel> { new(28, Color.Transparent), new($"#{buttons[i]}{new string(' ', 8 - buttons[i].Length)}#", Color.TextLight) },
                        new List<Sixel> { new(28, Color.Transparent), new($"\\{new string('#', 8)}/", Color.TextLight) }
                    });
                } else {
                    buffer[layer].AddRange(new List<List<Sixel>> {
                        new List<Sixel> { new(28, Color.Transparent), new(10, symbol: '#') },
                        new List<Sixel> { new(28, Color.Transparent), new($"#{buttons[i]}{new string(' ', 8 - buttons[i].Length)}#") },
                        new List<Sixel> { new(28, Color.Transparent), new(10, symbol: '#') }
                    });
                }
                buffer[layer].Add(new List<Sixel> { });
            }

        }

        /// <summary>Moves cursor up.</summary>
        private void CursorUp() {
            if (cursor > 0)
                cursor--;
            else
                cursor = buttons.Count() - 1;
        }

        /// <summary>Moves cursor down.</summary>
        private void CursorDown() {
            if (cursor < buttons.Count() - 1)
                cursor++;
            else
                cursor = 0;
        }

        /// <summary>"Clicks" on button.</summary>
        private void Enter() {
            switch (cursor) {
                case 0:
                    Change(new Battleships());
                    break;
                case 1:
                    Change(new Options());
                    break;
                case 2:
                    Shared.Func.ExitApplication();
                    break;
                default:
                    Change(new Home());
                    break;
            }
        }

    }
}

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
    /// <summary>Represents options menu.</summary>
    public class Options : Scene {
        /// <summary>Cursor position.</summary>
        private int cursor = 0;


        /// <summary>Options menu constructor.</summary>
        public Options() {
            Controls.Bind(ConsoleKey.UpArrow, CursorUp);
            Controls.Bind(ConsoleKey.DownArrow, CursorDown);
            Controls.Bind(ConsoleKey.LeftArrow, CursorLeft);
            Controls.Bind(ConsoleKey.RightArrow, CursorRight);
            Controls.Bind(ConsoleKey.Enter, Enter, 300);
        }



        /// <summary>Fish direction.</summary>
        private bool _fishToRight = true;

        /// <summary>On-screen Fish position offset.</summary>
        private int _fishOffset = 0;

        /// <summary>Fish offset border to know when change fish direction.</summary>
        private const int _fishOffsetBorder = 5;

        /// <summary>PerTick counter for fish animation.</summary>
        private int _fishState = 0;

        /// <summary>Border to know when to change fish offset.</summary>
        private const int _fishStateBorder = 15;

        /// <summary>Abstract into-buffer draw method.</summary>
        /// <param name="buffer">Reference to buffer to write.</param>
        public override void Draw(List<List<List<Sixel>>> buffer) {
            int layer = buffer.Count;

            //print border
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            buffer[layer].Add(new List<Sixel> { new(Graphics.Console.Width, Color.Blue, '#') });
            for (int i = 0; i < Graphics.Console.Height - 2; i++)
                buffer[layer].Add(new List<Sixel> { new("#", Color.Blue), new(Graphics.Console.Width - 2, Color.Ocean), new("#", Color.Blue) });
            buffer[layer].Add(new List<Sixel> { new(Graphics.Console.Width, Color.Blue, '#') });

            //print fish
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            layer++;
            for (int i = 0; i < 15; i++)
                buffer[layer].Add(new List<Sixel>(0));

            List<List<Sixel>> fishCopy = Shared.Func.CopySprite(Sprites.Fish[_fishToRight ? 0 : 1]);
            int colStart = buffer[layer].Count - fishCopy.Count;
            for (int i = 0; i < fishCopy.Count; i++) {
                buffer[layer][colStart + i].AddRange(new List<Sixel> { new(30 + _fishOffset, Color.Transparent) });
                buffer[layer][colStart + i].AddRange(fishCopy[i]);
            }

            _fishState++;
            if (_fishState >= _fishStateBorder) {
                _fishState = 0;
                if (_fishToRight) {
                    _fishOffset++;
                    if (_fishOffset >= _fishOffsetBorder)
                        _fishToRight = false;
                } else {
                    _fishOffset--;
                    if (_fishOffset <= 0)
                        _fishToRight = true;
                }
            }

            //print buttons
            buffer.Add(new List<List<Sixel>>(Graphics.Console.Height));
            layer++;
            buffer[layer].AddRange(new List<List<Sixel>> {
                    new List<Sixel> { },
                    new List<Sixel> { },
                    new List<Sixel> { new(" ", Color.Transparent), new($"[v^] Computer speed:  <{Shared.Sett.AISpeed}>", cursor == 0 ? Color.TextLight : Color.TextDark), new("      ", Color.Ocean) },
                    new List<Sixel> { },
                    new List<Sixel> { new(" ", Color.Transparent), new($"[v^] Show FPS:        <{(Shared.Sett.FpsCounter ? "True" : "False")}>", cursor == 1 ? Color.TextLight : Color.TextDark), new("      ", Color.Ocean) },
                    new List<Sixel> { },
                    new List<Sixel> { },
                    new List<Sixel> { },
                    new List<Sixel> { new(" ", Color.Transparent), new("Press [ENTER] for return to main menu") },
                });
        }

        /// <summary>Returns to home menu.</summary>
        private void Enter() {
            Change(new Home());
        }

        /// <summary>Moves cursor up.</summary>
        private void CursorUp() {
            if (cursor > 0)
                cursor--;
            else
                cursor = 1;
        }

        /// <summary>Moves cursor down.</summary>
        private void CursorDown() {
            if (cursor < 1)
                cursor++;
            else
                cursor = 0;
        }

        /// <summary>Decrease setting value.</summary>
        private void CursorLeft() {
            switch (cursor) {
                case 0:
                    if (Shared.Sett.AISpeed > Shared.Sett.AISpeedRange.From)
                        Shared.Sett.AISpeed--;
                    else
                        Shared.Sett.AISpeed = Shared.Sett.AISpeedRange.To;
                    break;
                case 1:
                    Shared.Sett.FpsCounter = Shared.Sett.FpsCounter ? false : true;
                    break;
            }
        }

        /// <summary>Increase setting value.</summary>
        private void CursorRight() {
            switch (cursor) {
                case 0:
                    if (Shared.Sett.AISpeed < Shared.Sett.AISpeedRange.To)
                        Shared.Sett.AISpeed++;
                    else
                        Shared.Sett.AISpeed = Shared.Sett.AISpeedRange.From;
                    break;
                case 1:
                    Shared.Sett.FpsCounter = Shared.Sett.FpsCounter ? false : true;
                    break;
            }
        }
    }
}

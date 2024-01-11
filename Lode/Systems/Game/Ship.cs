/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using static Lode.Graphics;

namespace Lode.Systems.Game {
    /// <summary>Represents game object with char[][] model for further on-map placing.</summary>
    public struct Ship {
        /// <summary>Represents after rotation object direction.</summary>
        public enum Rotation {
            Up = 0,
            Right,
            Down,
            Left
        }
        private static readonly int _rotationCount = Enum.GetNames(typeof(Rotation)).Length;

        /// <summary>Count of objects to place on map.</summary>
        public int Count { get; private set; }

        /// <summary>Model of object to place on map.</summary>
        public char[][] Model { get; private set; }

        /// <summary>Actual model rotation.</summary>
        public Rotation Rot { get; private set; }


        /// <summary>Represents const char namespace for game design.</summary>
        public static class Style {
            public const char Nothing = ' ';
            public const char ShipBody = 'O';
            public const char ShipWreck = 'X';
            public const char DeadWater = '#';
            public const char Gold = '$';
            public const char Silver = '&';
        }

        /// <summary>Binds in-game design char witch console color.</summary>
        public static Color StyleToColor(char style) {
            switch (style) {
                case Style.ShipBody:
                    return Color.LightBlue;
                case Style.ShipWreck:
                    return Color.DarkRed;
                case Style.DeadWater:
                    return Color.Smoke;
                case Style.Gold:
                    return Color.Gold;
                case Style.Silver:
                    return Color.Silver;
                default:
                    return Color.TextDark;
            }
        }

        /// <summary>Collection of ships to place on map.</summary>
        public static readonly Ship[] Models =
        {
            new Ship(4, new char[][] { new char[] { Style.ShipBody } }),

            new Ship(3, new char[][] { new char[] { Style.ShipBody, Style.ShipBody } }),

            new Ship(2, new char[][] { new char[] { Style.ShipBody, Style.ShipBody },
                                       new char[] { Style.Nothing,  Style.ShipBody } }),

            new Ship(1, new char[][] { new char[] { Style.ShipBody, Style.ShipBody, Style.ShipBody},
                                       new char[] { Style.Nothing,  Style.ShipBody, Style.Nothing},
                                       new char[] { Style.Nothing,  Style.ShipBody, Style.Nothing} }),
        };


        /// <summary>Creates Ship object.</summary>
        /// <param name="count">Count of objects to place on map.</param>
        /// <param name="model">Model of object to place on map.</param>
        public Ship(int count, char[][] model) {
            Count = count;
            Model = model;
            Rot = Rotation.Up;
        }


        /// <summary>Rotates model.</summary>
        /// <param name="rot">Specifies global rotation.</param>
        public void Rotate(Rotation rot) {
            Rotation relativeRot = (Rotation)(((int)Rot + (int)rot) % _rotationCount);
            Rot = rot;

            List<List<char>> modelRotated = new List<List<char>> { };

            int rx, ry;
            for (int y = 0; y < Model.Length; y++) {
                for (int x = 0; x < Model[y].Length; x++) {
                    switch (relativeRot) {
                        case Rotation.Right:
                            ry = x;
                            rx = (Model.Length - 1) - y;
                            break;
                        case Rotation.Down:
                            ry = (Model.Length - 1) - y;
                            rx = (Model[y].Length - 1) - x;
                            break;
                        case Rotation.Left:
                            ry = (Model[y].Length - 1) - x;
                            rx = y;
                            break;
                        default:
                            ry = y;
                            rx = x;
                            break;
                    }

                    while (modelRotated.Count <= ry)
                        modelRotated.Add(new List<char> { });

                    while (modelRotated[ry].Count <= rx)
                        modelRotated[ry].Add(Style.Nothing);

                    modelRotated[ry][rx] = Model[y][x];
                }
            }


            Model = new char[modelRotated.Count][];
            for (int y = 0; y < modelRotated.Count; y++)
                Model[y] = modelRotated[y].ToArray();
        }

        /// <summary>Randomly rotates model.</summary>
        public void RotateRandom() => Rotate((Ship.Rotation)Shared.Env.Rnd.Next(Ship._rotationCount));
    }
}

/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

namespace Lode.Systems.Game {
    /// <summary>Represents class for map logic and visualization.</summary>
    public class Map {
        /// <summary>Logical map has ship bodies.</summary>
        public char[][] Logic { get; set; }
        /// <summary>Visual map has only wrecks and dead water.</summary>
        public char[][] Visual { get; set; }

        public int Height => Logic.Length;
        public int Width => Logic[0].Length;

        public int ShipCount { get; set; } = 0;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Map() {
            Generate();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>Initialize and fills map.</summary>
        private char[][] Initialize() {
            char[][] map = new char[Shared.Sett.MapSize.y][];
            for (int i = 0; i < map.Length; i++)
                map[i] = new char[Shared.Sett.MapSize.x];
            for (int i = 0; i < map.Length; i++)
                for (int j = 0; j < map[i].Length; j++)
                    map[i][j] = Ship.Style.Nothing;
            return map;
        }

        /// <summary>Generates logic and visual maps.</summary>
        public void Generate() {
            Logic = Initialize();
            Visual = Initialize();

            ShipCount = 0;
            foreach (Ship ship in Ship.Models) {
                for (int i = 0; i < ship.Count; i++) {
                    ShipCount++;

                    ship.RotateRandom();

                    (int x, int y) pos = (0, 0);
                    do {
                        pos = (Shared.Env.Rnd.Next(Shared.Sett.MapSize.x), Shared.Env.Rnd.Next(Shared.Sett.MapSize.y));
                    } while (!TestCollision(Logic, pos, ship.Model));

                    for (int y = 0; y < ship.Model.Length; y++)
                        for (int x = 0; x < ship.Model[y].Length; x++) {
                            if (ship.Model[y][x] != Ship.Style.Nothing)
                                Logic[pos.y + y][pos.x + x] = ship.Model[y][x];
                        }
                }
            }
        }

        /// <summary>Test collision of model on exact position on exact map.</summary>
        /// <param name="shipMap">Ship map where to test.</param>
        /// <param name="pos">Pos where to test.</param>
        /// <param name="model">Model to test with.</param>
        /// <returns>True if not collision detected.</returns>
        public static bool TestCollision(char[][] shipMap, (int x, int y) pos, char[][] model) {
            for (int y = 0; y < model.Length; y++)
                for (int x = 0; x < model[y].Length; x++)
                    if (model[y][x] != Ship.Style.Nothing)
                        for (int i = y - 1; i <= y + 1; i++)
                            for (int j = x - 1; j <= x + 1; j++) {
                                int ax = pos.x + j;
                                int ay = pos.y + i;
                                if (
                                    (ay >= 0 && ay < shipMap.Length && ax >= 0 && ax < shipMap[ay].Length && shipMap[ay][ax] == Ship.Style.ShipBody)
                                    || pos.y + y >= shipMap.Length || pos.x + x >= shipMap[pos.y + y].Length
                                    )
                                    return false;
                            }

            return true;
        }

        /// <summary>Return <c>True</c> if at specified position is ship body or water.</summary>
        public bool CanAttack((int x, int y) at) {
            return Logic[at.y][at.x] == Ship.Style.ShipBody
                || Logic[at.y][at.x] == Ship.Style.Nothing;
        }

        /// <summary>Performs attack on water cell.</summary>
        /// <param name="pos">Position where to attack.</param>
        /// <returns>Tupple where: <list type="- ">
        /// <item><c>attack</c> specifies if cell was attackable (was clear water or living ship body)</item>
        /// <item><c>hitted</c> specifies if attack was performed on living ship body</item>
        /// <item><c>whole</c> specifies If whole ship was destroyed by performed attack</item>
        /// </list>
        /// </returns>
        public (bool attack, bool hitted, bool whole) Attack((int x, int y) pos) {
            if (Logic[pos.y][pos.x] != Ship.Style.ShipBody) {
                if (Logic[pos.y][pos.x] != Ship.Style.Nothing)
                    return (false, false, false);

                Visual[pos.y][pos.x] = Logic[pos.y][pos.x] = Ship.Style.DeadWater;
                return (true, false, false);
            }

            Visual[pos.y][pos.x] = Logic[pos.y][pos.x] = Ship.Style.ShipWreck;

            bool whole = IsWholeShipDestroyed(pos);

            if (whole)
                ShipCount--;

            return (true, true, whole);
        }

        /// <summary>Figures out if was whole ship destroyed at specified position.</summary>
        /// <param name="at">Position where to start finding.</param>
        /// <returns><c>True</c> if whole ship is destroyed.</returns>
        private bool IsWholeShipDestroyed((int x, int y) at) {
            Stack<(int, int)> shipBody = new Stack<(int, int)>();
            shipBody.Push(at);

            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            while (shipBody.Count > 0) {
                (int x, int y) pos = shipBody.Pop();
                visited.Add(pos);
                for (int i = pos.y - 1; i <= pos.y + 1; i++)
                    for (int j = pos.x - 1; j <= pos.x + 1; j++)
                        if (i >= 0 && i < Logic.Length && j >= 0 && j < Logic[i].Length) {
                            if (Logic[i][j] == Ship.Style.ShipWreck && !visited.Contains((j, i)))
                                shipBody.Push((j, i));
                            else if (Logic[i][j] == Ship.Style.ShipBody)
                                return false;
                        }
            }

            SetDeadWater(visited);
            return true;
        }

        /// <summary>Sets dead water border for specified ship.</summary>
        /// <param name="ship">Ship body positions.</param>
        public void SetDeadWater(HashSet<(int, int)> ship) {
            foreach ((int x, int y) in ship)
                for (int i = y - 1; i <= y + 1; i++)
                    for (int j = x - 1; j <= x + 1; j++)
                        if (i >= 0 && i < Logic.Length && j >= 0 && j < Logic[i].Length && Logic[i][j] != Ship.Style.ShipWreck)
                            Visual[i][j] = Logic[i][j] = Ship.Style.DeadWater;
        }

        private static readonly (int x, int y)[] _dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
        /// <summary>Finds not dead ship parts and water +-1 specified location.</summary>
        /// <param name="at">Position where to start finding.</param>
        /// <returns>Collection of not dead parts and water at +-1 specified location.</returns>
        public List<(int, int)> GetNearCanAttack((int x, int y) at) {
            List<(int, int)> result = new();
            for (int i = 0; i < _dirs.Length; i++) {
                (int x, int y) pos = (at.x + _dirs[i].x, at.y + _dirs[i].y);
                if (pos.y >= 0 && pos.y < Logic.Length && pos.x >= 0 && pos.x < Logic[pos.y].Length) {
                    if (Logic[pos.y][pos.x] == Ship.Style.ShipBody
                        || Logic[pos.y][pos.x] == Ship.Style.Nothing)
                        result.Add((pos.x, pos.y));
                }
            }
            return result;
        }

        /// <summary>Fills map with passed character.</summary>
        /// <param name="fill">Character for filling.</param>
        public void Fill(char fill) {
            for (int i = 0; i < Visual.Length; i++)
                for (int j = 0; j < Visual[i].Length; j++)
                    Logic[i][j] = Visual[i][j] = fill;
        }

        /// <summary>Cosmetically kills this map.</summary>
        public void Kill() {
            ShipCount = 0;
            Fill(Ship.Style.ShipWreck);
        }
    }
}

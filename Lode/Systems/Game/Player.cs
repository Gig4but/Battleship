/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

namespace Lode.Systems.Game {
    /// <summary>Represents Player base class.</summary>
    class Player {
        /// <summary>On map cursor position.</summary>
        public (int x, int y) Cursor = (0, 0);
        /// <summary>The UID of the player.</summary>
        public int Uid { get; private set; }
        /// <summary>Flag representing if player is ready.</summary>
        public bool Ready = false;


        /// <summary>Default constructor.</summary>
        /// <param name="uid">UID of the player.</param>
        public Player(int uid) {
            Uid = uid;
        }


        /// <summary>
        /// Attack on specified map with current cursor.
        /// For more info seealso <seealso cref="Map.Attack"/>
        /// </summary>
        public virtual (bool attack, bool hitted, bool whole) Attack(Map map) {
            return map.Attack(Cursor);
        }

    }
}

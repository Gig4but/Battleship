/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

namespace Lode.Systems.Game {
    /// <summary>Represents AI on top of <c>Player</c> class.</summary>
    class AI : Player {
        /// <summary>PerTick counter for AI cursor animation.</summary>
        private int cursorDelay = 0;

        /// <summary>Position of AI current attack.</summary>
        private (int x, int y) attackPosition;
        /// <summary>Position of AI current attack.</summary>
        private Stack<(int x, int y)> lastGood = new();

        /// <summary>Delegate called when AI cursor is on target.</summary>
        private Action RequestAttack;


        /// <summary>AI player constructor.</summary>
        /// <param name="requestAttack">Delegate called when AI has its visual cursor on generated attack position.</param>
        /// <param name="uid">UID of the player.</param>
        public AI(Action requestAttack, int uid) : base(uid) {
            RequestAttack = requestAttack;
            attackPosition = (Shared.Env.Rnd.Next(Shared.Sett.MapSize.y), Shared.Env.Rnd.Next(Shared.Sett.MapSize.x));
        }


        /// <summary>Animates <c>Cursor</c> by slow moving to target position.</summary>
        /// <returns>Current position of cursor (same as <c>.Cursor</c>).</returns>
        public (int x, int y) MoveCursor() {
            if (cursorDelay < Shared.Sett.AISpeed)
                cursorDelay++;
            else {
                cursorDelay = 0;
                if (Cursor.x < attackPosition.x)
                    Cursor = (Cursor.x + 1, Cursor.y);
                else if (Cursor.x > attackPosition.x)
                    Cursor = (Cursor.x - 1, Cursor.y);
                else if (Cursor.y < attackPosition.y)
                    Cursor = (Cursor.x, Cursor.y + 1);
                else if (Cursor.y > attackPosition.y)
                    Cursor = (Cursor.x, Cursor.y - 1);
                else
                    RequestAttack();
            }

            return Cursor;
        }


        /// <summary>
        /// State machine for new attack positions deciding.
        /// For more info seealso <seealso cref="Map.Attack"/>.
        /// </summary>
        public override (bool attack, bool hitted, bool whole) Attack(Map map) {
            var result = base.Attack(map);

            if (result.hitted && !result.whole) {
                lastGood.Push(attackPosition);
                AttackContinue(map);
            } else if (lastGood.Count != 0 && !result.whole) {
                AttackContinue(map);
            } else {
                lastGood.Clear();
                AttackNew(map);
            }

            return result;
        }

        /// <summary>Generates new random AI position for attack.</summary>
        /// <param name="map">Map where to generate.</param>
        private void AttackNew(Map map) {
            do
                attackPosition = (Shared.Env.Rnd.Next(Shared.Sett.MapSize.y), Shared.Env.Rnd.Next(Shared.Sett.MapSize.x));
            while (!map.CanAttack(attackPosition));
        }

        /// <summary>Generates new AI attack position near old successful one.</summary>
        /// /// <param name="map">Map where to generate.</param>
        private void AttackContinue(Map map) {
            var cells = map.GetNearCanAttack(lastGood.Peek());
            while (cells.Count == 0) {
                lastGood.Pop();
                cells = map.GetNearCanAttack(lastGood.Peek());
            }

            attackPosition = cells[Shared.Env.Rnd.Next(cells.Count)];
        }
    }
}

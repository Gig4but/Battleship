/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

namespace Lode.Systems.Game {
    /// <summary>Represents class for handling Players and Maps with specific logic.</summary>
    class GameManager {
        /// <summary>Collection of <c>(Player, Map)</c> associated with UID.</summary>
        private Dictionary<int, (Player player, Map map)> players = new();
        /// <summary>Order of UIDs.</summary>
        private List<int> order = new();

        /// <summary>Delegate for AI attacks.</summary>
        private Action RequestAttack;

        /// <summary>Current count of players manager handles.</summary>
        public int Count => players.Count;
        /// <summary>Flag decides if to increment player ID on <c>NextPlayer()</c> call.</summary>
        public bool SwitchPlayerAlso = true;

        /// <summary>The id of player which now plays.</summary>
        public int CurrentPlayerOrder { get; private set; } = 0;
        /// <summary>The uid of player which now plays. Returns -1 if current id hasn't have uid.</summary>
        public int CurrentPlayerUid {
            get {
                if (CurrentPlayerOrder >= order.Count)
                    return -1;
                return order[CurrentPlayerOrder];
            }
        }
        /// <summary>The id of map to attack.</summary>
        public int CurrentMapOrder { get; private set; } = 0;
        /// <summary>The uid of player which now plays. Returns -1 if current id hasn't have uid.</summary>
        public int CurrentMapUid {
            get {
                if (CurrentMapOrder >= order.Count)
                    return -1;
                return order[CurrentMapOrder];
            }
        }


        /// <summary>Default constructor.</summary>
        /// <param name="requestAttack">Delegate for AI attacking.</param>
        public GameManager(Action requestAttack) {
            RequestAttack = requestAttack;
        }


        /// <summary>Adds new player to managing.</summary>
        /// <param name="uid">The UID of the player.</param>
        /// <param name="ai">Flag to decides if player is an AI.</param>
        /// <param name="ready">Flag if player hast to be ready by default.</param>
        public void CreatePlayer(int uid = -1, bool ai = false, bool ready = false) {
            if (uid == -1) {
                uid = Count;
            }

            if (!players.ContainsKey(uid)) {
                Player player = (ai ? new AI(RequestAttack, uid) : new Player(uid));
                player.Ready = ready;
                players.Add(uid, (player, new Map()));
                order.Add(uid);
            }
        }

        /// <summary>Returns UID with associated ID.</summary>
        /// <param name="i">The ID of the player.</param>
        /// <returns>If not exist, returns -1.</returns>
        public int GetPlayerUid(int i) {
            if (order.Count < i || i < 0)
                return -1;
            return order[i];
        }

        /// <summary>Iterates to next map or player.</summary>
        public void NextPlayer() {
            do {
                CurrentMapOrder++;
                if (CurrentMapOrder >= Count) {
                    CurrentMapOrder = 0;
                    if (SwitchPlayerAlso)
                        CurrentPlayerOrder++;
                    if (CurrentPlayerOrder >= Count) {
                        CurrentPlayerOrder = 0;
                        CurrentMapOrder = 1;
                    }
                }
            } while (CurrentMapOrder == CurrentPlayerOrder || GetMapByOrder(CurrentMapOrder)?.ShipCount == 0);
        }

        /// <summary>Resets current map and player IDs.</summary>
        public void ResetCurrentOrder() {
            CurrentPlayerOrder = 0;
            CurrentMapOrder = 0;
            NextPlayer();
        }

        /// <summary>Returns all UIDs handled by manager.</summary>
        public int[] GetAllUid() => players.Keys.ToArray();

        public void RemovePlayerByUid(int uid) {
            if (players.ContainsKey(uid)) {
                players.Remove(uid);
                order.Remove(uid);
            }
        }
        public void RemovePlayerByOrder(int i) => RemovePlayerByUid(GetPlayerUid(i));

        public Player? GetCurrentPlayer() => GetPlayerByOrder(CurrentPlayerOrder);
        public Map? GetCurrentPlayerMap() => GetMapByOrder(CurrentPlayerOrder);
        public Map? GetCurrentMap() => GetMapByOrder(CurrentMapOrder);

        public Player? GetPlayerByUid(int uid) {
            if (!players.ContainsKey(uid))
                return null;
            return players[uid].player;
        }
        public Player? GetPlayerByOrder(int i) => GetPlayerByUid(GetPlayerUid(i));

        public Map? GetMapByUid(int uid) {
            if (!players.ContainsKey(uid))
                return null;
            return players[uid].map;
        }
        public Map? GetMapByOrder(int i) => GetMapByUid(GetPlayerUid(i));
    }
}

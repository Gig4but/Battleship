/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using Lode.Systems.Game;
using System.Runtime.CompilerServices;
using static Lode.Graphics;

namespace Lode {
    /// <summary>
    /// Largest Oleg's Deepest Environment.
    /// Class Lode represents complex sub-program - game.
    /// </summary>
    class Lode : IDisposable {
        /// <summary>Represents game modes.</summary>
        public enum Gamemode {
            Single = 0,
            Computer,
            Multiplayer,
            Server,
            Client,
        }
        private static readonly int _gamemodeCount = Enum.GetNames(typeof(Gamemode)).Length;

        /// <summary>Represents game states.</summary>
        private enum State {
            Settings = 0,
            Play,
            Creation,
            Win,
            Lobby,
            Connection,
        }

        /// <summary>Flag determines when to show player cursor.</summary>
        public bool ShowCursor = true;


        /// <summary>State of actual game.</summary>
        private State state = State.Settings;

        /// <summary>Mode of actual game.</summary>
        private Gamemode mode = Gamemode.Single;

        /// <summary>The "window" title.</summary>
        private string title = "LODE";

        /// <summary>Counter of attacks in singleplayer.</summary>
        private int attackCount = 0;
        /// <summary>Settings cursor position.</summary>
        private int settingsCursor = 0;
        /// <summary>Settings players counter.</summary>
        private int playersCount = 2;

        private GameManager gameManager;

        private NetManager? netManager;
        private string inviteKey = "";
        private string address = "";
        private string error = "";


        public Lode() {
            gameManager = new(Attack);
        }


        /// <summary>Virtual into-buffer draw method.</summary>
        /// <param name="buffer">Reference to buffer to write.</param>
        public void Draw(List<List<List<Sixel>>> buffer) {
            int layer = buffer.Count;

            DrawBackground(buffer, ref layer);

            // Print game or settings
            switch (state) {
                case State.Settings:
                    DrawSettings(buffer, ref layer);
                    break;
                case State.Lobby:
                    DrawLobby(buffer, ref layer);
                    break;
                case State.Connection:
                    DrawConnect(buffer, ref layer);
                    break;
                default:
                    DrawMaps(buffer, ref layer);
                    switch (state) {
                        case State.Creation:
                            DrawCreationHint(buffer, ref layer);
                            break;
                        case State.Win:
                            DrawEndHint(buffer, ref layer);
                            break;
                        default:
                            DrawCursor(buffer, ref layer);
                            break;
                    }
                    break;
            }

            DrawError(buffer, ref layer);
        }

        void DrawBackground(List<List<List<Sixel>>> buffer, ref int layer) {
            buffer.Add(new List<List<Sixel>> { new List<Sixel> { new($"{new string('#', 10)}>", Color.Blue), new(title), new($">{new string('#', Graphics.Console.Width - 12 - title.Length)}", Color.Blue) }, new List<Sixel> { } });
            buffer[layer].Capacity = Graphics.Console.Height;
        }

        void DrawError(List<List<List<Sixel>>> buffer, ref int layer) {
            if (error.Length > 0)
                buffer[layer].Add(new List<Sixel> { new($"Error: {error}", Color.LightRed) });
        }

        void DrawSettings(List<List<List<Sixel>>> buffer, ref int layer) {
            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { new(1, Color.Transparent), new($"[v^] Mode:         <{mode}>", (settingsCursor == 0 ? Color.TextLight : Color.TextDark)), new(6) },
                new List<Sixel> { },
                new List<Sixel> { new(1, Color.Transparent), new($"[v^] Player count: {(playersCount == 2 ? "|" : "<")}{playersCount}{(playersCount == Shared.Sett.MaxPlayerCount ? "|" : ">")}", (mode == Gamemode.Multiplayer ? (settingsCursor == 1 ? Color.TextLight : Color.TextDark) : Color.DarkGray)), new(6) },
                new List<Sixel> { },
                new List<Sixel> { new(1, Color.Transparent), new($"[v^] Map width:    <{Shared.Sett.MapSize.x}>", (mode == Gamemode.Client ? Color.DarkGray : (settingsCursor == 2 ? Color.TextLight : Color.TextDark))), new(6) },
                new List<Sixel> { },
                new List<Sixel> { new(1, Color.Transparent), new($"[v^] Map height:   <{Shared.Sett.MapSize.y}>", (mode == Gamemode.Client ? Color.DarkGray : (settingsCursor == 3 ? Color.TextLight : Color.TextDark))), new(6) },
                new List<Sixel> { },
                new List<Sixel> { new(1, Color.Transparent), new("[ENTER] to confirm") }
            });
        }

        void DrawMaps(List<List<List<Sixel>>> buffer, ref int layer) {
            int rowOffset = buffer[layer].Count;
            for (int i = 0; i < Shared.Sett.MapSize.y + 4; i++)
                buffer[layer].Add(new List<Sixel>(playersCount * (Shared.Sett.MapSize.x + 6)));

            for (int p = 0; p < gameManager.Count; p++) {
                Map? map = gameManager.GetMapByOrder(p);
                Player? player = gameManager.GetPlayerByOrder(p);
                if (map == null || player == null)
                    continue;


                _DrawMaps_MapTitle(buffer[layer][rowOffset], p, player, map);

                buffer[layer][rowOffset + 1].AddRange(new List<Sixel> { new(1, Color.Transparent), new(Shared.Sett.MapSize.x + 2, Color.Blue, '#') });

                char[][] mapToPrint;
                if ((state == State.Creation && p == gameManager.Count - 1)
                    || (mode == Gamemode.Computer && player is not AI)
                    || ((mode == Gamemode.Client || mode == Gamemode.Server) && gameManager.GetPlayerUid(p) == netManager?.Uid)) {
                    mapToPrint = map.Logic;
                } else {
                    mapToPrint = map.Visual;
                }

                for (int i = 0; i < map.Height; i++) {
                    int row = rowOffset + 2 + i;
                    buffer[layer][row].AddRange(new List<Sixel> { new(1, Color.Transparent), new("#", Color.Blue) });
                    for (int j = 0; j < map.Width; j++)
                        buffer[layer][row].Add(new(mapToPrint[i][j].ToString(), Ship.StyleToColor(mapToPrint[i][j])));
                    buffer[layer][row].Add(new("#", Color.Blue));
                }
                buffer[layer][rowOffset + 2 + Shared.Sett.MapSize.y].AddRange(new List<Sixel> { new(1, Color.Transparent), new(Shared.Sett.MapSize.x + 2, Color.Blue, '#') });
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _DrawMaps_MapTitle(List<Sixel> row, int p, Player player, Map map) {
            row.Add(new(1, Color.Transparent));
            switch (mode) {
                case Gamemode.Client:
                case Gamemode.Server:
                case Gamemode.Multiplayer:
                    Color color = Color.TextDark;
                    if (p == gameManager.CurrentPlayerOrder)
                        color = Color.TextLight;
                    else if (state == State.Lobby && player.Ready) {
                        color = Color.Green;
                    }

                    row.Add(new($"Player: {p + 1}", color));
                    break;
                case Gamemode.Computer:
                    row.Add(new(p == 0 ? "Computer" : "Player"));
                    break;
                case Gamemode.Single:
                    row.Add(new($"Attacks: {attackCount} - Ships left: {map.ShipCount}  "));
                    break;
            }
            Sixel mapTitle = row[row.Count - 1];
            int len = Shared.Sett.MapSize.x + 2 - mapTitle.Length;
            if (len > 0) {
                row[row.Count - 1] = new(mapTitle.Content + new string(' ', len), mapTitle.Color);
            }
        }

        void DrawCreationHint(List<List<List<Sixel>>> buffer, ref int layer) {
            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { },
                new List<Sixel> { new("[SPACEBAR] Confirm | [ANY ARROW] Generate new", Color.Blue) }
            });
        }

        void DrawEndHint(List<List<List<Sixel>>> buffer, ref int layer) {
            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { },
                new List<Sixel> { new("Game ended", Color.Blue), new("?", Color.Gold), new(64) }
            });
        }

        void DrawCursor(List<List<List<Sixel>>> buffer, ref int layer) {
            Player? player = gameManager.GetCurrentPlayer();
            int mapOrder = gameManager.CurrentMapOrder;

            if (player == null)
                return;

            if (player is AI)
                ((AI)player).MoveCursor();
            else {
                buffer[layer].AddRange(new List<List<Sixel>> {
                    new List<Sixel> { },
                    new List<Sixel> { new("[SPACEBAR] Attack | [ANY ARROW] Select coordinates", Color.Blue) }
                });
            }

            int ay = 4 + player.Cursor.y;
            int ax = 2 + player.Cursor.x + ((2 + Shared.Sett.MapSize.x) * mapOrder) + (1 * mapOrder);

            buffer[layer][ay][ax] = new(buffer[layer][ay][ax].Content, Color.TextLight);
        }

        void DrawLobby(List<List<List<Sixel>>> buffer, ref int layer) {
            DrawMaps(buffer, ref layer);
            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { new($"[SPACEBAR] {(mode == Gamemode.Server ? "Start game" : "Ready")} | [ANY ARROW] Generate new map", Color.Blue) },
                new List<Sixel> { new($"Invite key: [{inviteKey}]", Color.Gold) }
            });
        }

        (bool firstRow, bool turn) _connectCursor = (true, false);
        int _connectCursorTurnDelay = 0;
        bool _connectConnecting = false;
        int _connectLoading = 0;
        void DrawConnect(List<List<List<Sixel>>> buffer, ref int layer) {
            _connectCursorTurnDelay++;
            if (_connectCursorTurnDelay > Shared.Env.FpsLimit / 3) {
                _connectCursorTurnDelay = 0;
                _connectCursor = (_connectCursor.firstRow, !_connectCursor.turn);

                _connectLoading++;
                if (_connectLoading > 3)
                    _connectLoading = 0;
            }

            buffer[layer].AddRange(new List<List<Sixel>> {
                new List<Sixel> { },
                new List<Sixel> { new("Address:", Color.Sky), new(1), new(address), new((_connectCursor.firstRow ? 1 : 0), (_connectCursor.turn ? Color.TextLight : Color.TextDark)), new(20 - address.Length) },
                new List<Sixel> { },
                new List<Sixel> { new("Invite key:", Color.Sky), new(1), new(inviteKey), new((!_connectConnecting && !_connectCursor.firstRow ? 1 : 0), (_connectCursor.turn ? Color.TextLight : Color.TextDark)), new(10) },
                new List<Sixel> { },
                new List<Sixel> { new((_connectConnecting ? $"Connecting{new string('.', _connectLoading)}" : new string(' ', 20))) },
                new List<Sixel> { },
                new List<Sixel> { new((_connectCursor.firstRow ? "[ENTER] to confirm address" : $"Type {Shared.Env.InviteKeyLength}-digit invite key")), new(10) }
            });
        }

        // TODO voting for kick disconnected player
        int _mapsReceived = 1;
        void UpdateMap(Net.Packet<NetManager.EventData> packet) {
            if (netManager == null)
                return;

            switch (packet.Data.Event) {
                case NetManager.Event.Connected:
                    state = State.Lobby;
                    error = "";
                    break;

                case NetManager.Event.NewPlayer: {
                    gameManager.CreatePlayer(packet.Uid);
                    if (mode == Gamemode.Server && packet.Uid != 0) {
                        int[] allUid = gameManager.GetAllUid();
                        int[] extra = new int[allUid.Length * 2];
                        for (int i = 0; i < allUid.Length; i++) {
                            extra[i * 2] = allUid[i];
                            Player? player = gameManager.GetPlayerByUid(allUid[i]);
                            if (player != null)
                                extra[i * 2 + 1] = player.Ready ? 1 : 0;
                        }

                        Map? map = gameManager.GetMapByOrder(0);
                        netManager.SendAsServer(packet.Uid, 0, new NetManager.EventData {
                            Event = NetManager.Event.Settings,
                            Map = (map != null ? map.Visual : new Map().Visual),
                            Extra = extra
                        });
                    }
                }
                break;

                case NetManager.Event.Settings:
                    if (mode == Gamemode.Server || packet.Uid != 0)
                        break;

                    Shared.Sett.MapSize = (packet.Data.Map[0].Length, packet.Data.Map.Length);
                    for (int i = 0; i < packet.Data.Extra.Length; i += 2) {
                        gameManager.CreatePlayer(packet.Data.Extra[i], ready: (packet.Data.Extra[i + 1] == 1 ? true : false));
                    }
                    Graphics.Console.ClearNextDraw();
                    break;

                case NetManager.Event.LostPlayer:
                    if (state == State.Lobby) {
                        for (int i = 0; i < packet.Data.Extra.Length; i++) {
                            gameManager.RemovePlayerByUid(packet.Data.Extra[i]);
                        }
                    } else if (state == State.Play) {
                        Map? map = gameManager.GetMapByUid(packet.Uid);
                        if (map == null)
                            break;

                        if (map.ShipCount == 0)
                            break;

                        map.Kill();

                        if (PlayerEnded(map)) {
                            for (int i = 0; i < gameManager.Count; i++) {
                                Map? m = gameManager.GetMapByOrder(i);
                                if (m == null)
                                    continue;
                                ServerSendMapUpdate(m, gameManager.GetPlayerUid(i), false);
                            }
                            state = State.Win;
                            netManager.SendAsServer(-1, 0, new NetManager.EventData { Event = NetManager.Event.End });
                        } else {
                            bool next = gameManager.CurrentMapUid == packet.Uid;
                            if (next)
                                gameManager.NextPlayer();
                            ServerSendMapUpdate(map, packet.Uid, next);
                        }
                    }
                    Graphics.Console.ClearNextDraw();
                    break;

                case NetManager.Event.Ready: {
                    if (mode == Gamemode.Client && packet.Uid == 0) {
                        Map? map = gameManager.GetMapByUid(netManager.Uid);
                        if (map == null)
                            break;

                        netManager.SendAsClient(new NetManager.EventData { ForServerOnly = true, Event = NetManager.Event.MapUpdate, Map = map.Logic });
                        state = State.Play;
                        ResetConnectionState();
                        gameManager.NextPlayer();
                        Graphics.Console.ClearNextDraw();
                    } else if (packet.Uid != 0) {
                        Player? player = gameManager.GetPlayerByUid(packet.Uid);
                        if (player == null)
                            break;

                        player.Ready = !player.Ready;
                    }
                }
                break;

                case NetManager.Event.MapUpdate: {
                    Map? map = gameManager.GetMapByUid(packet.Uid);
                    if (map == null)
                        break;

                    if (mode == Gamemode.Server && state == State.Lobby) {
                        map.Logic = packet.Data.Map;
                        int mapsReceived = Interlocked.Increment(ref _mapsReceived);

                        if (mapsReceived == gameManager.Count) {
                            state = State.Play;
                            ResetConnectionState();
                            gameManager.NextPlayer();
                            Graphics.Console.ClearNextDraw();
                        }
                    } else if (mode == Gamemode.Client) {
                        if (packet.Uid == netManager.Uid)
                            map.Logic = packet.Data.Map;
                        else
                            map.Visual = packet.Data.Map;

                        if (packet.Data.Extra != null && packet.Data.Extra.Length > 0 && packet.Data.Extra[0] == 1) {
                            gameManager.NextPlayer();
                            if (packet.Data.Extra.Length > 1) {
                                map.ShipCount = packet.Data.Extra[1];
                            }
                        }
                    }
                }
                break;

                case NetManager.Event.Cursor: {
                    Player? player = gameManager.GetPlayerByUid(packet.Uid);
                    if (player == null)
                        break;
                    if (packet.Data.Extra.Length < 2)
                        break;

                    player.Cursor = (packet.Data.Extra[1], packet.Data.Extra[0]);
                }
                break;

                case NetManager.Event.Attack:
                    if (gameManager.CurrentPlayerUid == packet.Uid)
                        Attack();
                    break;

                case NetManager.Event.End: {
                    if (mode == Gamemode.Server)
                        break;

                    Map? map = gameManager.GetMapByUid(netManager.Uid);

                    if (map != null && map.ShipCount > 0)
                        map.Fill(Ship.Style.Gold);
                    state = State.Win;
                }
                break;
            }
        }

        private void ResetConnectionState(string error = "") {
            this.error = error;
            address = "";
            inviteKey = "";
            _connectConnecting = false;
            _connectCursor.firstRow = true;
        }

        void OnHalt(Net.Error error) {
            Graphics.Console.ClearNextDraw();
            switch (error) {
                case Net.Error.Disconnected:
                    if (state == State.Connection) {
                        ResetConnectionState("Cannot connect");
                        break;
                    }
                    if (state == State.Play) {
                        ResetConnectionState("Connection lost");
                        break;
                    }
                    if (state != State.Win && mode != Gamemode.Server)
                        state = State.Connection;
                    break;
                case Net.Error.ConnectionNotAllowed:
                case Net.Error.NetworkError:
                case Net.Error.WrongAdress:
                case Net.Error.UnsupportedConnection:
                    if (state == State.Connection) {
                        this.error = "Wrong address";
                        address = "";
                        inviteKey = "";
                        _connectConnecting = false;
                        _connectCursor.firstRow = true;
                        break;
                    }
                    this.error = error.ToString();
                    break;
                case Net.Error.InsufficientSendData:
                    if (state == State.Connection) {
                        this.error = "Wrong invite key";
                        inviteKey = "";
                        _connectConnecting = false;
                        break;
                    }
                    throw new Exception(error.ToString());
                default:
                    throw new Exception(error.ToString());
            }
        }

        void CreateServer() {
            Graphics.Console.ClearNextDraw();
            state = State.Lobby;
            mode = Gamemode.Server;
            inviteKey = Shared.Func.RandomCharVariation(new char[] { 'Q', 'W', 'E', 'R' }, Shared.Env.InviteKeyLength);
            netManager = new NetManager(true, UpdateMap, OnHalt);
            netManager.Start(inviteKey);
        }

        void CreateClient() {
            Graphics.Console.ClearNextDraw();
            state = State.Connection;
            mode = Gamemode.Client;
            netManager = new NetManager(false, UpdateMap, OnHalt);
        }


        #region note
        // can't use readonly char in switch because of "not constant value"
        // static char can't be constant, only readonly
        //Color Ship.StyleColor(char c)
        //{
        //    switch(c)
        //    {
        //        case Ship.Style.shipBody:
        //            return Color.Blue;
        //        default:
        //            return Color.TextDark;
        //    }
        //}
        #endregion

        /// <summary>Change state from settings.</summary>
        public void ConfirmSettings() {
            switch (state) {
                case State.Settings:
                    switch (mode) {
                        case Gamemode.Server:
                            CreateServer();
                            break;
                        case Gamemode.Client:
                            CreateClient();
                            break;
                        default:
                            Graphics.Console.ClearNextDraw();
                            state = State.Creation;
                            switch (mode) {
                                case Gamemode.Single:
                                    gameManager.CreatePlayer(0);
                                    state = State.Play;
                                    break;
                                case Gamemode.Computer:
                                    gameManager.CreatePlayer(0, true);
                                    gameManager.CreatePlayer(1);
                                    gameManager.NextPlayer();
                                    gameManager.NextPlayer();
                                    break;
                                default:
                                    gameManager.CreatePlayer(0);
                                    break;
                            }
                            break;
                    }
                    break;

                case State.Connection:
                    if (_connectCursor.firstRow)
                        _connectCursor.firstRow = false;
                    break;
            }
        }

        /// <summary>Moves cursor up.</summary>
        public void CursorUp() {
            switch (state) {
                case State.Play: {
                    Player? player = gameManager.GetCurrentPlayer();
                    if (player == null)
                        break;

                    if ((mode == Gamemode.Server || mode == Gamemode.Client)
                        && gameManager.GetPlayerUid(gameManager.CurrentPlayerOrder) != netManager?.Uid)
                        break;

                    if (mode == Gamemode.Computer && player is AI)
                        break;

                    if (player.Cursor.y > 0)
                        player.Cursor = (player.Cursor.x, player.Cursor.y - 1);
                    else
                        player.Cursor = (player.Cursor.x, Shared.Sett.MapSize.y - 1);

                    netManager?.SendAsClient(new NetManager.EventData { Event = NetManager.Event.Cursor, Extra = new int[] { player.Cursor.y, player.Cursor.x } });
                }
                break;

                case State.Creation: {
                    Map? map = gameManager.GetMapByOrder(gameManager.Count - 1);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Lobby: {
                    if (netManager == null)
                        break;
                    Map? map = gameManager.GetMapByUid(netManager.Uid);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Settings:
                    if (mode == Gamemode.Client) {
                        settingsCursor = 0;
                        break;
                    }

                    if (settingsCursor > 0)
                        settingsCursor--;
                    else
                        settingsCursor = 3;

                    if (settingsCursor == 1 && (mode != Gamemode.Multiplayer))
                        settingsCursor--;
                    break;
            }

        }

        /// <summary>Moves cursor down.</summary>
        public void CursorDown() {
            switch (state) {
                case State.Play: {
                    Player? player = gameManager.GetCurrentPlayer();
                    if (player == null)
                        break;

                    if ((mode == Gamemode.Server || mode == Gamemode.Client)
                        && gameManager.GetPlayerUid(gameManager.CurrentPlayerOrder) != netManager?.Uid)
                        break;

                    if (mode == Gamemode.Computer && player is AI)
                        break;

                    if (player.Cursor.y < Shared.Sett.MapSize.y - 1)
                        player.Cursor = (player.Cursor.x, player.Cursor.y + 1);
                    else
                        player.Cursor = (player.Cursor.x, 0);

                    netManager?.SendAsClient(new NetManager.EventData { Event = NetManager.Event.Cursor, Extra = new int[] { player.Cursor.y, player.Cursor.x } });
                }
                break;

                case State.Creation: {
                    Map? map = gameManager.GetMapByOrder(gameManager.Count - 1);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Lobby: {
                    if (netManager == null)
                        break;
                    Map? map = gameManager.GetMapByUid(netManager.Uid);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Settings:
                    if (mode == Gamemode.Client) {
                        settingsCursor = 0;
                        break;
                    }

                    if (settingsCursor < 3)
                        settingsCursor++;
                    else
                        settingsCursor = 0;

                    if (settingsCursor == 1 && (mode != Gamemode.Multiplayer))
                        settingsCursor++;
                    break;
            }
        }

        /// <summary>Moves cursor Left.</summary>
        public void CursorLeft() {
            switch (state) {
                case State.Play: {
                    Player? player = gameManager.GetCurrentPlayer();
                    if (player == null)
                        break;

                    if ((mode == Gamemode.Server || mode == Gamemode.Client)
                        && gameManager.GetPlayerUid(gameManager.CurrentPlayerOrder) != netManager?.Uid)
                        break;

                    if (mode == Gamemode.Computer && player is AI)
                        break;

                    if (player.Cursor.x > 0)
                        player.Cursor = (player.Cursor.x - 1, player.Cursor.y);
                    else
                        player.Cursor = (Shared.Sett.MapSize.x - 1, player.Cursor.y);

                    netManager?.SendAsClient(new NetManager.EventData { Event = NetManager.Event.Cursor, Extra = new int[] { player.Cursor.y, player.Cursor.x } });
                }
                break;

                case State.Creation: {
                    Map? map = gameManager.GetMapByOrder(gameManager.Count - 1);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Lobby: {
                    if (netManager == null)
                        break;
                    Map? map = gameManager.GetMapByUid(netManager.Uid);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Settings:
                    switch (settingsCursor) {
                        case 0:
                            if (mode > 0)
                                mode--;
                            else
                                mode = (Gamemode)(_gamemodeCount - 1);
                            break;
                        case 1:
                            if (playersCount > 2)
                                playersCount--;
                            break;
                        case 2:
                            if (Shared.Sett.MapSize.x > Shared.Sett.MapSizeRange.From.x)
                                Shared.Sett.MapSize.x--;
                            else
                                Shared.Sett.MapSize.x = Shared.Sett.MapSizeRange.To.x;
                            break;
                        case 3:
                            if (Shared.Sett.MapSize.y > Shared.Sett.MapSizeRange.From.y)
                                Shared.Sett.MapSize.y--;
                            else
                                Shared.Sett.MapSize.y = Shared.Sett.MapSizeRange.To.y;
                            break;
                    }
                    break;
            }
        }

        /// <summary>Moves cursor right.</summary>
        public void CursorRight() {
            switch (state) {
                case State.Play: {
                    Player? player = gameManager.GetCurrentPlayer();
                    if (player == null)
                        break;

                    if ((mode == Gamemode.Server || mode == Gamemode.Client)
                        && gameManager.GetPlayerUid(gameManager.CurrentPlayerOrder) != netManager?.Uid)
                        break;

                    if (mode == Gamemode.Computer && player is AI)
                        break;

                    if (player.Cursor.x < Shared.Sett.MapSize.x - 1)
                        player.Cursor = (player.Cursor.x + 1, player.Cursor.y);
                    else
                        player.Cursor = (0, player.Cursor.y);

                    netManager?.SendAsClient(new NetManager.EventData { Event = NetManager.Event.Cursor, Extra = new int[] { player.Cursor.y, player.Cursor.x } });
                }
                break;

                case State.Creation: {
                    Map? map = gameManager.GetMapByOrder(gameManager.Count - 1);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Lobby: {
                    if (netManager == null)
                        break;
                    Map? map = gameManager.GetMapByUid(netManager.Uid);
                    if (map == null)
                        break;

                    map.Generate();
                }
                break;

                case State.Settings:
                    switch (settingsCursor) {
                        case 0:
                            if ((int)mode < _gamemodeCount - 1)
                                mode++;
                            else
                                mode = 0;
                            break;
                        case 1:
                            if (playersCount < Shared.Sett.MaxPlayerCount)
                                playersCount++;
                            break;
                        case 2:
                            if (Shared.Sett.MapSize.x < Shared.Sett.MapSizeRange.To.x)
                                Shared.Sett.MapSize.x++;
                            else
                                Shared.Sett.MapSize.x = Shared.Sett.MapSizeRange.From.x;
                            break;
                        case 3:
                            if (Shared.Sett.MapSize.y < Shared.Sett.MapSizeRange.To.y)
                                Shared.Sett.MapSize.y++;
                            else
                                Shared.Sett.MapSize.y = Shared.Sett.MapSizeRange.From.y;
                            break;
                    }
                    break;
            }
        }

        /// <summary>Perform attack.</summary>
        public void Space() {
            switch (state) {
                case State.Play:
                    if (mode == Gamemode.Server && gameManager.CurrentPlayerOrder != 0) {
                        break;
                    }
                    if (mode == Gamemode.Client) {
                        netManager?.SendAsClient(new NetManager.EventData { ForServerOnly = true, Event = NetManager.Event.Attack });
                        break;
                    }
                    Attack();
                    break;

                case State.Creation: {
                    if (gameManager.Count < playersCount) {
                        gameManager.CreatePlayer(gameManager.Count);
                        gameManager.NextPlayer();
                    } else {
                        gameManager.ResetCurrentOrder();
                        Graphics.Console.ClearNextDraw();
                        state = State.Play;
                    }
                }
                break;

                case State.Lobby: {
                    if (mode == Gamemode.Client) {
                        netManager?.SendAsClient(new NetManager.EventData { Event = NetManager.Event.Ready });
                    } else if (mode == Gamemode.Server) {
                        if (gameManager.Count < 2)
                            break;

                        bool allPlayersReady = true;
                        for (int i = 1; i < gameManager.Count; i++) {
                            Player? player = gameManager.GetPlayerByOrder(i);
                            if (player == null)
                                continue;

                            if (!player.Ready) {
                                allPlayersReady = false;
                                break;
                            }
                        }

                        if (!allPlayersReady) {
                            error = "Not all players are ready!";
                            break;
                        }

                        netManager?.ServerStopListening();
                        netManager?.SendAsServer(-1, 0, new NetManager.EventData { Event = NetManager.Event.Ready });
                    }
                }
                break;
            }
        }

        private bool PlayerEnded(Map map) {
            int pos = 1;
            int lastNotDead = 0;
            for (int i = 0; i < gameManager.Count; i++) {
                Map? m = gameManager.GetMapByOrder(i);
                if (m != null && m.ShipCount > 0) {
                    pos++;
                    lastNotDead = i;
                }
            }

            if (pos > 2) {
                map.Fill(Ship.Style.ShipWreck);
            } else {
                if (mode != Gamemode.Single) {
                    map.Fill(Ship.Style.Silver);
                    gameManager.GetMapByOrder(lastNotDead)?.Fill(Ship.Style.Gold);
                }
                state = State.Win;
                return true;
            }

            return false;
        }

        private void ServerSendMapUpdate(Map map, int mapUid, bool next) {
            if (mode == Gamemode.Server) {
                if (netManager == null)
                    return;

                int[] extra = new int[] { (next ? 1 : 0), map.ShipCount };
                int[] allUid = gameManager.GetAllUid();
                for (int i = 0; i < allUid.Length; i++) {
                    if (allUid[i] != mapUid && allUid[i] != 0 && !netManager.IsDiconnected(allUid[i]))
                        netManager.SendAsServer(allUid[i], mapUid, new NetManager.EventData {
                            Event = NetManager.Event.MapUpdate,
                            Map = map.Visual,
                            Extra = extra
                        });
                }
                if (!netManager.IsDiconnected(mapUid))
                    netManager.SendAsServer(mapUid, mapUid, new NetManager.EventData { Event = NetManager.Event.MapUpdate, Map = map.Logic, Extra = extra });
            }
        }

        private void Attack() {
            Player? player = gameManager.GetCurrentPlayer();
            if (player == null)
                return;
            Map? map = gameManager.GetCurrentMap();
            if (map == null)
                return;

            var result = player.Attack(map);

            if (!result.attack)
                return;

            if (mode == Gamemode.Single)
                attackCount++;

            bool next = false;
            bool win = false;
            if (result.whole) {
                if (map.ShipCount == 0) {
                    next = true;
                    win = PlayerEnded(map);
                }
            }

            result.hitted = win || result.hitted;
            int playerMapUid = gameManager.GetPlayerUid(gameManager.CurrentMapOrder);
            if (!result.hitted && mode != Gamemode.Single) {
                next = true;
            }

            if (next)
                gameManager.NextPlayer();

            ServerSendMapUpdate(map, playerMapUid, next && !win);
            if (win)
                netManager?.SendAsServer(-1, 0, new NetManager.EventData { Event = NetManager.Event.End });
        }


        public void CharKey(char key) {
            if (state == State.Connection) {
                if (netManager == null)
                    return;

                if (_connectCursor.firstRow) {
                    switch (key) {
                        case '\0':
                            if (address.Length > 0)
                                address = address.Substring(0, address.Length - 1);
                            return;
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '0':
                        case '.':
                            if (address.Length < 15)
                                address += key;
                            return;
                    }
                } else {
                    switch (key) {
                        case 'Q':
                        case 'W':
                        case 'E':
                        case 'R':
                            if (inviteKey.Length < Shared.Env.InviteKeyLength)
                                inviteKey += key;
                            if (inviteKey.Length == Shared.Env.InviteKeyLength) {
                                _connectConnecting = true;
                                if (!netManager.Start(inviteKey, address: (address.Length == 0 ? "127.0.0.1" : address)))
                                    OnHalt(Net.Error.WrongAdress);
                            }
                            return;
                    }
                }
            }
        }

        public void Dispose() {
            if (netManager != null)
                netManager.Dispose();
        }
    }
}

/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static Lode.Net;

#pragma warning disable CS8602, CS8604 // null reference

namespace Lode.Systems.Game {
    /// <summary>
    /// Represents class for wraping networking logic for game. 
    /// /!\ DO NOT FORGET TO DISPOSE
    /// </summary>
    class NetManager : IDisposable {
        /// <summary>Determines type of info in packet.</summary>
        public enum Event {
            Connected,
            NewPlayer,
            Settings,
            LostPlayer,
            Ready,
            MapUpdate,
            Cursor,
            Attack,
            End
        }

        /// <summary>Represents packet to use in game network.</summary>
        public struct EventData {
            /// <summary>Flag to decide if packet is dedicated for server only.</summary>
            public bool ForServerOnly { get; set; }
            /// <summary>Type of info in packet.</summary>
            public Event @Event { get; set; }
            /// <summary>Possible map array.</summary>
            public char[][] Map { get; set; }
            /// <summary>Possible extra data.</summary>
            public int[] Extra { get; set; }
        }

        /// <summary>First key for handshake intiation.</summary>
        public int InviteKey { get; private set; }

        /// <summary>Collection of queues for server sender threads assigned with UID.</summary>
        private Dictionary<int, BlockingCollection<Packet<EventData>>>? serverUpdatesQueues;
        /// <summary>Queue for client sender thread.</summary>
        private BlockingCollection<EventData>? clientUpdatesQueue;
        /// <summary>Set of disonnected UIDs.</summary>
        private HashSet<int> disconnects = new();

        /// <summary>"Macro" for queue creation.</summary>
        private BlockingCollection<T> DefaultQueue<T>() => new BlockingCollection<T>(new ConcurrentQueue<T>());

        /// <summary>Server handler.</summary>
        private Server<EventData>? server;
        /// <summary>Client handler.</summary>
        private Client<EventData>? client;

        /// <summary>Client UID.</summary>
        public int Uid => client.Uid;
        /// <summary>Returns true if UID is disconnected.</summary>
        public bool IsDiconnected(int uid) => disconnects.Contains(uid);

        /// <summary>Delegate to pass received data.</summary>
        private Action<Packet<EventData>>? UpdateMap;
        /// <summary>Server and Client OnHalt delegate.</summary>
        private Action<Error>? OnHalt;
        /// <summary>Internal OnHalt called before public.</summary>
        private void _OnHalt(Error error) {
            if (server != null)
                CreateServer();
            CreateClient();
            OnHalt(error);
        }


        private void CreateServer() {
            serverUpdatesQueues = new();
            this.server = Server<EventData>.Create(ServerOnDataReceive, ServerGetDataToSend, ServerOnNewClient, ServerOnClientDisconnect, _OnHalt);
        }
        private void CreateClient() {
            clientUpdatesQueue = DefaultQueue<EventData>();
            this.client = Client<EventData>.Create(ClientOnDataReceive, ClientGetDataToSend, ClientOnConnect, ClientOnDisconnect, _OnHalt);
        }
        /// <summary>Creates server, client and queues.</summary>
        /// <param name="server">Decides if create server or only client.</param>
        /// <param name="updateMap">Delegate for passing received data.</param>
        /// <param name="onHalt">Delegate for halting.</param>
        public NetManager(bool server, Action<Packet<EventData>> updateMap, Action<Error> onHalt) {
            UpdateMap = updateMap;
            OnHalt = onHalt;
            if (server)
                CreateServer();
            CreateClient();
        }


        /// <summary>Starts server and client.</summary>
        /// <param name="inviteKey">Invite key for connecting.</param>
        /// <param name="address">Server address.</param>
        public bool Start(string inviteKey, string address = "127.0.0.1") {
            if (client == null)
                return false;

            Regex ipv4Rg = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            if (!ipv4Rg.IsMatch(address))
                return false;

            if (!IPAddress.TryParse(address, out IPAddress? addr))
                return false;

            byte[] key = Encoding.ASCII.GetBytes(inviteKey);

            if (server != null) {
                server.Start(key);
            }

            client.Start(addr.GetAddressBytes(), key);

            return true;
        }

        /// <summary>Sends data as server.</summary>
        /// <param name="receiver">UID of receiver. -1 is broadcast.</param>
        /// <param name="sender">UID of sender. 0 is server.</param>
        /// <param name="eventData">Data to send.</param>
        public void SendAsServer(int receiver, int sender, EventData eventData) {
            var packet = new Packet<EventData> { Uid = sender, Data = eventData };

            if (receiver == -1) {
                ServerOnDataReceive(packet);
            } else {
                lock (serverUpdatesQueues) {
                    serverUpdatesQueues[receiver].Add(packet);
                }
            }
        }
        /// <summary>Sends data as client.</summary>
        /// <param name="eventData">Data to send.</param>
        public void SendAsClient(EventData eventData) {
            clientUpdatesQueue.Add(eventData);
        }

        /// <summary>Handler for server received packet.</summary>
        /// <param name="packet">Received packet.</param>
        void ServerOnDataReceive(Packet<EventData> packet) {
            if (packet.Data.ForServerOnly) {
                ClientOnDataReceive(packet);
                return;
            }
            lock (serverUpdatesQueues) {
                lock (disconnects) {
                    foreach (int id in serverUpdatesQueues.Keys)
                        if (!disconnects.Contains(id))
                            serverUpdatesQueues[id].Add(packet);
                }
            }
        }
        /// <summary>Handler for client received packet.</summary>
        /// <param name="packet">Received packet.</param>
        void ClientOnDataReceive(Packet<EventData> packet) => UpdateMap(packet);

        /// <summary>Handler for server packet sending.</summary>
        /// <param name="uid">UID of connection.</param>
        Packet<EventData> ServerGetDataToSend(int uid) {
            BlockingCollection<Packet<EventData>> queue;
            lock (serverUpdatesQueues) {
                if (!serverUpdatesQueues.ContainsKey(uid))
                    return new Packet<EventData> { Uid = -1 };
                queue = serverUpdatesQueues[uid];
            }
            return queue.Take();
        }
        /// <summary>Handler for client packet sending.</summary>
        EventData ClientGetDataToSend() {
            return clientUpdatesQueue.Take();
        }

        /// <summary>Handler for server new client.</summary>
        /// <param name="uid">UID of new client.</param>
        void ServerOnNewClient(int uid) {
            lock (serverUpdatesQueues) {
                foreach (int id in serverUpdatesQueues.Keys) {
                    if (id != client.Uid)
                        serverUpdatesQueues[id].Add(new Packet<EventData> { Uid = uid, Data = new EventData() { Event = Event.NewPlayer } });
                }
                serverUpdatesQueues[uid] = DefaultQueue<Packet<EventData>>();
            }
            UpdateMap(new Packet<EventData> { Uid = uid, Data = new EventData { Event = Event.NewPlayer } });
        }
        /// <summary>Handler for client connection.</summary>
        /// <param name="uid">UID from server.</param>
        void ClientOnConnect(int uid) {
            if (server == null)
                UpdateMap(new Packet<EventData> { Uid = uid, Data = new EventData() { Event = Event.Connected } });
        }

        /// <summary>Handler for server on client disconnection.</summary>
        /// <param name="uid">UID of client.</param>
        void ServerOnClientDisconnect(int uid) {
            lock (disconnects) {
                disconnects.Add(uid);
            }
            lock (serverUpdatesQueues) {
                // It's not safe to remove queue
                // serverUpdatesQueues.Remove(uid);
                foreach (int id in serverUpdatesQueues.Keys) {
                    serverUpdatesQueues[id].Add(new Packet<EventData> { Uid = uid, Data = new EventData { Event = Event.LostPlayer, Extra = new int[] { uid } } });
                }
            }
        }
        /// <summary>Handler for client on disconnection.</summary>
        void ClientOnDisconnect() => OnHalt(Error.Disconnected);

        /// <summary>Stops server listening.</summary>
        public void ServerStopListening() => server?.StopListening();

        /// <summary>Disposes server, client and queues.</summary>
        public void Dispose() {
            if (server != null) {
                server.Dispose();
                serverUpdatesQueues.Clear();
            }
            client.Dispose();
            clientUpdatesQueue.Dispose();
        }
    }
}

#pragma warning restore CS8602, CS8604 // null reference

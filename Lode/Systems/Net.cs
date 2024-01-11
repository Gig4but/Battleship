/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lode {
    /// <summary>Provides network system.</summary>
    public static class Net {
        private static byte[] DefaultBuffer => new byte[4096];
        private static Socket DefaultSocket => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static EndPoint AllInterfacesEndPoint = new IPEndPoint(IPAddress.Any, Shared.Env.Port);


        /// <summary>Represents network wrapper for data with id.</summary>
        public struct Packet<DataType> {
            public int Uid { get; set; }
            public DataType Data { get; set; }

            public Packet(int uid, DataType data) {
                Uid = uid;
                Data = data;
            }
        }


        /// <summary>Represents errors for public using.</summary>
        public enum Error {
            UnknowError = 0,
            NetworkError,
            Disconnected,
            WrongAdress,
            ConnectionNotAllowed,
            UnsupportedConnection,
            InsufficientReceivedData,
            InsufficientSendData,
        }

        /// <summary>Represents errors for internal use only.</summary>
        private enum ErrorExtended {
            IgnoreThis = -2,
            RepeatAction = -1,
            // Error enum copy
            UnknowError = 0,
            NetworkError,
            Disconnected,
            WrongAdress,
            ConnectionNotAllowed,
            UnsupportedConnection,
            InsufficientReceivedData,
            InsufficientSendData,
        }

        /// <summary>Converts <c>SocketExteption</c> to <c>ExtendedError</c>.</summary>
        private static ErrorExtended SocketExceptionToError(SocketException socketException) {
            switch (socketException.SocketErrorCode) {
                case SocketError.IsConnected:
                case SocketError.Success:
                case SocketError.WouldBlock:
                    return ErrorExtended.IgnoreThis;

                case SocketError.AlreadyInProgress:
                case SocketError.InProgress:
                case SocketError.Interrupted:
                case SocketError.IOPending:
                case SocketError.OperationAborted:
                case SocketError.ProcessLimit:
                case SocketError.TimedOut:
                case SocketError.TryAgain:
                    return ErrorExtended.RepeatAction;

                case SocketError.NetworkDown:
                case SocketError.SystemNotReady:
                case SocketError.TooManyOpenSockets:
                    return ErrorExtended.NetworkError;

                case SocketError.ConnectionAborted:
                case SocketError.ConnectionReset:
                case SocketError.Disconnecting:
                case SocketError.HostDown:
                case SocketError.NetworkReset:
                case SocketError.NotConnected:
                case SocketError.Shutdown:
                    return ErrorExtended.Disconnected;

                case SocketError.AddressAlreadyInUse:
                case SocketError.AddressNotAvailable:
                case SocketError.DestinationAddressRequired:
                case SocketError.HostNotFound:
                case SocketError.HostUnreachable:
                case SocketError.InvalidArgument:
                case SocketError.NetworkUnreachable:
                case SocketError.NoData:
                    return ErrorExtended.WrongAdress;

                case SocketError.AccessDenied:
                case SocketError.ConnectionRefused:
                    return ErrorExtended.ConnectionNotAllowed;

                case SocketError.AddressFamilyNotSupported:
                case SocketError.OperationNotSupported:
                case SocketError.ProtocolFamilyNotSupported:
                case SocketError.ProtocolNotSupported:
                case SocketError.ProtocolOption:
                case SocketError.ProtocolType:
                case SocketError.SocketNotSupported:
                case SocketError.VersionNotSupported:
                    return ErrorExtended.UnsupportedConnection;

                case SocketError.MessageSize:
                case SocketError.NoBufferSpaceAvailable:
                    return ErrorExtended.InsufficientReceivedData;

                case SocketError.Fault:
                case SocketError.NoRecovery:
                case SocketError.NotInitialized:
                case SocketError.NotSocket:
                case SocketError.SocketError:
                case SocketError.TypeNotFound:
                default:
                    return ErrorExtended.UnknowError;
            }
        }



        /// <summary>Decrypts and deserializes the data.</summary>
        /// <param name="key">The key for data decryption.</param>
        /// <param name="data">The bytes to process.</param>
        /// <typeparam name="T">Type for deserialization.</typeparam>
        /// <returns>Original data if can deserialize into specified type, otherwise null.</returns>
        private static T? ReadBytes<T>(byte[] key, byte[] data) {
            //byte[] dataDecrypted = AesHelper.Decrypt(key, data);
            string dataJson = Encoding.ASCII.GetString(data);//dataDecrypted);
            if (dataJson.Length == 0 || dataJson[0] == '\0')
                return default(T);
            int end = dataJson.IndexOf('\0');
            if (end > 0)
                dataJson = dataJson.Substring(0, end);

            return JsonSerializer.Deserialize<T>(dataJson);
        }

        /// <summary>Serializes and encrypts the data.</summary>
        /// <param name="key">The key for data encryption.</param>
        /// <param name="data">The data to process.</param>
        /// <typeparam name="T">Type for serialization.</typeparam>
        /// <returns>Processed data in bytes.</returns>
        private static byte[] WriteBytes<T>(byte[] key, T data) {
            string dataJson = JsonSerializer.Serialize(data);
            if (dataJson.Length == 0 || dataJson[0] == '\0')
                return new byte[0];

            int end = dataJson.IndexOf('\0');
            if (end > 0)
                dataJson = dataJson.Substring(0, end);

            byte[] dataBytes = Encoding.ASCII.GetBytes(dataJson);
            return dataBytes;//AesHelper.Encrypt(key, dataBytes);
        }



        /// <summary>
        /// Represents network hub for hosts.
        /// For recreation, you have to dispose old object.
        /// </summary>
        /// <typeparam name="DataType">Specifies what type of data will be transferred in communications.</typeparam>
        public class Server<DataType> : IDisposable {
            private Socket server;
            private Dictionary<int, (Socket socket, byte[] key)> clients = new(8);

            /// <summary>UID counter.</summary>
            private int uid = 0;

            // /!\ CALLED ASYNC
            private Action<Packet<DataType>> OnDataReceive;
            // /!\ CALLED ASYNC
            private Func<int, Packet<DataType>> GetDataToSend;
            // /!\ CALLED ASYNC
            private Action<int> OnNewClient;
            // /!\ CALLED ASYNC
            private Action<int> OnClientDisconnect;
            /// <summary>Primary internal function to use.</summary>
            // /!\ CALLED ASYNC
            private void _OnClientDisconnect(int uid) {
                lock (clients) {
                    if (clients.ContainsKey(uid)) {
                        clients[uid].socket.Close();
                        clients.Remove(uid);
                    }
                }
                OnClientDisconnect(uid);
            }
            private Action<Error> OnHalt;

            /// <summary>Basic constructor-value-initializer.</summary>
            private Server(Socket server, Action<Packet<DataType>> onDataReceive, Func<int, Packet<DataType>> getDataToSend,
                Action<int> onNewClient, Action<int> onClientDisconnect, Action<Error> onHalt) {
                this.server = server;
                OnDataReceive = onDataReceive;
                GetDataToSend = getDataToSend;
                OnNewClient = onNewClient;
                OnClientDisconnect = onClientDisconnect;
                OnHalt = onHalt;
            }

            /// <summary>Factory for <c>Server</c>.</summary>
            /// <param name="onDataReceive">
            /// /!\ CALLED ASYNC
            /// Delegate called when server receives data. 
            /// <c>Packet</c> argument contains received data and sender UID.
            /// </param>
            /// <param name="getDataToSend">
            /// /!\ CALLED ASYNC
            /// Delegate called when server requesting data for sending.
            /// <c>int</c> argument specifies the host UID to which data will be sended.
            /// Delegate has to return <c>Packet</c> with data to send and UID who sends.
            /// If <c>Uid == -1</c>, packet is dropped and server is going to request next data.
            /// If <c>Data == null</c>, server is going to halt with <c>Error.InsufficientSendData</c>.
            /// </param>
            /// <param name="onNewClient">
            /// /!\ CALLED ASYNC
            /// Delegate called for notifying about new connect host.
            /// <c>int</c> argument specifies new host UID.
            /// </param>
            /// <param name="onClientDisconnect">
            /// /!\ CALLED ASYNC
            /// Delegate called for notifying about closed host connection.
            /// <c>int</c> argument specifies closed host UID.
            /// </param>
            /// <param name="onHalt">
            /// Delegate called if some server error occured.
            /// <c>Error</c> argument specifies user-frindly and most close to reality error.
            /// </param>
            /// <returns>If no errors occured ready for starting server, otherwise null.</returns>
            public static Server<DataType>? Create(Action<Packet<DataType>> onDataReceive, Func<int, Packet<DataType>> getDataToSend,
                Action<int> onNewClient, Action<int> onClientDisconnect, Action<Error> onHalt) {
                try {
                    Socket server = DefaultSocket;
                    server.Bind(AllInterfacesEndPoint);
                    return new Server<DataType>(server, onDataReceive, getDataToSend, onNewClient, onClientDisconnect, onHalt);
                } catch (Exception ex) {
                    ErrorExtended error = ErrorExtended.UnknowError;
                    if (ex is SocketException)
                        error = SocketExceptionToError((SocketException)ex);
                    if (error >= 0)
                        onHalt((Error)error);
                    return null;
                }
            }

            private CancellationTokenSource? _tokenListening;
            /// <summary>Gathering new host connections.</summary>
            private void Listen(byte[] inviteKey) {
                while (true) {
                    server.Listen(1);
                    Socket client = server.Accept();
                    bool receiving = true;
                    while (receiving) {
                        var result = IsInitKeyOK(client, inviteKey);
                        switch (result.state) {
                            case ErrorExtended.IgnoreThis:
                                if (!result.result) {
                                    client.Send(WriteBytes(Shared.Env.MainCryptoKey, new Packet<byte[]>(uid, new byte[] { })));
                                    client.Close();
                                }
                                receiving = false;
                                break;
                            case ErrorExtended.RepeatAction:
                                break;
                            case ErrorExtended.Disconnected:
                                receiving = false;
                                client.Close();
                                break;
                            default:
                                client.Close();
                                Halt(result.state);
                                return;
                        }
                    }

                    if (!client.Connected)
                        continue;

                    byte[] finalKey = new byte[32];
                    Shared.Env.Rnd.NextBytes(finalKey);

                    byte[] packet = WriteBytes(Shared.Env.MainCryptoKey, new Packet<byte[]>(uid, finalKey));

                    int bytesSent = 0;
                    for (int i = 0; i < Shared.Env.MaxTryCount && bytesSent != packet.Length; i++) {
                        try {
                            bytesSent = client.Send(packet);
                        } catch (Exception ex) {
                            if (ex is not SocketException)
                                throw;
                            ErrorExtended er = SocketExceptionToError((SocketException)ex);
                            switch (er) {
                                case ErrorExtended.IgnoreThis:
                                    break;
                                case ErrorExtended.RepeatAction:
                                    continue;
                                case ErrorExtended.Disconnected:
                                    i = Shared.Env.MaxTryCount;
                                    bytesSent = -1;
                                    continue;
                                default:
                                    Halt(er);
                                    return;
                            }
                        }
                    }
                    if (bytesSent != packet.Length) {
                        client.Close();
                        continue;
                    }

                    // no need of locking, at this moment only this function accessing collection
                    clients.Add(uid, (client, finalKey));
                    ReceiveSendFactory(uid, client, finalKey);
                    OnNewClient(uid);

                    uid++;
                }
            }

            private (ErrorExtended state, bool result) IsInitKeyOK(Socket client, byte[] initKey) {
                byte[]? buffer = DefaultBuffer;
                try {
                    client.Receive(buffer);
                } catch (Exception ex) {
                    ErrorExtended er = ErrorExtended.UnknowError;
                    if (ex is SocketException)
                        er = SocketExceptionToError((SocketException)ex);
                    return (er, new());
                }

                //AesHelper.Decrypt(Shared.Env.MainCryptoKey, buffer);
                buffer = ReadBytes<byte[]>(Shared.Env.MainCryptoKey, buffer);
                if (buffer == null)
                    return (ErrorExtended.InsufficientReceivedData, new());

                return (ErrorExtended.IgnoreThis, buffer.SequenceEqual(initKey));
            }

            private void Receive(int uid, Socket socket, byte[] key) {
                while (true) {
                    byte[] buffer = DefaultBuffer;

                    if (!clients.ContainsKey(uid))
                        return;

                    try {
                        socket.Receive(buffer);

                        Packet<byte[]>? packet = ReadBytes<Packet<byte[]>>(Shared.Env.MainCryptoKey, buffer);
                        if (packet == null)
                            continue;

                        // /!\ do not use Packet.Uid it's unsafe value
                        Packet<DataType>? data = ReadBytes<Packet<DataType>>(key, packet.Value.Data);
                        if (data == null)
                            continue;

                        OnDataReceive(new Packet<DataType>(uid, data.Value.Data));
                    } catch (Exception ex) {
                        if (ex is JsonException)
                            continue;
                        ErrorExtended er = ErrorExtended.UnknowError;
                        if (ex is SocketException)
                            er = SocketExceptionToError((SocketException)ex);
                        switch (er) {
                            case ErrorExtended.IgnoreThis:
                                break;
                            case ErrorExtended.RepeatAction:
                                continue;
                            case ErrorExtended.Disconnected:
                                _OnClientDisconnect(uid);
                                continue;
                            default:
                                Halt(er);
                                return;
                        }
                    }
                }
            }

            private void Send(int uid, Socket socket, byte[] key) {
                while (true) {
                    Packet<DataType> dataToSend = GetDataToSend(uid);

                    if (dataToSend.Uid == -2)
                        return;

                    if (dataToSend.Uid == -1)
                        continue;

                    if (dataToSend.Data == null)
                        Halt(ErrorExtended.InsufficientSendData);

                    try {
                        byte[] dataEncrypted = WriteBytes(key, dataToSend);
                        byte[] packet = WriteBytes(Shared.Env.MainCryptoKey, new Packet<byte[]>(dataToSend.Uid, dataEncrypted));

                        int bytesSent = 0;
                        for (int i = 0; i < Shared.Env.MaxTryCount && bytesSent != packet.Length; i++) {
                            try {
                                bytesSent = socket.Send(packet);
                            } catch (Exception ex) {
                                if (ex is ObjectDisposedException)
                                    return;

                                ErrorExtended er = ErrorExtended.UnknowError;
                                if (ex is SocketException)
                                    er = SocketExceptionToError((SocketException)ex);

                                switch (er) {
                                    case ErrorExtended.IgnoreThis:
                                        break;
                                    case ErrorExtended.RepeatAction:
                                        continue;
                                    case ErrorExtended.Disconnected:
                                        _OnClientDisconnect(dataToSend.Uid);
                                        return;
                                    default:
                                        Halt(er);
                                        return;
                                }
                            }
                        }
                    } catch (Exception ex) {
                        if (ex is JsonException)
                            Halt(ErrorExtended.InsufficientSendData);
                        else
                            Halt(ErrorExtended.UnknowError);
                    }
                }
            }

            private void ReceiveSendFactory(int uid, Socket socket, byte[] key) {
                if (_tokenReceiveSend == null)
                    return;

                Task.Factory.StartNew(() => Receive(uid, socket, key), _tokenReceiveSend.Token);
                Task.Factory.StartNew(() => Send(uid, socket, key), _tokenReceiveSend.Token);
            }

            private CancellationTokenSource? _tokenReceiveSend;
            /// <summary>
            /// Starts server. 
            /// As first listening for new connection and after starts receiving and sending on enstablished socket.
            /// </summary>
            /// <param name="inviteKey">Pre-Shared with hosts key for first handshake initiation.</param>
            public void Start(byte[] inviteKey) {
                _tokenReceiveSend = new CancellationTokenSource();

                _tokenListening = new CancellationTokenSource();
                Task listening = new Task(() => Listen(inviteKey), _tokenListening.Token);

                listening.Start();
            }

            public void StopListening() => _tokenListening?.Cancel();

            /// <summary>Stops all tasks and closes all connections.</summary>
            public void Stop() {
                if (_tokenReceiveSend != null && _tokenReceiveSend.IsCancellationRequested)
                    return;
                _tokenReceiveSend?.Cancel();
                _tokenListening?.Cancel();
                lock (clients) {
                    for (int i = 0; i < clients.Count; i++)
                        clients[i].socket.Close();
                    clients.Clear();
                }
            }

            private void Halt(ErrorExtended e) {
                if (_tokenReceiveSend != null && _tokenReceiveSend.IsCancellationRequested)
                    return;
                if (!Monitor.TryEnter(OnHalt))
                    return;
                Stop();
                int code = (int)e;
                if (code < 0)
                    return;
                OnHalt((Error)code);
                Monitor.Exit(OnHalt);
            }

            public void Dispose() {
                Stop();
                server.Close();
            }

            /// <summary>On object deleting, make sure all tasks are stopped.</summary>
            ~Server() {
                Dispose();
            }
        }



        /// <summary>
        /// Represents network host.
        /// For recreation, you have to dispose old object.
        /// </summary>
        /// <typeparam name="DataType">Specifies what type of data will be transferred in communications.</typeparam>
        public class Client<DataType> : IDisposable {
            private Socket server;
            public int Uid { get; private set; }
            private byte[] key = { };

            // /!\ CALLED ASYNC
            private Action<Packet<DataType>> OnDataReceive;
            // /!\ CALLED ASYNC
            private Func<DataType> GetDataToSend;
            // /!\ CALLED ASYNC
            private Action<int> OnConnect;
            // /!\ CALLED ASYNC
            private Action OnDisconnect;
            /// <summary>Primary internal function to use.</summary>
            // /!\ CALLED ASYNC
            private void _OnDisconnect() {
                Stop();
                OnDisconnect();
            }
            private Action<Error> OnHalt;

            /// <summary>Basic constructor-value-initializer.</summary>
            private Client(Socket server, Action<Packet<DataType>> onDataReceive, Func<DataType> getDataToSend,
                Action<int> onConnect, Action onDisconnect, Action<Error> onHalt) {
                this.server = server;
                OnDataReceive = onDataReceive;
                GetDataToSend = getDataToSend;
                OnConnect = onConnect;
                OnDisconnect = onDisconnect;
                OnHalt = onHalt;
            }

            /// <summary>Factory for <c>Client</c>.</summary>
            /// <param name="onDataReceive">
            /// /!\ CALLED ASYNC
            /// Delegate called when client receives data. 
            /// <c>DataType</c> argument contains received data.
            /// </param>
            /// <param name="getDataToSend">
            /// /!\ CALLED ASYNC
            /// Delegate called when client requesting data for sending.
            /// Delegate has to return <c>DataType data</c> to send.
            /// </param>
            /// <param name="onConnect">
            /// /!\ CALLED ASYNC
            /// Delegate called for notifying about connecting to server.
            /// <c>int</c> argument specifies host new UID.
            /// </param>
            /// <param name="onDisconnect">
            /// /!\ CALLED ASYNC
            /// Delegate called for notifying about closed server connection.
            /// </param>
            /// <param name="onHalt">
            /// Delegate called if some client error occured.
            /// <c>Error</c> argument specifies user-frindly and most close to reality error.
            /// </param>
            /// <returns>If no errors occured ready for starting client, otherwise null.</returns>
            public static Client<DataType>? Create(Action<Packet<DataType>> onDataReceive, Func<DataType> getDataToSend,
                Action<int> onConnect, Action onDisconnect, Action<Error> onHalt) {
                try {
                    return new Client<DataType>(DefaultSocket, onDataReceive, getDataToSend, onConnect, onDisconnect, onHalt);
                } catch (Exception ex) {
                    ErrorExtended error = ErrorExtended.UnknowError;
                    if (ex is SocketException)
                        error = SocketExceptionToError((SocketException)ex);
                    if (error >= 0)
                        onHalt((Error)error);
                    return null;
                }
            }

            private CancellationTokenSource? _tokenConnecting;
            private void Connect(byte[] address, byte[] inviteKey) {
                bool connecting = true;
                while (connecting) {
                    connecting = false;
                    try {
                        server.Connect(new IPAddress(address), Shared.Env.Port);
                        byte[] packetHello = DefaultBuffer;
                        WriteBytes(Shared.Env.MainCryptoKey, inviteKey).CopyTo(packetHello, 0);
                        int bytesSent = 0;
                        for (int i = 0; i < Shared.Env.MaxTryCount && bytesSent != packetHello.Length; i++) {
                            try {
                                bytesSent = server.Send(packetHello);
                            } catch (Exception ex) {
                                ErrorExtended er = ErrorExtended.UnknowError;
                                if (ex is SocketException)
                                    er = SocketExceptionToError((SocketException)ex);
                                switch (er) {
                                    case ErrorExtended.IgnoreThis:
                                        break;
                                    case ErrorExtended.RepeatAction:
                                        continue;
                                    case ErrorExtended.Disconnected:
                                        _OnDisconnect();
                                        Stop();
                                        return;
                                    default:
                                        Halt(er);
                                        return;
                                }
                            }
                        }

                        byte[] buffer = DefaultBuffer;
                        server.Receive(buffer);

                        Packet<byte[]>? response = ReadBytes<Packet<byte[]>>(Shared.Env.MainCryptoKey, buffer);
                        if (response == null) {
                            Halt(ErrorExtended.InsufficientReceivedData);
                            return;
                        }

                        if (response?.Data?.Length == 0) {
                            OnHalt(Error.InsufficientSendData);
                            return;
                        }

                        // suppress, bc IDE has some issue to recognize that this cant be null
#pragma warning disable CS8629 // Nullable value type may be null.
                        Uid = response.Value.Uid;
                        key = response.Value.Data;
#pragma warning restore CS8629 // Nullable value type may be null.
                        OnConnect(Uid);

                    } catch (Exception ex) {
                        ErrorExtended er = ErrorExtended.UnknowError;
                        if (ex is SocketException)
                            er = SocketExceptionToError((SocketException)ex);
                        switch (er) {
                            case ErrorExtended.IgnoreThis:
                                break;
                            case ErrorExtended.RepeatAction:
                                connecting = true;
                                continue;
                            default:
                                Halt(er);
                                return;

                        }
                    }
                }
            }

            private void Receive() {
                while (true) {
                    byte[] buffer = DefaultBuffer;

                    try {
                        server.Receive(buffer);

                        Packet<byte[]>? packet = ReadBytes<Packet<byte[]>>(Shared.Env.MainCryptoKey, buffer);
                        if (packet == null)
                            continue;

                        Packet<DataType>? data = ReadBytes<Packet<DataType>>(key, packet.Value.Data);
                        if (data == null)
                            continue;

                        OnDataReceive(data.Value);
                        continue;

                    } catch (Exception ex) {
                        if (ex is JsonException)
                            continue;
                        ErrorExtended er = ErrorExtended.UnknowError;
                        if (ex is SocketException)
                            er = SocketExceptionToError((SocketException)ex);

                        switch (er) {
                            case ErrorExtended.IgnoreThis:
                                break;
                            case ErrorExtended.RepeatAction:
                                continue;
                            case ErrorExtended.Disconnected:
                                _OnDisconnect();
                                return;
                            default:
                                Halt(er);
                                return;

                        }
                    }
                }
            }

            private void Send() {
                try {
                    while (true) {
                        DataType dataToSend = GetDataToSend();
                        Packet<DataType> packedDataToSend = new Packet<DataType>(Uid, dataToSend);
                        byte[] dataEncrypted = WriteBytes(key, packedDataToSend);
                        byte[] packet = WriteBytes(Shared.Env.MainCryptoKey, new Packet<byte[]>(Uid, dataEncrypted));

                        int bytesSent = 0;
                        for (int i = 0; i < Shared.Env.MaxTryCount && bytesSent != packet.Length; i++) {
                            try {
                                bytesSent = server.Send(packet);
                            } catch (Exception ex) {
                                ErrorExtended er = ErrorExtended.UnknowError;
                                if (ex is SocketException)
                                    er = SocketExceptionToError((SocketException)ex);

                                switch (er) {
                                    case ErrorExtended.IgnoreThis:
                                        break;
                                    case ErrorExtended.RepeatAction:
                                        continue;
                                    case ErrorExtended.Disconnected:
                                        _OnDisconnect();
                                        return;
                                    default:
                                        Halt(er);
                                        return;
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    if (ex is JsonException)
                        Halt(ErrorExtended.InsufficientSendData);
                    else
                        Halt(ErrorExtended.UnknowError);
                }
            }

            private CancellationTokenSource? _tokenReceiveSend;
            /// <summary>Starts client. As first connectiong to server and after starts receiving and sending.</summary>
            /// <param name="inviteKey">Pre-Shared with hosts key for first handshake inition.</param>
            /// <param name="max">Higher bound of maximum hosts to gather.</param>
            public void Start(byte[] address, byte[] inviteKey) {
                _tokenReceiveSend = new CancellationTokenSource();
                Task receive = new Task(Receive, _tokenReceiveSend.Token);
                Task send = new Task(Send, _tokenReceiveSend.Token);

                _tokenConnecting = new CancellationTokenSource();
                Task connectToServer = new Task(() => Connect(address, inviteKey), _tokenConnecting.Token);
                connectToServer.ContinueWith(_ => { receive.Start(); send.Start(); }, _tokenReceiveSend.Token);

                connectToServer.Start();
            }

            /// <summary>Stops all tasks and closes connection.</summary>
            public void Stop() {
                if (_tokenReceiveSend != null && _tokenReceiveSend.IsCancellationRequested)
                    return;
                _tokenReceiveSend?.Cancel();
                _tokenConnecting?.Cancel();
                if (server != null)
                    server.Close();
            }

            private void Halt(ErrorExtended e) {
                if (_tokenReceiveSend != null && _tokenReceiveSend.IsCancellationRequested)
                    return;
                if (!Monitor.TryEnter(OnHalt))
                    return;
                Stop();
                int code = (int)e;
                if (code < 0)
                    return;
                OnHalt((Error)code);
                Monitor.Exit(OnHalt);
            }

            public void Dispose() {
                Stop();
                server.Close();
            }

            /// <summary>On object deleting, make sure all tasks are stopped.</summary>
            ~Client() {
                Dispose();
            }
        }



        /// <summary>High-level interface for user friendly data (En/De)cryption.</summary>
        private static class AesHelper {
            private enum Operation {
                Encrypt,
                Decrypt,
            }

            private static byte[] PerformOperation(Operation operation, byte[] key, byte[] data) {
                using (Aes aes = Aes.Create()) {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = new byte[16];
                    using (MemoryStream ms = new MemoryStream()) {
                        var cryptor = (operation == Operation.Encrypt) ? aes.CreateEncryptor() : aes.CreateDecryptor();
                        using (CryptoStream cs = new CryptoStream(ms, cryptor, CryptoStreamMode.Write)) {
                            cs.Write(data, 0, data.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }

            public static byte[] Encrypt(byte[] key, byte[] data) => PerformOperation(Operation.Encrypt, key, data);
            public static byte[] Decrypt(byte[] key, byte[] data) => PerformOperation(Operation.Decrypt, key, data);
        }
    }
}

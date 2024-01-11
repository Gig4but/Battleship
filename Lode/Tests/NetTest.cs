/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2023
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

//using Lode;
//using static Lode.Game;
//
//int i = 0;
//Net.Server<string>? server = Net.Server<string>.Create(
//    (Net.Packet<string> p) => { Console.WriteLine($"Client sent: {p.Data}"); i = -1; },
//    (int uid) => {
//        while (i != -1)
//            continue;
//        Console.WriteLine("Crashing server...");
//        return new();
//    },
//    (int id) => { Console.WriteLine("Client conneted"); },
//    (int id) => { },
//    (Net.Error er) => { Console.WriteLine($"Server crashed with error: {er}"); }
//    );
//if (server == null) {
//    Console.WriteLine("Server creation is null");
//    return;
//}
//
//Net.Client<string>? client = Net.Client<string>.Create(
//(string s) => { Console.WriteLine($"Server sent: {s}"); },
//() => { return "Hello world!"; },
//() => { Console.WriteLine("Server disconnected"); end = true; },
//(Net.Error er) => { Console.WriteLine($"Client crashed with error: {er}"); }
//);
//if (client == null) {
//    Console.WriteLine("Client creation is null");
//    return;
//}
//
//server.Start(new byte[] { 0, 1, 1, 1 }, 1);
//client.Start(new byte[] { 127, 0, 0, 1 }, new byte[] { 0, 1, 1, 1 });
﻿using System.Net;
using System.Net.Sockets;
using System.Text;

Console.Title = "--- TCP Socket Client ---";

Console.WriteLine("--- TCP Socket Client ---");

// адрес сервера и его порт
var ip = "127.0.0.1";
// var ip = "192.168.0.100";
// var ip = "10.1.196.107";
int port = 8085;


string[] msges = {
     "date",
     "host_name",
     "pwd",
     "list",
     "mul 12.5 1.8",
     "sum 2.7 8.4",
     "solve 0 12 36",
     "solve 1 12 36",
     "solve 2 12 6",
     "rename file1.ext file2.ext",
     "rename index.html file1.ext",
     "list",
     "rename file1.ext index.html",
     "list",
     "shutdown"
 };

foreach (var msg in msges) {
    Console.Write($"TcpClient: \"{msg}\"");
    TcpClient(port, IPAddress.Parse(ip), msg);
    Console.WriteLine();
} // foreach msg


Console.Write("\n\nНажмите любую клавишу для продолжения...");
Console.ReadKey(true);
Console.WriteLine("\n\n");
return;


// клиентские операции с TCP-сокетом
void TcpClient (int portServer, IPAddress ipServer, string msg) {
    // конечная точка для клиента - это сервер
    var ipPointServer = new IPEndPoint(ipServer, portServer);

    // сокет для клиента
    // AddressFamily.InterNetwork - IP v4
    // SocketType.Stream          - работаем с пакетами TCP 
    // ProtocolType.Tcp           - протокол транспортного уровня
    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    try {
        // подключение к серверу - блокирующий вызов до установки соединения
        socket.Connect(ipPointServer);

        // формирование массива байтов для отправки
        byte[] data = Encoding.Unicode.GetBytes(msg);

        // отправка запроса на сервер
        socket.Send(data);

        // получение ответа от сервера
        data = new byte[1536]; // буфер для ответа сервера
        var sbr = new StringBuilder(); // контейнер для декодированного ответа сервера

        // чтение данных сервера из сокета пока есть, что читать
        do {
            // !!!! собственно чтение данных от Сервера !!!!
            var bytes = socket.Receive(data, data.Length, 0);
            sbr.Append(Encoding.Unicode.GetString(data, 0, bytes));
        } while (socket.Available > 0);

        // вывод полученного ответа от сервера
        Console.WriteLine($"\nTcpClient: {sbr}");

        socket.Shutdown(SocketShutdown.Both);
    } // try 
    catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n\n{ex.Message}\n\n");
        Console.ForegroundColor = ConsoleColor.Gray;
    } // catch
    finally {
        socket.Close();
    } // finally
} // TcpClient

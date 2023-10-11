using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

Console.WindowHeight = 20;
Console.WindowWidth = 60;
Console.Title = "--- TCP Socket Server ---";

(Console.ForegroundColor, Console.BackgroundColor) =
    (ConsoleColor.Blue, ConsoleColor.Black);
Console.WriteLine("--- TCP Socket Server ---");

// Задать символ "точка" в качестве разделителя целой и дробной 
// частей вещественных чисел
#region для решения проблемы ввода дробных чисел в поля типа number
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
#endregion

// адрес сервера и его порт
var ip = "127.0.0.1";
// var ip = "192.168.0.100";
// var ip = "10.1.196.107";
int tcpPort = 8085;
TcpServer(tcpPort, IPAddress.Parse(ip));

Console.WriteLine("Bye, bye.");
return;


// port порт для работы сервера
// ip   адрес сервера
void TcpServer(int port, IPAddress ip) {
    // конечная точка для сервера: IP и порт
    var ipPoint = new IPEndPoint(ip, port);

    // сокет для прослушивания сети
    // AddressFamily.InterNetwork - IP v4
    // SocketType.Stream          - работаем с пакетами 
    // ProtocolType.Tcp           - протокол транспортного уровня    TCP/IP -- DARPA    IPX/SPX -- Novell
    var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    try {
        // привязка сокета к конечной точке
        listenSocket.Bind(ipPoint);

        // прослушиваем сеть по протоколу TCP на заданном порту
        // backlog: максимальная длина очереди ожидающих подключений, в данном случае 10 
        listenSocket.Listen(10);
        Console.WriteLine($"Сервер стартовал, {ip}::{port}, ожидание подключений...");

        // цикл работы сервера
        while (true) {
            // блокирующий вызов, ожидание подключения, после подключения
            // используем сокет handler для обмена данными 
            Socket handler = listenSocket.Accept();

            // клиент обратился с запросом, поэтому Accept() завершен,
            // получим данные клиента - для примера это строка символов
            var data = new byte[1536];      // 1536 - блок данных TCP
            var sbr = new StringBuilder(); // контейнер для декодированного запроса клиента

            // чтение данных клиента из сокета пока есть, что читать
            do {
                // !!!! собственно чтение данных от Клиента !!!!
                var bytes = handler.Receive(data); // количество полученных байтов
                sbr.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (handler.Available > 0);

            // обработка клиенткого запроса

            // вывод полученного сообщения от клиента 
            Console.WriteLine($"TcpServer {DateTime.Now:G}: {sbr}");

            // отправка ответа Клиенту
            // выполнение команд "time", "send сообщение" Клиента
            string answer;
            var clientCommand = sbr.ToString();
            var tokens = clientCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (tokens[0]) {
                case "date":
                answer = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                break;

                // host_name – возвращает имя компьютера, на котором работает сервер
                case "host_name":
                answer = $"{Environment.MachineName}";
                break;

                // pwd – возвращает полное имя папки App_Files приложения
                case "pwd":
                var path = $"{Environment.CurrentDirectory}\\App_Files";
                answer = Directory.Exists(path)
                    ? path
                    : "not found";
                break;

                // list ¬– клиент получает список имен файлов, хранящихся
                // на сервере, в папке App_Files (в папке исполняемого файла),
                // имена файлов разделены строкой “\n”
                case "list":
                path = $"{Environment.CurrentDirectory}\\App_Files";
                answer = Directory.Exists(path)
                    ? string.Join("", Directory.GetFiles(path)
                        .Select(f => Path.GetFileName(f)!)
                        .ToList())
                    : "not found";
                break;

                // mul число1 число2 – сервер возвращает строку, содержащую
                // два вещественных числа и произведение этих чисел 
                case "mul":
                // получить числа из строки запроса
                if (tokens.Length != 3) {
                    answer = "mul: invalid arguments number";
                } else {
                    double number1 = double.Parse(tokens[1]);
                    double number2 = double.Parse(tokens[2]);
                    answer = $"{number1:f5} * {number2:f5} = {number1 * number2:f5}";
                } // if
                break;

                // sum число1 число2 – сервер возвращает строку, содержащую
                // два вещественных числа и сумму этих чисел   
                case "sum":
                // получить числа из строки запроса
                if (tokens.Length != 3) {
                    answer = "sum: invalid arguments number";
                } else {
                    double number1 = double.Parse(tokens[1]);
                    double number2 = double.Parse(tokens[2]);
                    answer = $"{number1:f5} + {number2:f5} = {number1 + number2:f5}";
                } // if
                break;

                // solve a b c – сервер возвращает три числа a, b, c
                // и вычисленные корни квадратного уравнения 〖a∙x〗^2+b∙x+c=0,
                // при отсутствии действительных корней возвращать числа a, b,
                // c и строку “\nнет корней\n”
                case "solve":
                if (tokens.Length != 4) {
                    answer = "solve: invalid arguments number";
                } else {
                    var a = double.Parse(tokens[1]);
                    var b = double.Parse(tokens[2]);
                    var c = double.Parse(tokens[3]);
                    answer = $"{a:f5} {b:f5} {c:f5}: ";

                    var d = b * b - 4 * a * c;
                    if (!a.Equals(0) && d >= 0) {
                        var t = 2 * a;
                        d = Math.Sqrt(d);
                        var x1 = (-b - d) / t;
                        var x2 = (-b + d) / t;
                        answer += $"{x1:f5} {x2:f5}";
                    } else {
                        answer += "\nнет корней\n";
                    } // if
                } // if
                break;

                // shutdown – завершение работы сервера
                case "shutdown":
                answer = "halted";
                break;
                
                default:
                // любой другой токен от клиента - просто выводим его в консоль
                answer = $"{DateTime.Now} \"{sbr}\"";
                break;
            } // if

            // преобразовать ответ в массив байтов и передать клиенту
            data = Encoding.Unicode.GetBytes(answer);
            handler.Send(data);

            // закрыть сокет, используемый для обмена информацией
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            // завершение работы сервера
            if (tokens[0] == "shutdown") break;
        } // while
    } catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n\n{ex.Message}\n\n");
        Console.ForegroundColor = ConsoleColor.Gray;
    } finally {
        listenSocket.Close();
    } // try-catch-finally
} // TcpServer

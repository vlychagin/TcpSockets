using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTcpServer.Controllers;

// Серверные операции
public class ServerController
{

    public string Pwd() {
        var path = $"{Environment.CurrentDirectory}\\App_Files";
        return Directory.Exists(path)
            ? path
            : "not found";
    } // Pwd

} // class ServerController

using Jindium;
using jSock;

namespace SimpleWebChat;

class User
{
    public string ClientID { get; set; }
    public string Name { get; set; }

    public User(string clientID, string name = "Unknown")
    {
        ClientID = clientID;
        Name = name;
    }
}

class Program
{
    //Objects for both Jindium and jSock.
    private static JinServer jinServer = new JinServer("http://localhost:5000/");
    private static jSockServer server = new jSockServer("127.0.0.1", 8080);

    //A a list to store chat users.
    private static List<User> users = new List<User>();

    static void Main()
    {
        jinServer.ServerRoutes.AddContentRoute("/", "/index.html");
        jinServer.ServerRoutes.AddContentRoute("/", "/");

        //Events for jSock communication.
        server.OnConnect += Server_OnConnect;
        server.OnDisconnect += Server_OnDisconnect;
        server.OnRecieve += Server_OnRecieve;
        server.OnError += Server_OnError;

        server.Start();
        jinServer.Start();
    }

    private static void Server_OnConnect(int clientID)
    {
        Console.WriteLine($"A client has connected! ({clientID.ToString()})");
        users.Add(new User(clientID.ToString()));
    }

    private static void Server_OnDisconnect(int clientID)
    {
        Console.WriteLine($"A client has disconnected! ({clientID.ToString()})");
        users.RemoveAll(x => x.ClientID == clientID.ToString());
    }

    private static void Server_OnRecieve(int clientID, string text)
    {
        if (text.StartsWith("/"))
        {
            if (text.StartsWith("/name "))
            {
                string name = text.Substring(6);

                if (name.Length >= 3 && users.Any(x => x.ClientID == clientID.ToString()))
                {
                    users.First(x => x.ClientID == clientID.ToString()).Name = name;
                    server.Reply(clientID, $"System: Your name has been set to '{name}', change it with '/name'");
                }
                else
                {
                    server.Reply(clientID, "Your name must be at least 3 characters long!");
                }
            }
        }
        else
        {
            string name = "UNKNOWN";
            if (users.Any(x => x.ClientID == clientID.ToString()))  name = users.First(x => x.ClientID == clientID.ToString()).Name;

            Console.WriteLine(name + ": " + text);
            server.Broadcast(name + ": " + text);
        }
    }

    private static void Server_OnError(string data)
    {
        Console.WriteLine("Error:" + data);
    }
}
using System.Net.Sockets;
using System.Text;
using NLog;

namespace Plus.Communication.RCON;

public class RconConnection
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.Rcon.RconConnection");
    private byte[] _buffer = new byte[1024];
    private Socket _socket;
    private readonly IRconSocket _rconSocket;

    public RconConnection(Socket socket, IRconSocket rconSocket)
    {
        _socket = socket;
        _rconSocket = rconSocket;
        try
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnCallBack, _socket);
        }
        catch
        {
            Dispose();
        }
    }

    public void OnCallBack(IAsyncResult iAr)
    {
        try
        {
            if (!int.TryParse(_socket.EndReceive(iAr).ToString(), out var bytes))
            {
                Dispose();
                return;
            }
            var data = Encoding.Default.GetString(_buffer, 0, bytes);
            if (!_rconSocket.GetCommands().Parse(data)) Log.Error($"Failed to execute a MUS command. Raw data: {data}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        Dispose();
    }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
        }
        _socket = null;
        _buffer = null;
    }
}
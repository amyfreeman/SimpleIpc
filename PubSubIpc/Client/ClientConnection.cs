using log4net;
using PubSubIpc.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubIpc.Client
{
    public abstract class ClientConnection : Connection
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _port;

        protected ClientConnection(int port = 13001)
        {
            _port = port;
        }

        protected void ConnectToServer()
        {
            log.Debug("Establishing a connection");
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(remoteEP);
        }

        protected void SendControl(ControlBytes controlByte, string message = null)
        {
            log.Debug($"Sending control (byte = {controlByte}, message = {message})");
            byte[] bytesToSend;
            if (message != null)
            {
                List<byte> encodedMessage = Encoding.ASCII.GetBytes(message).ToList();
                bytesToSend = encodedMessage.Prepend((byte)controlByte).Prepend((byte)ControlBytes.Escape).ToArray();
            }
            else
            {
                bytesToSend = new byte[]{(byte)ControlBytes.Escape, (byte)controlByte};
            }
            _sendDataSubject.OnNext(bytesToSend);
        }
    }
}
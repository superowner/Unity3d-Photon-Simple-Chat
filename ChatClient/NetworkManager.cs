using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    public class Event
    {
        public delegate void OnConnected(IPhotonPeerListener peer);
        public static OnConnected onConnected;
        public delegate void OnDisConnected(IPhotonPeerListener peer);
        public static OnDisConnected onDisConnected;
        public delegate void OnMessage(object messages);
        public static OnMessage onMessage;
        public delegate void OnOperationResponse(OperationResponse operationResponse);
        public static OnOperationResponse onOperationResponse;
        public delegate void OnEvent(EventData eventData);
        public static OnEvent onEvent;
    }

    public class NetworkManager : IPhotonPeerListener
    {
        public bool connected;
        public PhotonPeer peer;

        public NetworkManager()
        {
            connected = false;
            peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        }

        public bool Connect()
        {
            if (connected) return false;
            peer.Connect("127.0.0.1:5055", "ChatServer");
            while (!this.connected)
            {
                peer.Service();
            }
            return true;
        }

        public bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte? channelId)
        {
            if (!connected) return false;
            if (channelId != null) return this.peer.OpCustom(customOpCode, customOpParameters, sendReliable, (byte)channelId);
            return this.peer.OpCustom(customOpCode, customOpParameters, sendReliable);
        }

        public void DebugReturn(DebugLevel level, string message)
        {
            Console.WriteLine(level + ": " + message);
        }

        public void OnEvent(EventData eventData)
        {
            Console.WriteLine("Event: " + eventData.Code);
            if (Event.onEvent != null) Event.onEvent(eventData);
        }

        public void OnMessage(object messages)
        {
            Console.WriteLine("Message: " + messages);
            if (Event.onMessage != null) Event.onMessage(messages);
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            Console.WriteLine("Response: " + operationResponse.OperationCode);
            if (Event.onOperationResponse != null) Event.onOperationResponse(operationResponse);
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            if (statusCode == StatusCode.Connect)
            {
                connected = true;
                if (Event.onConnected != null) Event.onConnected(this);
            }
            else if (statusCode == StatusCode.Disconnect)
            {
                connected = false;
                if (Event.onDisConnected != null) Event.onDisConnected(this);
            }
            else
            {
                Console.WriteLine("Status: " + statusCode);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ChatServer.Common;
using System.Collections;

namespace ChatServer
{

    public class ChatServerPeer : ClientPeer
    {
        private static readonly object syncRoot = new object();

        private static Action<ChatServerPeer, EventData, SendParameters> BroadcastNotify; // event
        private void broadcastNotify(ChatServerPeer peer, EventData eventData, SendParameters sendParameters)
        {
            if (peer != this)
            {
                this.SendEvent(eventData, sendParameters);
            }
        }

        private static Dictionary<Guid, ChatServerPeer> _peerCollection = new Dictionary<Guid, ChatServerPeer>();

        private Guid ClientId;
        private string name;

        public ChatServerPeer(InitRequest initRequest) : base(initRequest)
        {
            ClientId = Guid.NewGuid();
            _peerCollection.Add(ClientId, this);

            BroadcastNotify += broadcastNotify; // sub event
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            _peerCollection.Remove(ClientId);

            BroadcastNotify -= broadcastNotify; // un-sub event
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch ((OperationCode)operationRequest.OperationCode)
            {
                case OperationCode.JOIN_CHAT:
                    onJoin(operationRequest, sendParameters);
                    break;
                case OperationCode.CHAT:
                    onChat(operationRequest, sendParameters);
                    break;
                default:
                    break;
            }
        }

        public void onJoin(OperationRequest operationRequest, SendParameters sendParameters)
        {
            lock (syncRoot)
            {
                object _name;
                operationRequest.Parameters.TryGetValue((byte)Data.DATA1, out _name);
                this.name = _name.ToString();

                OperationResponse operationResponse = new OperationResponse { OperationCode = (byte)OperationCode.JOIN_CHAT, Parameters = new Dictionary<byte, object>() { { (byte)Data.DATA1, this.name } }, ReturnCode = (byte)ErrorCode.NONE };
                this.SendOperationResponse(operationResponse, sendParameters);

                EventData e = new EventData { Code = (byte)EventCode.ANOTHER_JOIN, Parameters = new Dictionary<byte, object>() { { (byte)Data.DATA1, this.name } } };
                BroadcastNotify(this, e, sendParameters);
            }
        }

        private void onChat(OperationRequest operationRequest, SendParameters sendParameters)
        {
            lock (syncRoot)
            {
                OperationResponse operationResponse = new OperationResponse { OperationCode = (byte)OperationCode.CHAT, Parameters = new Dictionary<byte, object>() { { (byte)Data.DATA1, this.name }, { (byte)Data.DATA2, operationRequest.Parameters[(byte)Data.DATA1] } }, ReturnCode = (byte)ErrorCode.NONE };
                this.SendOperationResponse(operationResponse, sendParameters);

                EventData e = new EventData { Code = (byte)EventCode.CHAT, Parameters = operationResponse.Parameters };
                BroadcastNotify(this, e, sendParameters);
            }
        }
    }

}

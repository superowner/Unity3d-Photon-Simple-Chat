using ChatServer.Common;
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient
{
    public partial class ChatClientForm : Form
    {
        private bool isDebug = false;
        private NetworkManager client;

        public ChatClientForm()
        {
            InitializeComponent();

            lblStatus.Text = "Disconnected";
            txtChatContent.ReadOnly = true;
            txtChat.ReadOnly = true;
            btnChat.Enabled = false;

            // add event handler
            Event.onConnected += onConnected;
            Event.onDisConnected += onDisConnected;
            Event.onMessage += onMessage;
            Event.onOperationResponse += onOperationResponse;
            Event.onEvent += onEvent;

            client = new NetworkManager();

        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(Connect));
            thread.IsBackground = true;
            thread.Start();
        }

        public void Connect()
        {
            client.Connect();

            while (true)
            {
                client.peer.Service();
            }
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            client.OpCustom((byte)OperationCode.CHAT, new Dictionary<byte, object> { { (byte)Data.DATA1, txtChat.Text } }, true, null);
            txtChat.Text = "";
        }

        public void onConnected(IPhotonPeerListener peer)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                lock (this)
                {
                    lblStatus.Text = "Connected";
                    btnJoin.Enabled = false;
                    txtName.ReadOnly = true;
                    txtChat.ReadOnly = false;
                    btnChat.Enabled = true;
                    client.OpCustom((byte)OperationCode.JOIN_CHAT, new Dictionary<byte, object> { { (byte)Data.DATA1, txtName.Text } }, true, null);
                }
            });
        }

        public void onDisConnected(IPhotonPeerListener peer)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                lock (this)
                {
                    lblStatus.Text = "Disconnected";
                    btnJoin.Enabled = true;
                    txtName.ReadOnly = false;
                    txtChat.ReadOnly = true;
                    btnChat.Enabled = false;
                }
            });
        }

        public void onMessage(object message)
        {
        }

        public void onOperationResponse(OperationResponse operationResponse)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                lock (this)
                {
                    Debug(String.Format("DEBUG-onOperationResponse: {0} {1}", operationResponse.OperationCode, operationResponse.Parameters));
                    switch ((OperationCode)operationResponse.OperationCode)
                    {
                        case OperationCode.JOIN_CHAT:
                            txtChatContent.Text += String.Format("{0} joined \n", operationResponse.Parameters[(byte)Data.DATA1]);
                            break;
                        case OperationCode.CHAT:
                            txtChatContent.Text += String.Format("{0}: {1}\n", operationResponse.Parameters[(byte)Data.DATA1], operationResponse.Parameters[(byte)Data.DATA2]);
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public void onEvent(EventData eventData)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                lock (this)
                {
                    Debug(String.Format("DEBUG-OnEvent: {0} {1}", eventData.Code, eventData.Parameters));
                    switch ((EventCode)eventData.Code)
                    {
                        case EventCode.ANOTHER_JOIN:
                            txtChatContent.Text += String.Format("{0} joined \n", eventData.Parameters[(byte)Data.DATA1]);
                            break;
                        case EventCode.CHAT:
                            txtChatContent.Text += String.Format("{0}: {1}\n", eventData.Parameters[(byte)Data.DATA1], eventData.Parameters[(byte)Data.DATA2]);
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public void Debug(string message)
        {
            if (isDebug)
            {
                txtChatContent.Text += String.Format("{0} \n", message);
            }
        }
    }
}

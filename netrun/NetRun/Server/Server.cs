using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetRun.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Lidgren.Network.Xna;
using Lidgren.Network;
using System.Threading;

namespace NetRun.Server
{
    class Server : BaseGameScreen
    {
        NetPeerConfiguration config;
        NetServer server;
        double nextSendUpdates, now;
        List<String> messages;
        bool localGame;
        Client.Client.GameType gameType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="configName"></param>
        /// <param name="maxConnections"></param>
        /// <param name="port"></param>
        public Server(Game game, SpriteBatch spriteBatch, string configName,int maxConnections,int port = 14242)
            : base(game, spriteBatch)
        {
             messages = new List<String>();
            //set up config for server based on user options
            //maxconncetiosn shoudlbe 1 on single player and more on multiplayer
            config = new NetPeerConfiguration(configName);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = port;
            config.MaximumConnections = maxConnections;
            //create and start server
            server = new NetServer(config);
            server.Start();

            //schedule initial sending of position updates
            nextSendUpdates = NetTime.Now;
        }

        public override void Update(GameTime gameTime)
        {
            NetIncomingMessage message;
            while ((message = server.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        //
                        //Server recieved a discovery request froma client; send a discovery response( with no extra data attached)
                        //
                        server.SendDiscoveryResponse(null, message.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //
                        //just print diagnostic message to console
                        //
                        Console.WriteLine(message.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            //
                            //a new player just connected!
                            //
                            Console.WriteLine(NetUtility.ToHexString(message.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                            messages.Add(NetUtility.ToHexString(message.SenderConnection.RemoteUniqueIdentifier) + " connected!");

                            //randomize his position and store in connectiont ag
                            message.SenderConnection.Tag = new int[] {
                                NetRandom.Instance.Next(10,100),
                                NetRandom.Instance.Next(10,100)
                            };
                        }
                        break;

                    case NetIncomingMessageType.Data:
                        //
                        //the client sent input to thte server
                        //
                        int xinput = message.ReadInt32();
                        int yinput = message.ReadInt32();

                        int[] pos = message.SenderConnection.Tag as int[];

                        //fancy movement logic goes here; we just append input to position
                        pos[0] += xinput;
                        pos[1] += yinput;
                        break;
                }//end switch

                //
                //set position updates 30 times per secodn
                //
                double now = NetTime.Now;
                if (now > nextSendUpdates)
                {
                    //yes, it's time to send position updates
                    //for each player
                    foreach (NetConnection player in server.Connections)
                    {
                        // ... send nformation about every other player (actually including self)
                        foreach (NetConnection otherPlayer in server.Connections)
                        {
                            //send position update about 'otherPlayer' to 'player'
                            NetOutgoingMessage outgoingMessage = server.CreateMessage();

                            //write who this position is for
                            outgoingMessage.Write(otherPlayer.RemoteUniqueIdentifier);
                            if (otherPlayer.Tag == null)
                                otherPlayer.Tag = new int[2];

                            int[] pos = otherPlayer.Tag as int[];
                            outgoingMessage.Write(pos[0]);
                            outgoingMessage.Write(pos[1]);

                            for(int i = 0; i < messages.Count; i++)
                            {
                                outgoingMessage.Write(messages[i]);
                            }
                            //send message
                            server.SendMessage(outgoingMessage, player, NetDeliveryMethod.Unreliable);
                        }//end foreach otherplayer in connections
                    }//end foreach player in connections

                    //schedule next update
                    nextSendUpdates += (1.0 / 30.0);
                }
            }//end while to loop through messages

            //sleep to allow other processes to runs moothly
            Thread.Sleep(1);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public void Shutdown(string disconnectMessage)
        {
            server.Shutdown(disconnectMessage);

        }
    }
}

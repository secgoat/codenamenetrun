using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetRun.Screens;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Lidgren.Network.Xna;
using Lidgren.Network;
using Microsoft.Xna.Framework.Input;
using System.Net;
using NetRun.TileEngine;

namespace NetRun.Client
{
    class Client : BaseGameScreen
    {
        SpriteFont font;
        Texture2D[] textures;
        Dictionary<long, Vector2> positions = new Dictionary<long, Vector2>();
        List<String> messages = new List<String>();
        NetClient client;
        NetPeerConfiguration config;

        string address;
        int port;

        public enum GameType
        {
            local,
            scanLan,
            hosted
        };
        GameType gameType;


        //variables for the tileengine
        TileMap levelMap = new TileMap();
        int squaresDown = 18;
        int squaresAcross = 11;

        public List<IPEndPoint> ServerEndpoints {get; private set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="configName"></param>
        public Client(Game game, SpriteBatch spriteBatch, string configName, GameType gameType = GameType.local, string address = "127.0.0.1", int port = 14242)
            : base(game, spriteBatch)
        {
            this.game = game;
            this.spriteBatch = spriteBatch;

            this.address = address;
            this.port = port;
            this.gameType = gameType;
            config = new NetPeerConfiguration(configName);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            client = new NetClient(config);
            client.Start();
        }


        public override void Initialize()
        {
            //gametype comes from join / host network game screens and tells the client how to look for the game
            switch (gameType)
            {
                case GameType.local:
                    client.DiscoverKnownPeer("127.0.0.1", 14242);
                    break;
                case GameType.scanLan:
                    client.DiscoverLocalPeers(this.port);
                    break;
                case GameType.hosted:
                    client.DiscoverKnownPeer(this.address, this.port);
                    break;
                default:
                    client.DiscoverLocalPeers(this.port);
                    break;
            }
            base.Initialize();
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Tile.TileSetTexture = game.Content.Load<Texture2D>(@"tilesets\part2_tileset");
            textures = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                textures[i] = game.Content.Load<Texture2D>("c" + (i + 1));
            font = game.Content.Load<SpriteFont>("font");
        }


        protected override void UnloadContent()
        {
        }


        public override void Update(GameTime gameTime)
        {
            //
            // Collect input
            //
            int xinput = 0;
            int yinput = 0;
            KeyboardState keyState = Keyboard.GetState();

            // exit game if escape or Back is pressed
            if (keyState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                
            }

            // use arrows or dpad to move avatar
            if (GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed || keyState.IsKeyDown(Keys.Left))
                xinput = -1;
            if (GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed || keyState.IsKeyDown(Keys.Right))
                xinput = 1;
            if (GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed || keyState.IsKeyDown(Keys.Up))
                yinput = -1;
            if (GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed || keyState.IsKeyDown(Keys.Down))
                yinput = 1;

            if (xinput != 0 || yinput != 0)
            {
                //
                // If there's input; send it to server
                //
                NetOutgoingMessage om = client.CreateMessage();
                om.Write(xinput); // very inefficient to send a full Int32 (4 bytes) but we'll use this for simplicity
                om.Write(yinput);
                client.SendMessage(om, NetDeliveryMethod.Unreliable);
                
            }

            // read messages
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        //TODO: connect to first discovered server if single player, other wise display list of responses and let player choose. This could be done by showing a list of all LAN addresses available then allowing the player to choose and connect, but send the connection request as DiscoverKnownPeer. Will also need to  ahve a bool to tell the client if the user has asked to connect to a server or not.
                        client.Connect(msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.Data:
                        // server sent a position update
                        long who = msg.ReadInt64();
                        int x = msg.ReadInt32();
                        int y = msg.ReadInt32();
                       
                        positions[who] = new Vector2(x, y);
                        CalculateCameraPos(x, y);
                        break;
                }
            }
            base.Update(gameTime);
        }

        void CalculateCameraPos(int playerX, int playerY)
        {
            //just for reminder Mathclamp keeps the first number between min (second number) and max (third number)
            //this keeps the camera from goign off the map and keeps it centered on the player wehn they arent right on the edge.
            Camera.Location.X = MathHelper.Clamp(playerX, 0, (levelMap.MapWidth - squaresAcross) * Tile.TileWidth);
            Camera.Location.Y = MathHelper.Clamp(playerY, 0, (levelMap.MapHeight - squaresDown) * Tile.TileHeight);
        
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

            //draw tileMap
            Vector2 firstSquare = new Vector2(Camera.Location.X / Tile.TileWidth, Camera.Location.Y / Tile.TileHeight); 
            int firstX = (int)firstSquare.X;
            int firstY = (int)firstSquare.Y;

            Vector2 squareOffset = new Vector2(Camera.Location.X % Tile.TileWidth, Camera.Location.Y % Tile.TileHeight);
            int offsetX = (int)squareOffset.X;
            int offsetY = (int)squareOffset.Y;

            for (int y = 0; y < squaresDown; y++)
            {
                for (int x = 0; x < squaresDown; x++)
                {
                    foreach (int tileID in levelMap.Rows[y + firstY].Columns[x + firstX].BaseTiles)
                    {
                        spriteBatch.Draw(
                            Tile.TileSetTexture,
                            new Rectangle((x * Tile.TileWidth) - offsetX, (y * Tile.TileHeight) - offsetY, Tile.TileWidth, Tile.TileHeight),
                            Tile.GetSourceRectangle(tileID),
                            Color.Wheat);
                    }
                }
            }
                // draw all players
                foreach (var kvp in positions)
                {
                    // use player unique identifier to choose an image
                    int num = Math.Abs((int)kvp.Key) % textures.Length;

                    // draw player
                    spriteBatch.Draw(textures[num], kvp.Value, Color.White);
                    if (messages.Count > 0)
                        spriteBatch.DrawString(font, messages[0], Vector2.One, Color.Black);
                }

            //spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Shutdown(string disconnectMessage)
        {
            client.Shutdown(disconnectMessage);

        }

       
    }
}


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
using TileEngine;

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
        int squaresDown = 37;
        int squaresAcross = 17;

        int baseOffsetX = 32;
        int baseOffsetY = -64;
        float heightRowDepthMod = 0.0000001f;

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
            Tile.TileSetTexture = game.Content.Load<Texture2D>(@"tilesets\part4_tileset");
            textures = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                textures[i] = game.Content.Load<Texture2D>("c" + (i + 1));
            font = game.Content.Load<SpriteFont>("tinyFont");
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
            GraphicsDevice.Clear(Color.Black);

            Vector2 firstSquare = new Vector2(Camera.Location.X / Tile.TileStepX, Camera.Location.Y / Tile.TileStepY);
            int firstX = (int)firstSquare.X;
            int firstY = (int)firstSquare.Y;

            Vector2 squareOffset = new Vector2(Camera.Location.X % Tile.TileStepX, Camera.Location.Y % Tile.TileStepY);
            int offsetX = (int)squareOffset.X;
            int offsetY = (int)squareOffset.Y;

            float maxdepth = ((levelMap.MapWidth + 1) * ((levelMap.MapHeight + 1) * Tile.TileWidth)) / 10;
            float depthOffset;

            for (int y = 0; y < squaresDown; y++)
            {
                int rowOffset = 0;
                if ((firstY + y) % 2 == 1)
                    rowOffset = Tile.OddRowXOffset;

                for (int x = 0; x < squaresAcross; x++)
                {
                    int mapx = (firstX + x);
                    int mapy = (firstY + y);
                    depthOffset = 0.7f - ((mapx + (mapy * Tile.TileWidth)) / maxdepth);

                    foreach (int tileID in levelMap.Rows[mapy].Columns[mapx].BaseTiles)
                    {
                        spriteBatch.Draw(

                            Tile.TileSetTexture,
                            new Rectangle(
                                (x * Tile.TileStepX) - offsetX + rowOffset + baseOffsetX,
                                (y * Tile.TileStepY) - offsetY + baseOffsetY,
                                Tile.TileWidth, Tile.TileHeight),
                            Tile.GetSourceRectangle(tileID),
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            1.0f);
                    }

                    int heightRow = 0;

                    foreach (int tileID in levelMap.Rows[mapy].Columns[mapx].HeightTiles)
                    {
                        spriteBatch.Draw(
                            Tile.TileSetTexture,
                            new Rectangle(
                                (x * Tile.TileStepX) - offsetX + rowOffset + baseOffsetX,
                                (y * Tile.TileStepY) - offsetY + baseOffsetY - (heightRow * Tile.HeightTileOffset),
                                Tile.TileWidth, Tile.TileHeight),
                            Tile.GetSourceRectangle(tileID),
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            depthOffset - ((float)heightRow * heightRowDepthMod));
                        heightRow++;
                    }

                    foreach (int tileID in levelMap.Rows[y + firstY].Columns[x + firstX].TopperTiles)
                    {
                        spriteBatch.Draw(
                            Tile.TileSetTexture,
                            new Rectangle(
                                (x * Tile.TileStepX) - offsetX + rowOffset + baseOffsetX,
                                (y * Tile.TileStepY) - offsetY + baseOffsetY - (heightRow * Tile.HeightTileOffset),
                                Tile.TileWidth, Tile.TileHeight),
                            Tile.GetSourceRectangle(tileID),
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            depthOffset - ((float)heightRow * heightRowDepthMod));
                    }

                    /*spriteBatch.DrawString(font, (x + firstX).ToString() + ", " + (y + firstY).ToString(),
                        new Vector2((x * Tile.TileStepX) - offsetX + rowOffset + baseOffsetX + 24,
                            (y * Tile.TileStepY) - offsetY + baseOffsetY + 48), Color.White, 0f, Vector2.Zero,
                            1.0f, SpriteEffects.None, 0.0f); */
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
                        spriteBatch.DrawString(font, messages[0], Vector2.One, Color.Black, 0.0f, Vector2.Zero, new Vector2(1,1), SpriteEffects.None, 1.0f);
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


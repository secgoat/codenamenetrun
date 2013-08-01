using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NetRun.Screens;
using NetRun.Server;
using NetRun.Client;

using FuchsGUI;
using Lidgren.Network;
using System.Threading;

namespace NetRun
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont, formFont;

        KeyboardState keyboardState;
        KeyboardState oldKeyboardState;
        //add the different screens you want here 
        StartScreen startScreen; //basic start menu / first screen they see
        NetworkGameSelectScreen networkScreen; //the screen / menu to choose to join or host a network game
        JoinNetworkGameScreen joinGameScreen;
        HostNetworkGameScreen hostGameScreen;
        ActionScreen actionScreen; // right now this is the "Game" screen //TODO: change this to the Client
        BaseGameScreen activeScreen; //just a way to keep track of which screen is currently active
        PopUpScreen popUpScreen; // this is right now just a screen that says ar eyou sure you want to quit


        //textures for menu backgrounds etc.
        Texture2D popUpTexture;
        Texture2D actionScreentexture;
        Texture2D blankBlackTexture;

        //Network pieces
        Client.Client client;
        Client.Client.GameType gameType; //use this enum to tell client if local, join lan or hosted game

        Server.Server server;
        string gameConfigName;


        //screensize to pass in for menus for positioning
        Rectangle screenSize;
        public Rectangle ScreenSize { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            gameConfigName = "GameServer";
            
        }


        protected override void Initialize()
        {
            ScreenSize = this.GraphicsDevice.Viewport.Bounds;
            base.Initialize();
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("font");
            formFont = Content.Load<SpriteFont>("FormFont");

            popUpTexture = Content.Load<Texture2D>("quitscreen");
            actionScreentexture = Content.Load<Texture2D>("greenmetal");
            blankBlackTexture = Content.Load<Texture2D>("black");

            //initialize and start the screens
            startScreen = new StartScreen(this, spriteBatch, formFont);
            Components.Add(startScreen);
            startScreen.ButtonClicked += new StartScreen.ClickEvent(HandleStartScreenButtons);
            startScreen.Hide();

            networkScreen = new NetworkGameSelectScreen(this, spriteBatch, formFont, blankBlackTexture);
            Components.Add(networkScreen);
            networkScreen.ButtonClicked += new NetworkGameSelectScreen.ClickEvent(HandleNetworkSelectScreenButtons);
            networkScreen.Hide();

            joinGameScreen = new JoinNetworkGameScreen(this, spriteBatch, formFont, blankBlackTexture);
            Components.Add(joinGameScreen);
            joinGameScreen.ButtonClicked += new JoinNetworkGameScreen.ClickEvent(HandleJoinGameScreenButtons);
            joinGameScreen.Hide();

            hostGameScreen = new HostNetworkGameScreen(this, spriteBatch, formFont, blankBlackTexture);
            Components.Add(hostGameScreen);
            hostGameScreen.ButtonClicked += new HostNetworkGameScreen.ClickEvent(HandleHostGameScreenButtons);
            hostGameScreen.Hide();

            actionScreen = new ActionScreen(this, spriteBatch, actionScreentexture);
            Components.Add(actionScreen);
            actionScreen.Hide();

            popUpScreen = new PopUpScreen(this, spriteBatch, spriteFont, popUpTexture);
            Components.Add(popUpScreen);
            popUpScreen.Hide();



            activeScreen = startScreen;
            //activeScreen = joinGameScreen;
            activeScreen.Show();

            IsMouseVisible = true;


        }


        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            if (activeScreen == startScreen)
            {
                // HandleStartScreen();
                //using button events instead
            }

            else if (activeScreen == networkScreen)
            {
                //HandleNetworkSelectScreenButtons();
            }

            else if (activeScreen == joinGameScreen)
            {
                //not sure i need this as I am using delegates instead?
                //HandleJoinGameScreen();
            }

            else if (activeScreen == actionScreen)
            {
                HandleActionScreen();
            }
            else if (activeScreen == popUpScreen)
            {
                HandlePopUpScreen();
            }

            oldKeyboardState = keyboardState;
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend); // draw sprites form back (1.0f) to front (0.0f)
            base.Draw(gameTime);
            spriteBatch.End();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (client != null)
                client.Shutdown("bye");

            if (server != null)
                server.Shutdown("Bye");
            base.OnExiting(sender, args);
        }

        void StartServer(Client.Client.GameType gameType)
        {
            int maxConnections = 0;
            if (gameType == Client.Client.GameType.local)
                maxConnections = 1;
            else
                maxConnections = hostGameScreen.MaxConnections;

            server = new Server.Server(this, spriteBatch, gameConfigName, maxConnections);
            Components.Add(server);
            //server.Hide();
            StartClient();
        }

        void StartClient()
        {
            client = new Client.Client(this, spriteBatch, gameConfigName, this.gameType);
            Components.Add(client);
            activeScreen.Hide();
            activeScreen = client;
            activeScreen.Show();
        }

        private bool CheckKey(Keys key)
        {
            return keyboardState.IsKeyUp(key) && oldKeyboardState.IsKeyDown(key);
        }

        private void HandleStartScreenButtons(Control sender)
        {
            if (sender.Name == "StartGame")
            {
                gameType = Client.Client.GameType.local;
                StartServer(gameType);
            }
            if (sender.Name == "NetworkGame")
            {
                activeScreen.Hide();
                activeScreen = networkScreen;
                activeScreen.Show();
            }
            if (sender.Name == "QuitGame")
            {
                this.Exit();
            }
        }

        private void HandleNetworkSelectScreenButtons(Control sender)
        {
            if (sender.Name == "HostGame")
            {
                gameType = Client.Client.GameType.hosted;
                activeScreen.Hide();
                activeScreen = hostGameScreen;
                activeScreen.Show();
            }
            if (sender.Name == "JoinGame")
            {
                //load join screen
                activeScreen.Hide();
                activeScreen = joinGameScreen;
                activeScreen.Show();
            }
            if (sender.Name == "BackButton")
            {
                //go back to startScreen
                activeScreen.Hide();
                activeScreen = startScreen;
                activeScreen.Show();
            }
        }

        private void HandleJoinGameScreenButtons(Control sender)
        {
            if (sender.Name == "ScanLan")
            {
                gameType = Client.Client.GameType.scanLan;
                StartClient();
                Console.WriteLine("SCAN LAN!");
            }
            if (sender.Name == "Connect") //Join a network game by address
            {
                gameType = Client.Client.GameType.hosted;
                StartClient();
                Console.WriteLine("Connect to {0}:{1}", joinGameScreen.Address, joinGameScreen.Port);
            }
            if (sender.Name == "BackButton")
            {
                activeScreen.Hide();
                activeScreen = networkScreen;
                activeScreen.Show();
            }
        }

        private void HandleHostGameScreenButtons(Control sender)
        {
            if (sender.Name == "Start")
            {
                gameType = Client.Client.GameType.hosted;
                StartServer(gameType);
            }
            if (sender.Name == "BackButton")
            {
                activeScreen.Hide();
                activeScreen = networkScreen;
                activeScreen.Show();
            }

        }

        private void HandleActionScreen()
        {
            if (CheckKey(Keys.F1))
            {
                //activeScreen.Hide();
                activeScreen.Enabled = false;
                activeScreen = popUpScreen;
                activeScreen.Show();
            }
        }

        private void HandlePopUpScreen()
        {
            if (CheckKey(Keys.Enter))
            {
                if (popUpScreen.SelectedIndex == 0)
                {
                    activeScreen.Hide();
                    actionScreen.Hide();
                    activeScreen = startScreen;
                    activeScreen.Show();
                }
                if (popUpScreen.SelectedIndex == 1)
                {
                    activeScreen.Hide();
                    activeScreen = actionScreen;
                    activeScreen.Show();
                }
            }
        }

    }
}

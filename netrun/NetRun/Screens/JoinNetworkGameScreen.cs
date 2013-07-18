using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FuchsGUI;
using Microsoft.Xna.Framework.Input;

using Lidgren.Network;

namespace NetRun.Screens
{
    class JoinNetworkGameScreen : BaseGameScreen
    {
        Texture2D buttonTexture, textboxTexture, background, formBackground; //for gui components

        //Fuchs GUI components
        Form connectionMethodForm;
        Button connectButton, lanButton, backButton;
        List<System.Net.IPEndPoint> lanGames; //use this to dynamically create buttons with server ip address inorder to connect to a game
        
        TextBox textBoxIP, textBoxPort;
        Label adressLabel, portLabel;

        Rectangle imageRectangle;


        //delegates to send events back to the main game1 window
        public delegate void ClickEvent(Control sender);
        public event ClickEvent ButtonClicked;

        public string Address { get; set; }
        public string Port { get; set; }
        

        public JoinNetworkGameScreen(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D background)
            : base(game, spriteBatch)
        {
            this.background = background;
            formBackground = game.Content.Load<Texture2D>("alienmetal");
            buttonTexture = game.Content.Load<Texture2D>("buttonTexture");
            textboxTexture = game.Content.Load<Texture2D>("textboxTexture");

            Rectangle formLocation = CenterGUIForm(350, 350);
            connectionMethodForm = new Form("Connect", "Connection Metod", formLocation, formBackground, spriteFont, Color.White);
   
            //figure out the width and heigh of the text on the buttons
            Vector2 lanButtonSize, connectButtonSize;
            lanButtonSize = spriteFont.MeasureString("Scan Lan");
            connectButtonSize = spriteFont.MeasureString("Connect");

            lanButton = new Button("ScanLan", "Scan LAN", new Rectangle(11,19,(int)lanButtonSize.X + 10, (int)lanButtonSize.Y + 10), buttonTexture, spriteFont, Color.White);
            connectionMethodForm.AddControl(lanButton);

            connectButton = new Button("Connect", "Connect", new Rectangle(11,99,(int)connectButtonSize.X + 10, (int)connectButtonSize.Y + 10), buttonTexture, spriteFont, Color.White);
            connectionMethodForm.AddControl(connectButton);

            backButton = new Button("BackButton", @"Back", new Rectangle(141, 99, 95, 23), buttonTexture, spriteFont, Color.White);
            connectionMethodForm.AddControl(backButton);

            textBoxIP = new TextBox("Address", "", 100, new Rectangle(11, 73, 127, 20), textboxTexture, spriteFont, Color.White);
            connectionMethodForm.AddControl(textBoxIP);

            adressLabel = new Label("adressLabel", @"Address", new Vector2(8, 57), spriteFont, new Color(0, 0, 0), 0, 0);
            connectionMethodForm.AddControl(adressLabel);

            textBoxPort = new TextBox("Port", "", 8, new Rectangle(168, 73, 58, 20), textboxTexture, spriteFont, Color.White);
            connectionMethodForm.AddControl(textBoxPort);

            portLabel = new Label("portLabel", @"Port", new Vector2(165, 57), spriteFont, new Color(0, 0, 0), 0, 0);
            connectionMethodForm.AddControl(portLabel);

            lanButton.onClick += new EHandler(ButtonClick);
            connectButton.onClick += new EHandler(ButtonClick);
            backButton.onClick += new EHandler(ButtonClick);
            
            imageRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            connectionMethodForm.Update(mouseState, keyboardState);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(background, imageRectangle, Color.White);
            connectionMethodForm.Draw(spriteBatch);
            //lanButton.Draw(spriteBatch);
            //sendButton.Draw(spriteBatch);
            //textBoxIP.Draw(spriteBatch);
            //textBoxPort.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        void ButtonClick(Control sender)
        {
            if (ButtonClicked != null)
            {
                this.Address = textBoxIP.Text;
                this.Port = textBoxPort.Text;
                this.ButtonClicked(sender);
            }
        }
    }
}

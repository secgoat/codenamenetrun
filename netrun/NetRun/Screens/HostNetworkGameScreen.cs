using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FuchsGUI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace NetRun.Screens
{
    class HostNetworkGameScreen : BaseGameScreen
    {

        Texture2D buttonTexture, textboxTexture, background, formBackground; //for gui components

        //Fuchs GUI components
        Form hostGameForm;
        Button startButton, backButton;
        Label maxConnectionsLabel, portLabel;
        TextBox textBoxMaxConnections, textBoxPort;

        Rectangle imageRectangle;


        //delegates to send events back to the main game1 window
        public delegate void ClickEvent(Control sender);
        public event ClickEvent ButtonClicked;

        public int MaxConnections { get; set; }
        public int Port { get; set; }
        

        public HostNetworkGameScreen(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D background)
            : base(game, spriteBatch)
        {
            
            this.background = background;
            formBackground = game.Content.Load<Texture2D>("alienmetal");
            buttonTexture = game.Content.Load<Texture2D>("buttonTexture");
            textboxTexture = game.Content.Load<Texture2D>("textboxTexture");

            //center the form ont he screen
            Rectangle center = this.CenterGUIForm(242, 224);
            hostGameForm = new Form("Host", "Host a Game", new Rectangle(center.X, center.Y, center.Width, center.Height), formBackground, spriteFont, Color.White);

            //figure out the width and heigh of the text on the buttons
            Vector2 startButtonSize, backButtonSize;
            startButtonSize = spriteFont.MeasureString("Start Game");
            backButtonSize = spriteFont.MeasureString("Back");

            startButton = new Button("Start", "Start Game", new Rectangle(6, 186, (int)startButtonSize.X + 10, (int)startButtonSize.Y + 10), buttonTexture, spriteFont, Color.White);
            backButton = new Button("BackButton", @"Back", new Rectangle(132, 186,  (int)startButtonSize.X + 10, (int)startButtonSize.Y + 10), buttonTexture, spriteFont, Color.White);
            
            textBoxMaxConnections = new TextBox("MaxConnections", "10", 100, new Rectangle(11, 42, 60, 20), textboxTexture, spriteFont, Color.Black);
            maxConnectionsLabel = new Label("maxConnectionsLabel", @"Max Connections", new Vector2(8, 26), spriteFont, Color.White, 0, 0);
           
            textBoxPort = new TextBox("Port", "14242", 8, new Rectangle(11, 116, 60, 20), textboxTexture, spriteFont, Color.Black);
            portLabel = new Label("portLabel", @"Port", new Vector2(8, 91), spriteFont, Color.White, 0, 0);

            hostGameForm.AddControl(startButton);
            hostGameForm.AddControl(backButton);
            hostGameForm.AddControl(textBoxMaxConnections);
            hostGameForm.AddControl(maxConnectionsLabel);
            hostGameForm.AddControl(textBoxPort);
            hostGameForm.AddControl(portLabel);

            startButton.onClick += new EHandler(ButtonClick);
            backButton.onClick += new EHandler(ButtonClick);

            imageRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        

        }

       

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            hostGameForm.Update(mouseState, keyboardState);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(background, imageRectangle, Color.White);
            hostGameForm.Draw(spriteBatch);
            base.Draw(gameTime);
        }

        void ButtonClick(Control sender)
        {
            if (ButtonClicked != null)
            {
                this.MaxConnections = Int32.Parse(textBoxMaxConnections.Text);
                this.Port = Int32.Parse(textBoxPort.Text);
                this.ButtonClicked(sender);
            }
        }

    }
}

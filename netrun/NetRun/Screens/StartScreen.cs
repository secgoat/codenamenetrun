using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FuchsGUI;
using Microsoft.Xna.Framework.Input;

namespace NetRun.Screens
{
    public class StartScreen : BaseGameScreen
    {
      
        //delegates to control button presses
        public delegate void ClickEvent(Control sender);
        public event ClickEvent ButtonClicked;

        //main form components
        public Form mainMenuForm;
        public Button quitGameButton, networkGameButton, startGameButton;
        Texture2D formBackground;
        Texture2D buttonTexture;



        public StartScreen(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont)
            : base(game, spriteBatch)
        {

            Rectangle formLocation = CenterGUIForm(150, 200);
            formBackground = null;
            //formBackground = game.Content.Load<Texture2D>("alienmetal");
            buttonTexture = game.Content.Load<Texture2D>("buttonTexture");
            
            mainMenuForm = new Form("MainMenu", "Main Menu", formLocation, formBackground, spriteFont, Color.White);
            quitGameButton = new Button("QuitGame", @"Quit Game", new Rectangle(27, 132, 95, 23), buttonTexture, spriteFont, Color.White);
            networkGameButton = new Button("NetworkGame", @"Network Game",new Rectangle(27, 90, 95, 23), buttonTexture, spriteFont, Color.White);
            startGameButton = new Button("StartGame", @"Start Game", new Rectangle(27, 42, 95, 23), buttonTexture, spriteFont, Color.White);

            mainMenuForm.AddControl(startGameButton);
            mainMenuForm.AddControl(networkGameButton);
            mainMenuForm.AddControl(quitGameButton);

            startGameButton.onClick += new EHandler(ButtonClick);
            networkGameButton.onClick += new EHandler(ButtonClick);
            quitGameButton.onClick += new EHandler(ButtonClick);

        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyoardState = Keyboard.GetState();
            mainMenuForm.Update(mouseState, keyoardState);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //spriteBatch.Draw(image, imageRectangle, Color.White);
            mainMenuForm.Draw(this.spriteBatch);
            base.Draw(gameTime);
        }

        void ButtonClick(Control sender)
        {
            if (ButtonClicked != null)
                this.ButtonClicked(sender);
            //what do here?
        }
    }
}

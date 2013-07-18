using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetRun.Screens
{
    class PopUpScreen : BaseGameScreen
    {
        MenuComponent menuComponent;
        Texture2D image;
        Rectangle imageRectangle;
        SpriteFont spriteFont;

        string popUpMessage;
        Vector2 messageSize;
        Vector2 messagePosition;

        public int SelectedIndex
        {
            get { return menuComponent.SelectedIndex; }
            set { menuComponent.SelectedIndex = value; }
        }

        public PopUpScreen(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D image)
            : base(game, spriteBatch)
        {
            this.spriteFont = spriteFont;
            string[] menuItems = { "Yes", "No" };
            popUpMessage = "Are You sure you want to quit?";
            messageSize = spriteFont.MeasureString(popUpMessage);

            menuComponent = new MenuComponent(game, spriteBatch, spriteFont, menuItems);
            Components.Add(menuComponent);
            this.image = image;
            imageRectangle = new Rectangle((Game.Window.ClientBounds.Width - this.image.Width) / 2, 
                (Game.Window.ClientBounds.Height - this.image.Height) / 2, this.image.Width, this.image.Height);

            menuComponent.Position = new Vector2((imageRectangle.Width - menuComponent.Width), 
                imageRectangle.Bottom - menuComponent.Height - 50);
            
            messagePosition = new Vector2((imageRectangle.X + (imageRectangle.Width - messageSize.X) /2), imageRectangle.Y + 10 ); //wow this one was fun, first take the X of the popup menu then figure out how many pixels on one side of message and add those two togethe to get them cenetered
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //spriteBatch.DrawString(spriteFont, "Are You sure You want to quit?", Vector2.Zero, Color.White);
            spriteBatch.Draw(image, imageRectangle, Color.White);
            spriteBatch.DrawString(spriteFont, popUpMessage, messagePosition, Color.White);
            
            base.Draw(gameTime);
        }
    }
}

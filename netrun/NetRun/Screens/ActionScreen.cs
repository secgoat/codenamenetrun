﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace NetRun.Screens
{
    class ActionScreen : BaseGameScreen
    {
        KeyboardState keyboardState;
        Texture2D image;
        Rectangle imageRectangle;
        public ActionScreen(Game game, SpriteBatch spriteBatch, Texture2D image)
        : base(game, spriteBatch)
        {
            this.image = image; 
            imageRectangle = new Rectangle(0, 0,
                Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                game.Exit();
        }
        public override void Draw(GameTime gameTime)
        {
           // spriteBatch.Draw(image, imageRectangle, Color.White);
            
            base.Draw(gameTime);
        }
    }
}
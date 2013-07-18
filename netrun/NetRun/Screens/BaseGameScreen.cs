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


namespace NetRun.Screens
{

    public abstract class BaseGameScreen : Microsoft.Xna.Framework.DrawableGameComponent
    {
       List<GameComponent> components = new List<GameComponent>(); //used to hold any children of gamescreen instance
        protected Game game; //use this to keep track of the over all game, when state is passed to this instance of screen we need to knwo where to go back to
        //protected SpriteBatch spriteBatch; //obvious, since this is a drawable game comopnent we need to have a sprite batch to draw with.
        protected SpriteBatch spriteBatch;
        protected Rectangle windowSize;
        public List<GameComponent> Components { get { return components; } }

        public BaseGameScreen(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.game = game; //set local game variable to initial game
            this.windowSize = game.GraphicsDevice.Viewport.Bounds;
            this.spriteBatch = spriteBatch; // also synch local spritebacth to initial game spritebatch
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (GameComponent component in components)
            {
                if (component.Enabled == true)
                    component.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            foreach(GameComponent component in components)
            {
                if(component is DrawableGameComponent && ((DrawableGameComponent) component).Visible)
                    ((DrawableGameComponent) component).Draw(gameTime);
            }
        }

        public virtual void Show()
        {
            this.Visible = true;
            this.Enabled = true;
            foreach (GameComponent component in components)
            {
                component.Enabled = true;
                if (component is DrawableGameComponent)
                    ((DrawableGameComponent)component).Visible = true;
            }
        }

        public virtual void Hide()
        {
            this.Visible = false;
            this.Enabled = false;
            foreach (GameComponent component in components)
            {
                component.Enabled = false;
                if (component is DrawableGameComponent)
                    ((DrawableGameComponent)component).Visible = false;
            }
        }

        /// <summary>
        /// Pause the screen but leave it visible (enabled = false)
        /// </summary>
        public virtual void Pause()
        {
            this.Enabled = false;
        }

        /// <summary>
        /// unpause the screen (enabled = true)
        /// </summary>
        public virtual void UnPause()
        {
            this.Enabled = true;
        }

     
        /// <summary>
        /// plug in the FuchsGUI form width and height and it will center the menu on the screen.
        /// Mostly helpful for the main menu style screens
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public virtual Rectangle CenterGUIForm(int width, int height)
        {
            Rectangle formLocation;
            Vector2 center = new Vector2((this.windowSize.Width - width) / 2, (this.windowSize.Height - height) / 2);
            return new Rectangle((int)center.X, (int)center.Y, width, height);
        }


    }
}


using System;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BattleEngine
{
	public class BattleMenu : DrawableGameComponent
	{
		static SpriteFont font;
		
		SpriteBatch spriteBatch;
		
		String[] menuItems;
		
		Vector2 location;
		
		int currentIndex;
		
		
		public BattleMenu (Game game, String[] menuItems, Vector2 location) 
			: base(game)
		{
			this.menuItems = menuItems;
			this.location = location;
			currentIndex = 0;
		}
		
		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			if (font == null) {
				font = Game.Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>( "Courier New" );
			}
			base.LoadContent ();
		}
		
		/// <summary>
		/// Increases the index of the selected menu item.
		/// </summary>
		public void selectNext() {
			currentIndex = ++currentIndex % menuItems.Length;
		}
		
		public void selectPrevious() {
			currentIndex = (currentIndex + menuItems.Length - 1) % menuItems.Length;
		}
		
		public int getSelected() {
			return currentIndex;
		}
		
		public override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin ();
			
			Vector2 menuPosition = location;
			Color textColor;
			for (int i = 0; i < menuItems.Length; i++) {
				if (i == currentIndex) {
					textColor = Color.Yellow;
				} else {
					textColor = Color.White;
				}
				spriteBatch.DrawString (
					font, menuItems[i], menuPosition, 
                    textColor);
				menuPosition.X += 10 + font.MeasureString (menuItems[i]).X;
			}
			spriteBatch.End();
		}
	}
}


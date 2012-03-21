using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BattleEngine
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		
		SpriteBatch spriteBatch;
		
		RenderTarget2D renderTarget;
		
		Texture2D unit;
		
		Texture2D cursor;
		
		Texture2D background;
		
		Vector2 cursorPosition;
		
		static int enemyCount = 3;
		static int playerCount = 3;
		
		Vector2[] enemyPositions = new Vector2[enemyCount];
		
		Vector2[] playerPositions = new Vector2[playerCount];
		
		int selectedUnit = 0;
		
		enum MenuStates {UnitSelect, ActionSelect, PositionSelect};
		
		MenuStates menuState = MenuStates.UnitSelect;
		
		KeyboardState oldState;
		
		public Game1 ()
		{
			graphics = new GraphicsDeviceManager(this);
			
			// 16:9 Aspect ratio
			float width = 854, height = 480;
			
			graphics.PreferredBackBufferWidth = (int)width;
			graphics.PreferredBackBufferHeight = (int)height;
			
			Vector2 unitPosition = new Vector2(4f/16f*width, 3f/9f*height);
			
			// Initialize enemy positions
			for (int i = 0; i < 3; i++ ) {
				enemyPositions[i] = new Vector2(unitPosition.X, unitPosition.Y);
				unitPosition.Y += height/9f;
			}
			// Initialize player positions
			unitPosition.X = 12f/16f*width;
			unitPosition.Y = 3f/9f*height;
			for (int i = 0; i < 3; i++ ) {
				playerPositions[i] = new Vector2(unitPosition.X, unitPosition.Y);
				unitPosition.Y += height/9f;
			}
			graphics.IsFullScreen = false; // for now.
			
			Content.RootDirectory = "Content";
		}
		
		protected override void Initialize ()
		{
			base.Initialize();	
			oldState = Keyboard.GetState ();
		}
		
		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			renderTarget = new RenderTarget2D(
				GraphicsDevice,
				GraphicsDevice.PresentationParameters.BackBufferWidth,
				GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None
			);
			
			unit = Content.Load<Texture2D> ("unit.png");
			cursor = Content.Load<Texture2D> ("cursor.png");
			background = Content.Load<Texture2D> ("background.png");
		}
		
		protected override void UnloadContent ()
		{
			if (renderTarget != null) {
				try {
					renderTarget.Dispose();
					renderTarget = null;
				} catch {
				}
			}
		}
		
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit ();
			KeyboardState newState = Keyboard.GetState ();
			UpdateMenu(newState, gameTime);
			oldState = newState;
			base.Update (gameTime);
		}
		
		private bool IsPressed(KeyboardState newState, Keys key)
		{
			if (newState.IsKeyDown(key) && !oldState.IsKeyDown(key)) {
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Updates the menu state. Navigate units, options using arrow keys.
		/// Select an option with space or enter. 
		/// </summary>
		/// <param name='gameTime'>
		/// Game time.
		/// </param>
		protected void UpdateMenu(KeyboardState newState, GameTime gameTime)
		{
			switch (menuState) {
			case MenuStates.ActionSelect:
				break;
			case MenuStates.PositionSelect:
				break;
			case MenuStates.UnitSelect:
			default:
				if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedUnit = (selectedUnit + playerCount -1) % playerCount;
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedUnit = ++selectedUnit % playerCount;
				}
				break;
			}
			
		}
		
		protected override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin();
			// Draw background of Battle area
			spriteBatch.Draw (background,
			                  Vector2.Zero,
			                  Color.White);
			
			// Draw Cursor
			cursorPosition = playerPositions[selectedUnit];
			spriteBatch.Draw (cursor, cursorPosition, Color.White);
			
			// Draw Enemy Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, enemyPositions[i], Color.White);
			}
			// Draw Player Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, playerPositions[i], Color.White);
			}
			
			spriteBatch.End ();
			base.Draw (gameTime);
		}
		
	}					
}


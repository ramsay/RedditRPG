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
		
		public static SpriteFont CourierNew;
		
		Vector2 cursorPosition;
		
		static int enemyCount = 3;
		static int playerCount = 3;
		
		Vector2[] enemyPositions = new Vector2[enemyCount];
		
		Vector2[] playerPositions = new Vector2[playerCount];
		
			
		int selectedUnit = 0;
		int selectedTarget = 0;
		enum MenuStates {UnitSelect, ActionSelect, PositionSelect, 
			TargetSelect, RangeSelect, CoordSelect};
		Stack<MenuStates> menuState;
		
		KeyboardState oldState;
		
		BattleMenu[] menus = new BattleMenu[6];
		
		enum ActionState {Attack, Position};
		enum PositionState {Charge, Stay, KeepDistance, RunAway};
		
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
			
			Vector2 menuPosition = new Vector2(
				1f/16f*width,
				8f/9f*height);
			BattleMenu actionsMenu = new BattleMenu(this, new[] {"Attack", "Position", "Done"}, menuPosition);
			BattleMenu positionMenu = new BattleMenu(this, new[] {"Charge", "Stay", "Keep Distance", "Run Away", "Done"}, menuPosition);
			BattleMenu rangeMenu = new BattleMenu(this, new[] {"1m", "2m", "3m", "5m", "8m", "13m", "Done"}, menuPosition);
			
			menus[(int)MenuStates.ActionSelect] = actionsMenu;
			menus[(int)MenuStates.PositionSelect] = positionMenu;
			menus[(int)MenuStates.RangeSelect] = rangeMenu;
			
			foreach (BattleMenu menu in menus) {
				if (menu != null) {
					menu.Visible = false;
					Components.Add (menu);
				}
			}
			menuState = new Stack<MenuStates>();
			menuState.Push (MenuStates.UnitSelect);
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
		
		private void PushMenuState(MenuStates state) {
			// Initializers
			if (state == MenuStates.CoordSelect) {
				cursorPosition = playerPositions[selectedUnit];
			}
			
			// Deactivate current Menu
			if (menus[(int)menuState.Peek ()] != null) {
				menus[(int)menuState.Peek ()].Visible = false;
			}
			// Activate new Menu
			if (menus[(int)state] != null) {
				menus[(int)state].Visible = true;
			}
			//Store state
			menuState.Push (state);
		}
		
		private MenuStates PopMenuState() {
			// Deactivate current Menu
			if (menus[(int)menuState.Peek ()] != null) {
				menus[(int)menuState.Peek ()].Visible = false;
			}
			
			// Restore state
			MenuStates oldState = menuState.Pop();
			
			// Activate new Menu
			if (menus[(int)menuState.Peek()] != null) {
				menus[(int)menuState.Peek()].Visible = true;
			}
			return oldState;
			
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
			switch (menuState.Peek ()) {
			case MenuStates.CoordSelect:
				if (IsPressed(newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					PopMenuState();
				} else {
					if (newState.IsKeyDown(Keys.Up)) {
						cursorPosition.Y -= 1;
					}
					if (newState.IsKeyDown(Keys.Down)) {
						cursorPosition.Y += 1;
					}
					if (newState.IsKeyDown (Keys.Right)) {
						cursorPosition.X += 2;
					}
					if (newState.IsKeyDown (Keys.Left)) {
						cursorPosition.X -= 2;
					}
				}
				break;
			case MenuStates.RangeSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)MenuStates.RangeSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)MenuStates.RangeSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					int selected = menus[(int)MenuStates.RangeSelect].getSelected();
					if (selected < 6) {
						// Range list is a fibonacii series: 1, 2, 3, 5, 8, 13
						int a = 1;
						int distance = 1;
						for (int i = 1; i < selected; i++) {
							int temp = distance;
							distance += a;
							a = temp;
						}
						PushMenuState(MenuStates.TargetSelect);
					} else { // Done
						PopMenuState();
					}
				}
				break;
			case MenuStates.TargetSelect:
				int unitCount = enemyCount + playerCount;
				if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedTarget = (selectedTarget + unitCount -1) % unitCount;
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedTarget = ++selectedTarget % unitCount;
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					PopMenuState();
				}
				break;
			case MenuStates.PositionSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)MenuStates.PositionSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)MenuStates.PositionSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((PositionState)menus[(int)MenuStates.PositionSelect].getSelected ()) {
					case PositionState.Charge:
						PushMenuState(MenuStates.TargetSelect);
						break;
					case PositionState.Stay:
						PushMenuState(MenuStates.CoordSelect);
						break;
					case PositionState.KeepDistance:
						PushMenuState(MenuStates.RangeSelect);
						break;
					case PositionState.RunAway:
						break;
					default: // Done
						PopMenuState();
						break;
					}
				}
				break;
			case MenuStates.ActionSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)MenuStates.ActionSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)MenuStates.ActionSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)MenuStates.ActionSelect].getSelected ()) {
					case ActionState.Attack: // Attack
						PushMenuState(MenuStates.TargetSelect);
						break;
					case ActionState.Position: // Position
						PushMenuState(MenuStates.PositionSelect);
						break;
					default: // Done
						PopMenuState();
						break;
					}
				}
				break;
			case MenuStates.UnitSelect:
			default:
				if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedUnit = (selectedUnit + playerCount -1) % playerCount;
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedUnit = ++selectedUnit % playerCount;
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					PushMenuState(MenuStates.ActionSelect);
				}
				break;
			}
			
		}
		
		protected override void Draw (GameTime gameTime)
		{
			DrawBackground(gameTime);
			
			DrawCursors (gameTime);
			
			DrawUnits (gameTime);
			
			spriteBatch.End ();
			base.Draw (gameTime);
		}
		
		protected void DrawBackground(GameTime gameTime) {
			spriteBatch.Begin();
			// Draw background of Battle area
			
			spriteBatch.Draw (background,
			                  Vector2.Zero,
			                  Color.White);
			
			spriteBatch.End ();
		}
		
		protected void DrawCursors(GameTime gameTime) {
			spriteBatch.Begin ();
			// Selected Unit
			Vector2 drawPosition;
			drawPosition = playerPositions[selectedUnit];
			spriteBatch.Draw (cursor, drawPosition, Color.Yellow);
			
			if (menuState.Peek() == MenuStates.TargetSelect) {
				if (selectedTarget < playerCount) {
					drawPosition = playerPositions[selectedTarget];
				} else {
					drawPosition = enemyPositions[selectedTarget-playerCount];
				}
				spriteBatch.Draw(cursor, drawPosition, Color.Red);
			}
			if (menuState.Peek () == MenuStates.CoordSelect) {
				spriteBatch.Draw (cursor, cursorPosition, Color.White);
			}
			spriteBatch.End ();
		}
		
		protected void DrawUnits(GameTime gameTime) {
			spriteBatch.Begin();
			// Draw Enemy Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, enemyPositions[i], Color.White);
			}
			// Draw Player Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, playerPositions[i], Color.White);
			}
			spriteBatch.End ();
		}
		
	}					
}


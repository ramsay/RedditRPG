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
		
		public SpriteFont CourierNew;
		
		Vector2 cursorPosition;
		
		static int enemyCount = 3;
		static int playerCount = 3;
		
		Unit[] enemyTeam = new Unit[enemyCount];
		
		Unit[] playerTeam = new Unit[playerCount];
		
		static int playTimeLimit = 6; // seconds
		DateTime playStartTime;
		
		int selectedUnit = 0;
		int selectedTarget = 0;
		enum GameState {Input, Play};
		GameState gameState;
		enum InputState {UnitSelect, ActionSelect, PositionSelect, 
			TargetSelect, RangeSelect, CoordSelect};
		Stack<InputState> menuState;
		
		KeyboardState oldState;
		
		BattleMenu[] menus = new BattleMenu[6];
		
		enum ActionState {Attack, Position};
		
		public Game1 ()
		{
			graphics = new GraphicsDeviceManager(this);
			
			// 16:9 Aspect ratio
			float width = 854, height = 480;
			
			graphics.PreferredBackBufferWidth = (int)width;
			graphics.PreferredBackBufferHeight = (int)height;
			
			Stats defaultStats = new Stats(5,50,5,10);
			Vector2 unitPosition = new Vector2(4f/16f*width, 3f/9f*height);
			
			// Initialize enemy positions
			for (int i = 0; i < 3; i++ ) {
				enemyTeam[i] = new Unit(
					"", new Stats(defaultStats), 
					new Vector2(unitPosition.X, unitPosition.Y)
					);
				unitPosition.Y += height/9f;
			}
			// Initialize player positions
			unitPosition.X = 12f/16f*width;
			unitPosition.Y = 3f/9f*height;
			for (int i = 0; i < 3; i++ ) {
				playerTeam[i] = new Unit(
					"", new Stats(defaultStats), 
					new Vector2(unitPosition.X, unitPosition.Y));
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
			
			menus[(int)InputState.ActionSelect] = actionsMenu;
			menus[(int)InputState.PositionSelect] = positionMenu;
			menus[(int)InputState.RangeSelect] = rangeMenu;
			
			foreach (BattleMenu menu in menus) {
				if (menu != null) {
					menu.Visible = false;
					Components.Add (menu);
				}
			}
			menuState = new Stack<InputState>();
			menuState.Push (InputState.UnitSelect);
			gameState = GameState.Input;
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
			
			CourierNew = Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>( "Courier New" );
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
			switch (gameState) {
			case GameState.Play:
				UpdatePlay(newState, gameTime);
				break;
			case GameState.Input:
			default:
				UpdateMenu(newState, gameTime);
				break;
			}
			oldState = newState;
			base.Update (gameTime);
		}
		
		private void InitiateInput() {
			gameState = GameState.Input;
		}
		
		private void InitiatePlay(GameTime gameTime) {
			gameState = GameState.Play;
			playStartTime = DateTime.Now;
			Console.Out.WriteLine ("Play initiated at {0}", playStartTime);
			
			Console.Out.WriteLine ("Player unit's state:");
			for (int i = 0; i < playerCount; i++) {
				Console.Out.Write ("Unit[{0}]: ", i);
				playerTeam[i].WriteAttackState();
				playerTeam[i].WritePositionState();
				Console.Out.WriteLine();
			}
			
			Console.Out.WriteLine ("Enemy unit's state:");
			for (int i = 0; i < playerCount; i++) {
				Console.Out.Write ("Enemy[{0}]: ", i);
				enemyTeam[i].WriteAttackState();
				enemyTeam[i].WritePositionState();
				Console.Out.WriteLine();
			}
		}
		
		private void UpdatePlay(KeyboardState newState, GameTime gameTime) {
			if (DateTime.Now.Subtract(playStartTime).TotalSeconds >= playTimeLimit) {
				InitiateInput();
				return;
			}
			
		}
		
		private bool IsPressed(KeyboardState newState, Keys key)
		{
			if (newState.IsKeyDown(key) && !oldState.IsKeyDown(key)) {
				return true;
			}
			return false;
		}
		
		private void PushInputState(InputState state) {
			// Initializers
			if (state == InputState.CoordSelect) {
				cursorPosition = playerTeam[selectedUnit].Position;
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
		
		private void StoreUnitTarget() {
			switch (menuState.Peek()) {
			case InputState.ActionSelect: //Attack
				if (selectedTarget < playerCount) {
					playerTeam[selectedUnit].setAttackTarget(enemyTeam[selectedTarget]);
				} else {
					playerTeam[selectedUnit].setAttackTarget(enemyTeam[selectedTarget-playerCount]);
				}
				break;
			case InputState.PositionSelect: // Charge
				if (selectedTarget < playerCount) {
					playerTeam[selectedUnit].setPositionTarget(enemyTeam[selectedTarget]);
				} else {
					playerTeam[selectedUnit].setPositionTarget(enemyTeam[selectedTarget-playerCount]);
				}
				break;
			case InputState.RangeSelect: // Keep at Distance
				if (selectedTarget < playerCount) {
					playerTeam[selectedUnit].setPositionTarget(enemyTeam[selectedTarget]);
				} else {
					playerTeam[selectedUnit].setPositionTarget(enemyTeam[selectedTarget-playerCount]);
				}
				break;
			}
		}
		
		private void StoreCoordTarget() {
			switch (menuState.Peek()) {
			case InputState.PositionSelect:
				playerTeam[selectedUnit].setPositionTarget(cursorPosition);
				break;
			}
		}
		
		
		private InputState PopInputState() {
			// Deactivate current Menu
			if (menus[(int)menuState.Peek ()] != null) {
				menus[(int)menuState.Peek ()].Visible = false;
			}
			
			// Restore state
			InputState oldState = menuState.Pop();
			if (oldState == InputState.TargetSelect) {
				StoreUnitTarget();
			} else if (oldState == InputState.CoordSelect) {
				StoreCoordTarget();
			}
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
			case InputState.CoordSelect:
				if (IsPressed(newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					PopInputState();
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
			case InputState.RangeSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.RangeSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.RangeSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					int selected = menus[(int)InputState.RangeSelect].getSelected();
					if (selected < 6) {
						// Range list is a fibonacii series: 1, 2, 3, 5, 8, 13
						int a = 1;
						int distance = 1;
						for (int i = 0; i < selected; i++) {
							int temp = distance;
							distance += a;
							a = temp;
						}
						playerTeam[selectedUnit].setKeepDistance(distance);
						PushInputState(InputState.TargetSelect);
					} else { // Done
						if (playerTeam[selectedUnit].hasPositionTarget()) {
							PopInputState();
						}
					}
				}
				break;
			case InputState.TargetSelect:
				int unitCount = enemyCount + playerCount;
				if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedTarget = (selectedTarget + unitCount -1) % unitCount;
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedTarget = ++selectedTarget % unitCount;
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					PopInputState();
				}
				break;
			case InputState.PositionSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.PositionSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.PositionSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((PositionState)menus[(int)InputState.PositionSelect].getSelected ()) {
					case PositionState.Charge:
						playerTeam[selectedUnit].setPositionState(PositionState.Charge);
						PushInputState(InputState.TargetSelect);
						break;
					case PositionState.Stay:
						playerTeam[selectedUnit].setPositionState(PositionState.Stay);
						PushInputState(InputState.CoordSelect);
						break;
					case PositionState.KeepDistance:
						playerTeam[selectedUnit].setPositionState(PositionState.KeepDistance);
						PushInputState(InputState.RangeSelect);
						break;
					case PositionState.RunAway:
						playerTeam[selectedUnit].setPositionState(PositionState.RunAway);
						break;
					default: // Done
						PopInputState();
						break;
					}
				}
				break;
			case InputState.ActionSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.ActionSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.ActionSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)InputState.ActionSelect].getSelected ()) {
					case ActionState.Attack: // Attack
						PushInputState(InputState.TargetSelect);
						break;
					case ActionState.Position: // Position
						PushInputState(InputState.PositionSelect);
						break;
					default: // Done
						PopInputState();
						break;
					}
				}
				break;
			case InputState.UnitSelect:
			default:
				if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedUnit = (selectedUnit + playerCount ) % (playerCount+1);
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedUnit = ++selectedUnit % (playerCount+1);
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					if (selectedUnit < playerCount) {
						PushInputState(InputState.ActionSelect);
					} else {
						InitiatePlay(gameTime);
					}
				}
				break;
			}
			
		}
		
		protected override void Draw (GameTime gameTime)
		{
			DrawBackground(gameTime);
			
			DrawCursors (gameTime);
			
			DrawUnits (gameTime);
			
			if (gameState == GameState.Input && menuState.Peek () == InputState.UnitSelect) {
				Vector2 menuPosition = new Vector2(
					1f/16f*GraphicsDevice.DisplayMode.Width,
					8f/9f*GraphicsDevice.DisplayMode.Height);
				spriteBatch.Begin ();
				Color color = Color.White;
				if (selectedUnit == playerCount) {
					color = Color.Yellow;
				}
				spriteBatch.DrawString (CourierNew, "Play!", menuPosition, color);
				spriteBatch.End ();
			}
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
			if (selectedUnit < playerCount) {
				drawPosition = playerTeam[selectedUnit].Position;
				spriteBatch.Draw (cursor, drawPosition, Color.Yellow);
			}
			if (menuState.Peek() == InputState.TargetSelect) {
				if (selectedTarget < playerCount) {
					drawPosition = playerTeam[selectedTarget].Position;
				} else {
					drawPosition = enemyTeam[selectedTarget-playerCount].Position;
				}
				spriteBatch.Draw(cursor, drawPosition, Color.Red);
			}
			if (menuState.Peek () == InputState.CoordSelect) {
				spriteBatch.Draw (cursor, cursorPosition, Color.White);
			}
			spriteBatch.End ();
		}
		
		protected void DrawUnits(GameTime gameTime) {
			spriteBatch.Begin();
			// Draw Enemy Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, enemyTeam[i].Position, Color.White);
			}
			// Draw Player Units
			for (int i = 0; i < 3; i++ ) {
				spriteBatch.Draw (unit, playerTeam[i].Position, Color.White);
			}
			spriteBatch.End ();
		}
		
	}					
}


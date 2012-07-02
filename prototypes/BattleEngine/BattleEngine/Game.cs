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
		RenderTarget2D renderTarget;
		KeyboardState oldState;
        MouseState oldMouseState;
		
		SpriteBatch spriteBatch;
		Texture2D unit;
		Texture2D cursor;
		Texture2D background;		
		public SpriteFont CourierNew;
		
		// Game constants
		static int enemyCount = 3;
		static int playerCount = 3;
		enum GameState {Initial, Input, Play, Win, GameOver};
		enum InputState {OffensiveSelect, DefensiveSelect, SkillSelect, ItemSelect, 
			TargetSelect};
		enum ActionState {Basic, Skill, Item, SwapState, Escape};
		
		// Battle entities
		Unit[] enemyTeam = new Unit[enemyCount];
		Unit[] playerTeam = new Unit[playerCount];
		
        //AI
        AI enemyAI;
        
		int selectedUnit = 0;
		int selectedTarget = 0;
		
		GameState gameState;
		/// <summary>
		/// The state of the menu.
		/// OffensiveSelection
		///   Attack  -> TargetSelect -> Done
		///   Skill   -> SkillSelect  -> TargetSelect -> Done
		///   Item    -> ItemSelect   -> TargetSelect -> Done
		///   Defense -> DefensiveSelection
		///   Escape  -> Done
		/// DefensiveSelection
		///   Guard   -> Done
		///   Skill   -> SkillSelect   -> TargetSelect -> Done
		///   Item    -> ItemSelect    -> TargetSelect -> Done
		///   Offense -> OffensiveSelection
		///   Escape  -> Done
		/// </summary>
		Stack<InputState> menuState;
		BattleMenu[] menus = new BattleMenu[5];
		BattleQueue queue;
		BattleQueue.Turn[] turnBlock;
		int turnInputIndex;
		int turnPlayIndex;

        BattleMenu endGameMenu;
		StatsMenu statsMenu;
		
		public Game1 ()
		{
			graphics = new GraphicsDeviceManager(this);
			
            graphics.PreferredBackBufferWidth = (int)BattleConstants.SCREEN_WIDTH;
			graphics.PreferredBackBufferHeight = (int)BattleConstants.SCREEN_HEIGHT;
			
			graphics.IsFullScreen = false; // for now.
			
			Content.RootDirectory = "Content";
			
			// Initialise menu
			Vector2 menuPosition = new Vector2(
				1f/16f*BattleConstants.SCREEN_WIDTH,
				8f/9f*BattleConstants.SCREEN_HEIGHT);
			
			menus[(int)InputState.OffensiveSelect] = new BattleMenu(
                this, new[] {"Attack", "Skills", "Items", "Defense", 
				"Escape", "Back"}, menuPosition);
			
			menus[(int)InputState.DefensiveSelect] = new BattleMenu(
                this, new[] {"Guard", "Skills", "Items", "Offense", 
				"Escape", "Back"}, menuPosition);
			
			menus[(int)InputState.SkillSelect] = new BattleMenu(this, new[]{"Back"}, menuPosition);
			
			menus[(int)InputState.ItemSelect] = new BattleMenu(this, new[]{"Back"}, menuPosition);
			
			menus[(int)InputState.TargetSelect] = new BattleMenu(this, new[]{"Back"}, menuPosition);
			
			foreach (BattleMenu menu in menus) {
				if (menu != null) {
					menu.Visible = false;
					Components.Add (menu);
				}
            }            

            endGameMenu = new BattleMenu(
                this, new[] { "Play Again?", "Quit" }, menuPosition);
            Components.Add(endGameMenu);
			
			queue = new BattleQueue(this, playerTeam, enemyTeam, 10);
			Components.Add (queue);
			
			statsMenu = new StatsMenu(this);
			
			this.IsMouseVisible = true;
		}
		
		protected override void Initialize ()
		{
			base.Initialize();
            Reset();
		}

        private void Reset()
        {
            oldState = Keyboard.GetState();
			oldMouseState = Mouse.GetState();

            Stats defaultStats = new Stats(2.68F, 20, 5, 10);
            Vector2 unitPosition = new Vector2(
                4f / 16f * BattleConstants.SCREEN_WIDTH,
                3f / 9f * BattleConstants.SCREEN_HEIGHT);

            // Initialize enemy positions
            for (int i = 0; i < enemyCount; i++)
            {
                enemyTeam[i] = new Unit(
                    "Enemy", defaultStats,
                    new Vector2(unitPosition.X, unitPosition.Y)
                    );
                unitPosition.Y += BattleConstants.SCREEN_HEIGHT / 9f;
            }
            // Initialize player positions
            unitPosition.X = 12f / 16f * BattleConstants.SCREEN_WIDTH;
            unitPosition.Y = 3f / 9f * BattleConstants.SCREEN_HEIGHT;
            for (int i = 0; i < playerCount; i++)
            {
                playerTeam[i] = new Unit(
                    "Player", defaultStats,
                    new Vector2(unitPosition.X, unitPosition.Y));
                unitPosition.Y += BattleConstants.SCREEN_HEIGHT / 9f;
            }
			queue.Initialize();
			queue.Visible = true;
			
            // Initialize AI
            enemyAI = new AI(enemyTeam, playerTeam);
            foreach(Unit unit in enemyTeam) {
                unit.intelligence = enemyAI.Agressive;
            }

            foreach (BattleMenu menu in menus)
            {
                if (menu != null)
                {
                    menu.Visible = false;
                }
            }
            endGameMenu.Visible = false;
			turnPlayIndex = 0;
			turnInputIndex = 0;
            menuState = new Stack<InputState>();
			gameState = GameState.Initial;
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
			List<Unit> enemyList = new List<Unit>(enemyTeam);
			
			KeyboardState newState = Keyboard.GetState ();
			
			switch (gameState)
			{
				case GameState.Initial:
					turnBlock = queue.getTurnBlock();
					if (turnBlock.Length < 1) {
						gameState = GameState.Initial;
					} else if (enemyList.Contains(turnBlock[0].unit)) {
						gameState = GameState.Play;
					} else {
						gameState = GameState.Input;
					    PushInputState (InputState.OffensiveSelect);
					}
					break;
				case GameState.Win:
                    UpdateEndGame(newState, gameTime);
				    break;
				case GameState.GameOver:
                    UpdateEndGame(newState, gameTime);
					break;
				case GameState.Play:
					UpdatePlay(newState, gameTime);
					break;
				case GameState.Input:
				default:
					UpdateMenu(newState, gameTime);
					break;
			}
			
			// Check if statsMenu should be displayed
			CheckMouseInput();
			
			// Keyboard State
			oldState = newState;
			
			base.Update (gameTime);
		}
		
		private void InitiateInput() {
			gameState = GameState.Input;
		}
		
		/// <summary>
		/// Checks the if a unit has been selected and the unit's stats
		/// </summary>
		private void CheckMouseInput()
		{
			MouseState currentMouseState = Mouse.GetState();
			
			// If player clicked on something
			if( currentMouseState.LeftButton == ButtonState.Released
			    && oldMouseState.LeftButton == ButtonState.Pressed )
			{	
				bool unitSelected = false;
				
				// Ckecks if clicked on emeny
				foreach(Unit enemy in enemyTeam)
				{
					if(MouseHitUnit(currentMouseState.X, currentMouseState.Y, enemy.Position))
					{
						statsMenu.setSelectedUnit(enemy);
						statsMenu.StatsVisible = true;
						unitSelected = true;
						break;
					}
				}
				
				// Checks if clicked on player
				if(!unitSelected)
				{
					foreach(Unit player in playerTeam)
					{
						if(MouseHitUnit(currentMouseState.X, currentMouseState.Y, player.Position))
						{
							statsMenu.setSelectedUnit(player);
							statsMenu.StatsVisible = true;
							unitSelected = true;
							break;
						}
					}
				}
				
				// Turn the menu off if have not selected on anything
				if(!unitSelected)
				{
					statsMenu.StatsVisible = false;
				}
			}		    
			
			oldMouseState = currentMouseState;
		}
		
		private bool MouseHitUnit( int mousePositionX, int mousePositionY, Vector2 unitPosition )
		{
			return mousePositionX >= unitPosition.X && mousePositionX <= unitPosition.X + unit.Width
				&& mousePositionY >= unitPosition.Y && mousePositionY <= unitPosition.Y + unit.Height;
			
		}
			
		private void InitiatePlay(GameTime gameTime) {
			gameState = GameState.Play;
            foreach (Unit unit in enemyTeam) {
                unit.InitializePlayState();
            }
            foreach (Unit unit in playerTeam) {
                unit.InitializePlayState();
            }
		}
		
		private bool hasNoSurvivor( Unit[] team )
		{	
			foreach( Unit unit in team )
			{
				if( unit.CurrentStats.health > 0 )
				{ return false; }
			}
			
			return true;
		}
		
        private void UpdatePlay(KeyboardState newState, GameTime gameTime)
		{
			Queue<BattleQueue.Turn> turns = new Queue<BattleQueue.Turn>(queue.getTurnBlock());
			if (turns.Count < 1) 
			{	
				queue.clearCompleted();
				
				if(hasNoSurvivor(playerTeam)) // TODO: Other conditions? Status, run away etc
				{
                    endGameMenu.Visible = true;
					gameState = GameState.GameOver;
				}
				else if(hasNoSurvivor(enemyTeam))
				{
                    endGameMenu.Visible = true;
					gameState = GameState.Win;
				}
				else
				{
					gameState = GameState.Input;
				}
				
				return;
			}
			else
			{
				if (turnBlock[turnPlayIndex].Equals (BattleQueue.EmptyTurn) || turnBlock[turnPlayIndex].completed) {
					turnPlayIndex++;
					// TODO USE currentTurn.action
					// TODO animate
					turnBlock[turnPlayIndex].completed = true;
				}
			}
		}
		
		private bool IsPressed(KeyboardState newState, Keys key)
		{
			if (newState.IsKeyDown(key) && !oldState.IsKeyDown(key)) {
				return true;
			}
			return false;
		}
		
		private void SwapInputState(InputState state) {
			// Deactivate current Menu
			if (menuState.Count > 0 && menus[(int)menuState.Peek ()] != null) {
				menus[(int)menuState.Peek ()].Visible = false;
			}
			
			if (menuState.Count > 0) {
				menuState.Pop();
			}
			
			// Activate new Menu
			if (menus[(int)state] != null) {
				menus[(int)state].Visible = true;
			}
			//Store state
			menuState.Push (state);
		}
		
		private void PushInputState(InputState state) {
			// Deactivate current Menu
			if (menuState.Count > 0 && menus[(int)menuState.Peek ()] != null) {
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
			case InputState.TargetSelect: //Attack
				if (selectedTarget < playerCount) {
					playerTeam[selectedUnit].setAttackTarget(playerTeam[selectedTarget]);
				} else {
					playerTeam[selectedUnit].setAttackTarget(enemyTeam[selectedTarget-playerCount]);
				}
				break;
			}
		}
		
		private InputState PopInputState() {
			// Deactivate current Menu
			if (menus[(int)menuState.Peek ()] != null) {
				menus[(int)menuState.Peek ()].Visible = false;
			}
			InputState oldState = menuState.Peek ();
			if (menuState.Count > 1) {
				// Restore state
				menuState.Pop();
				if (oldState == InputState.TargetSelect) {
					StoreUnitTarget();
				}
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
			case InputState.TargetSelect:
				int unitCount = enemyCount + playerCount;
				if ( IsPressed (newState, Keys.Back) ) {
					PopInputState();
				} else if ( IsPressed (newState, Keys.Up) || IsPressed (newState, Keys.Left)) {
					selectedTarget = (selectedTarget + unitCount -1) % unitCount;
				} else if (IsPressed (newState, Keys.Down) || IsPressed (newState, Keys.Right)) {
					selectedTarget = ++selectedTarget % unitCount;
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					StoreUnitTarget();
					turnInputIndex++;
				}
				break;
			case InputState.OffensiveSelect:
				if ( IsPressed (newState, Keys.Back) ) {
					PopInputState();
				} else if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.OffensiveSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.OffensiveSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)InputState.OffensiveSelect].getSelected ()) {
					case ActionState.Basic: // Attack
						PushInputState(InputState.TargetSelect);
						break;
					case ActionState.Item:
						PushInputState(InputState.ItemSelect);
						break;
					case ActionState.Skill:
						PushInputState(InputState.SkillSelect);
						break;
					case ActionState.SwapState:
						SwapInputState(InputState.DefensiveSelect);
						break;
					case ActionState.Escape:
						turnBlock[turnInputIndex].action = turnBlock[turnInputIndex].unit.escape;
						PushInputState (InputState.OffensiveSelect);
						break;
					default: // Back
						PopInputState();
						break;
					}
				}
				break;
		    case InputState.DefensiveSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.DefensiveSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.DefensiveSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)InputState.DefensiveSelect].getSelected ()) {
					case ActionState.Basic: // Guard
						turnBlock[turnInputIndex].action = turnBlock[turnInputIndex].unit.guard;
						// Go to next turnBlock item
						turnInputIndex++;
						PushInputState (InputState.OffensiveSelect);
						break;
					case ActionState.Item:
						PushInputState(InputState.ItemSelect);
						break;
					case ActionState.Skill:
						PushInputState(InputState.SkillSelect);
						break;
					case ActionState.SwapState:
						SwapInputState(InputState.OffensiveSelect);
						break;
					case ActionState.Escape:
						turnBlock[turnInputIndex].action = turnBlock[turnInputIndex].unit.escape;
						turnInputIndex++;
						PushInputState (InputState.OffensiveSelect);
						break;
					default: // Back
						turnInputIndex--;
						PopInputState();
						break;
					}
				}
				break;
			case InputState.ItemSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.ItemSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.ItemSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)InputState.ItemSelect].getSelected ()) {
					default: // Back
						turnInputIndex--;
						PopInputState();
						break;
					}
				}
				break;
			case InputState.SkillSelect:
				if ( IsPressed (newState, Keys.Left)) {
					menus[(int)InputState.SkillSelect].selectPrevious();
				} else if (IsPressed (newState, Keys.Right)) {
					menus[(int)InputState.SkillSelect].selectNext();
				} else if (IsPressed (newState, Keys.Space) || IsPressed (newState, Keys.Enter)) {
					switch ((ActionState)menus[(int)InputState.SkillSelect].getSelected ()) {
					default: // Back
						turnInputIndex--;
						PopInputState();
						break;
					}
				}
				break;
			}
			if (turnBlock.Length <= turnInputIndex) {
				gameState = GameState.Play;
			}
		}

        private void UpdateEndGame(KeyboardState newState, GameTime gameTime)
        {
            if (IsPressed(newState, Keys.Left)) {
                endGameMenu.selectPrevious();
            } else if (IsPressed(newState, Keys.Right)) {
                endGameMenu.selectNext();
            }
            else if (IsPressed(newState, Keys.Space) || IsPressed(newState, Keys.Enter)) {
                switch (endGameMenu.getSelected()) {
                case 0: // Play again
                    Reset();
                    break;
                case 1: // Quit
                default:
                    this.Exit();
                    break;

                }
            }
        }

		protected override void Draw (GameTime gameTime)
		{
			DrawBackground(gameTime);
			
			DrawCursors (gameTime);
			
			DrawUnits (gameTime);

            if (gameState == GameState.GameOver) {
                spriteBatch.Begin();
                spriteBatch.DrawString(CourierNew, "You lose!", Vector2.Zero, Color.Red);
                spriteBatch.End();
            }
            if (gameState == GameState.Win) {
                spriteBatch.Begin();
                spriteBatch.DrawString(CourierNew, "You Win!", Vector2.Zero, Color.Green);
                spriteBatch.End();
            }

			if(statsMenu.StatsVisible) { statsMenu.Draw(spriteBatch); }
			
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
			if (menuState.Count > 0 && menuState.Peek() == InputState.TargetSelect) {
				if (selectedTarget < playerCount) {
					drawPosition = playerTeam[selectedTarget].Position;
				} else {
					drawPosition = enemyTeam[selectedTarget-playerCount].Position;
				}
				spriteBatch.Draw(cursor, drawPosition, Color.Red);
			}
			spriteBatch.End ();
		}
		
		protected void DrawUnits(GameTime gameTime) {
			spriteBatch.Begin();
			// Draw Enemy Units
            Color unitFilter = Color.White;
			for (int i = 0; i < 3; i++ ) {
                if (enemyTeam[i].CurrentStats.health < 1) {
                    unitFilter = Color.Red;
                } else {
                    unitFilter = Color.White;
                }
				spriteBatch.Draw (unit, enemyTeam[i].Position, unitFilter);
			}
			// Draw Player Units
			for (int i = 0; i < 3; i++ ) {
                if (playerTeam[i].CurrentStats.health < 1) {
                    unitFilter = Color.Red;
                } else {
                    unitFilter = Color.White;
                }
				spriteBatch.Draw (unit, playerTeam[i].Position, unitFilter);
			}
			spriteBatch.End ();
		}
		
	}					
}


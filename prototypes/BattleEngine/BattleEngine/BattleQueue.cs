using System;
using System.Collections.Generic;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace BattleEngine
{
	/// <summary>
	/// Battle queue. Battle is turn based allowing each party member and
	/// enemy to initiate 1 attack per turn. We shall initialize, show and
	/// allow the player to interact (change) with a
	/// list of the next `length` turns.
	/// </summary>
	public class BattleQueue :  DrawableGameComponent
	{
		/**
		 * Actions:
		 *     Attack: [actor, target] 
		 *     Skill:  [actor, target(optional)]
		 *     Change: Not an Action, just a menu option
		 *     Guard:  [actor]
		 *     Item:   [actor, target, item]
		 *     Escape: [actor] 
		 **/
		public struct Turn {
			public Unit unit;
			public Unit.Action action;
			public bool completed;
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
		}
		public static Turn EmptyTurn = new Turn();
		
		// Drawable fields
		static SpriteFont font;
		
		SpriteBatch spriteBatch;
		
		
		List<Turn> turnList;
		int length;
		int inputIndex;
		
		Unit[] party;
		Unit[] enemies;
		List<Unit> units;
		
		public BattleQueue (Game game, Unit[] party, Unit[] enemies, int length) : base(game)
		{
			this.length = length;
			turnList = new List<Turn>();
			this.party = party;
			this.enemies = enemies;
			units = new List<Unit>();
		}
		
		// Drawable methods
		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			if (font == null) {
				font = Game.Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>( "Courier New" );
			}
			base.LoadContent ();
		}
		
		public override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
			this.populate();
		}
		
		public override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin ();
			
			Vector2 position = Vector2.Zero;
			Color textColor = Color.White;
			
			for (int i = 0; i < length; i++) {
				if (i >= turnList.Count || turnList[i].unit == null) {
					spriteBatch.DrawString (
						font, "Empty", position, 
	                    textColor);
				} else {
					spriteBatch.DrawString (
						font, turnList[i].unit.Name, position, 
	                    textColor);
				}
				position.Y += font.MeasureString ("Bob").Y - 4;
			}
			spriteBatch.End();
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
		}
		
		public void populate()
		{
			units.Clear ();
			units.AddRange (party);
			units.AddRange(enemies);
			// Filter out null, dead, etc.
			foreach( Unit u in units) {
				if (u == null || u.CurrentStats.health < 1) {
					units.Remove (u);
				}
			}
			units.Sort(Unit.SpeedComparison);
			units.Reverse ();
			
			Turn turn;
			while (units.Count > 0 && turnList.Count < length) {
				foreach (Unit unit in units) {
					turn = new Turn();
					turn.unit = unit;
					turnList.Add(turn);
				}
			}
		}
		
		public Turn[] getTurnBlock()
		{
			List<Turn> block = new List<Turn>();
			List<Unit> enemyList = new List<Unit>(enemies);
			if (turnList.Count < 1) {
				return block.ToArray();
			}
			
			bool enemyTurnBlock = false;
			if ( enemyList.Contains (turnList[0].unit)) {
				enemyTurnBlock = true;
			}
			foreach (Turn t in turnList) {
				if (enemyList.Contains(t.unit) && enemyTurnBlock) {
					block.Add (t);
				} else if (!enemyList.Contains(t.unit) && !enemyTurnBlock) {
					block.Add (t);
				}
			}
			return block.ToArray ();
		}
		
		public void clearCompleted()
		{
			while (turnList[0].completed) {
				turnList.RemoveAt(0);
			}
		}
	}
}


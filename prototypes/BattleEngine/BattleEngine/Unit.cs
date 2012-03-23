using System;
using Microsoft.Xna.Framework;

namespace BattleEngine
{
	public class Unit
	{
		private String name;
		public String Name {
			get { return name; }
		}
		
		private Stats stats;
		public Stats Stats {
			get { return stats; }
		}
		
		private Vector2 position;
		public Vector2 Position {
			get { return position; }
		}
		
		private bool attackCompleted;
		private bool attackState;
		private Unit attackTarget;
		private float attackRange = 25; // 25px is approx 0.5 meters on 854 x 480 screen
		
		private PositionState positionState;
		// Allows us to use both static coords and moving units as targets
		private Unit positionTarget;
		
		public Unit (String name, Stats stats, Vector2 position)
		{
			this.name = name;
			this.stats = stats;
			this.position = position;
			this.attackState = false;
			
			this.positionState = PositionState.Stay;
			this.positionTarget = this;
		}
		
		/// <summary>
		/// Initializes a dummy instance of the <see cref="BattleEngine.Unit"/> class.
		/// This is for the purpose of making units and vectors interchangeable 
		/// position targets.
		/// </summary>
		/// <param name='position'>
		/// Position.
		/// </param>
		private Unit(Vector2 position) {
			this.position = position;
		}
		
		public void setAttackTarget(Unit target) {
			attackTarget = target;
		}
		
		public void setPositionTarget(Unit target) {
			positionTarget = target;
		}
		
		public void setPositionTarget(Vector2 target) {
			Unit dummy = new Unit(target);
			positionTarget = dummy;
		}
		protected double distanceTo (Vector2 target) {
			return Math.Sqrt(
				Math.Pow(target.X - position.X, 2) 
				+ Math.Pow (target.Y - position.Y, 2));
		}
		
		public void damage(int attack) {
			int dam = attack - stats.defense;
			if (dam > 0) {
				stats.health -= dam;
			}
		}
		
		private void move(Unit target) {
			// TODO
		}
		
		public void InitizePlayState() {
			attackCompleted = false;
		}
		
		public void play(GameTime gameTime) {
			// Attack takes precedence
			if (attackState && !attackCompleted && attackTarget != null) {
				if (distanceTo(attackTarget.Position) < attackRange) {
					move (attackTarget);
				} else {
					attackTarget.damage(stats.strength);
					attackCompleted = true;
				}
			} else {
				move (positionTarget);
			}
		}
	}
}

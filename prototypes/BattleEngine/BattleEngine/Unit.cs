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
		
		private Stats stats; // Overall stats
		private Stats currentStats; // Current stats in battle

		public Stats Stats {
			get { return stats; }
		}

		public Stats CurrentStats {
			get { return currentStats; }
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
		private int keepDistance;
		private float keepDistancePx;
		
		private MoveTimer movement;
		
		public MoveTimer Movement
		{
			get{ return movement; }
			set{ movement = value; }
		}
		
		public Unit AttackTarget
		{
			get{ return attackTarget; }
		}
		
		public Unit (String name, Stats stats, Vector2 position)
		{
			this.name = name;
			this.stats = stats;
			this.currentStats = new Stats(stats.speed, stats.health, stats.defense, stats.strength);
			this.position = position;
			this.attackState = false;
			
			this.positionState = PositionState.Stay;
			this.positionTarget = this;
			
			//movement = new MoveTimer(0,6.0f, position, position, stats.speed);
			movement = new MoveTimer(0,6.0f, position, position, 20);
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
			attackState = true;
			attackTarget = target;
		}
		
		public void setPositionState(PositionState state) {
			positionState = state;
		}
		
		public void setKeepDistance(int distance) {
			keepDistance = distance;
			keepDistancePx = keepDistance * BattleConstants.METRE_TO_PX;
		}
		
		public void setPositionTarget(Unit target) {
			positionTarget = target;
		}
		
		public bool hasPositionTarget() {
			return positionTarget != null;
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
				//stats.health = Math.Max(stats.health - dam, 0);
				currentStats.health = Math.Max(stats.health - dam, 0);
			}
		}
		
		private void move(GameTime gameTime, Vector2 target)
		{
			movement.Update((float) gameTime.ElapsedGameTime.TotalSeconds, target);
			position = movement.CurrentPosition;
		}
		
		private void moveDirection(GameTime gameTime, Vector2 target)
		{
			movement.UpdateDirection((float) gameTime.ElapsedGameTime.TotalSeconds, target);
			position = movement.CurrentPosition;
		}

		public void InitizePlayState() {
			attackCompleted = false;
		}
		
		public void play(GameTime gameTime) {
			// Attack takes precedence
			if (attackState && !attackCompleted && attackTarget != null) {
				if (distanceTo(attackTarget.Position) > attackRange) {
					move(gameTime, attackTarget.Position);
				} else {
					attackTarget.damage(stats.strength);
					attackCompleted = true;
				}
			}
			// Get into position
			else if(hasPositionTarget())
			{
				moveToPosition(gameTime);
			}
		}
		
		
		private void moveToPosition(GameTime gameTime)
		{
			switch(positionState)
			{
				case PositionState.Charge:
					move(gameTime, positionTarget.Position);
					break;
				case PositionState.KeepDistance:
					float distanceToTarget = Vector2.Distance( positionTarget.Position, position );
					float error = 0.3f * BattleConstants.METRE_TO_PX;  // For now
				
					if( distanceToTarget > (keepDistancePx + error))
					{ move(gameTime, positionTarget.Position); }
					else if( distanceToTarget < (keepDistancePx - error) )
					{
						Vector2 direction = Vector2.Subtract(position, positionTarget.Position);
						direction.Normalize();
						
						moveDirection(gameTime, direction );
					}
				
					break;
				
				case PositionState.RunAway:
					// Get direction away from centre of battle area
					Vector2 runDirection = Vector2.Subtract( position, BattleConstants.AREA_CENTRE_POSITION );
					runDirection.Normalize();	

					moveDirection(gameTime, runDirection );
				
					// TODO: Need feedback for units that reached edge of battle area
					break;				
				case PositionState.Stay:
					move(gameTime, positionTarget.Position);
					break;
			}
			
			
		}
		
		public void WriteAttackState() {
			if (attackState) {
				Console.Out.Write ("Attack unit at {0} ", attackTarget.Position);
			} else {
				Console.Out.Write ("Attack none ");
			}
		}
		
		public void WritePositionState() {
			switch (positionState) {
			case PositionState.Charge:
				Console.Out.Write ("Charge unit at {0} ", positionTarget.Position);
				break;
			case PositionState.KeepDistance:
				Console.Out.Write ("Keep {0} meters away from unit at {1} ", keepDistance, positionTarget.Position);
				break;
			case PositionState.RunAway:
				Console.Out.Write("Run away ");
				break;
			case PositionState.Stay:
				Console.Out.Write("Move to {0} and stay ", positionTarget.Position);
				break;
			default:
				Console.Out.Write("None");
				break;
			}
		}
	}
}


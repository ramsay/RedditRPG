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

        // May or may not be set.
        public Intelligence intelligence;

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
			this.currentStats = stats;
			this.position = position;
			this.attackState = false;
			
			this.positionState = PositionState.Stay;
			this.positionTarget = this;
			
			//movement = new MoveTimer(0,6.0f, position, position, stats.speed);
			movement = new MoveTimer(0,6.0f, position, position, currentStats.speed);
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
				currentStats.health = Math.Max(currentStats.health - dam, 0);
			}
		}
		
		private void move(GameTime gameTime, Vector2 target)
		{
			movement.Update((float) gameTime.ElapsedGameTime.TotalSeconds, target);
			position = movement.CurrentPosition;
		}
		
		private void moveDirection(GameTime gameTime, Vector2 direction)
		{
			movement.UpdateDirection((float) gameTime.ElapsedGameTime.TotalSeconds, direction);
			position = movement.CurrentPosition;
		}

		public void InitializePlayState() {
            if (intelligence != null) {
                intelligence(this);
            }
			attackCompleted = false;
            if (attackState)
            {
                this.movement = new MoveTimer(
                    0, 6.0f, this.position, this.AttackTarget.Position,
                    this.stats.speed);
            }
            else
            {
                this.movement = new MoveTimer(
                    0, 6.0f, this.position, this.positionTarget.Position,
                    this.stats.speed);
            }
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
		}
		
		
		public void WriteAttackState() {
			if (attackState) {
				Console.Out.Write ("Attack unit at {0} ", attackTarget.Position);
			} else {
				Console.Out.Write ("Attack none ");
			}
		}
		
		/**
		 * Attack: [actor, target] 
		 *     Skill:  [actor, target(optional)]
		 *     Change: Not an Action, just a menu option
		 *     Guard:  [actor]
		 *     Item:   [actor, target, item]
		 *     Escape: [actor]
		 */
		public void attack(Unit target, IUseable useable)
		{
		}
		
		public interface IUseable {
			string Name();
			string Description();
		}
		
		public class Useable : IUseable {
			string _name;
			string _description;
			public Useable(string name, string description) { _name = name; _description = description; }
			public string Name() { return _name; }
			public string Description() { return _description; }
		}
		public class Skill : Useable {
			public Skill (string name, string description) : base(name, description) {
			}
		}
		
		public void useSkill(Unit target, IUseable skill)
		{
		}
		
		public void guard(Unit target, IUseable usable)
		{
		}
		
		public class Item : Useable {
			public Item (string name, string description) : base (name, description) {
			}
		}
		
		public void useItem(Unit target, IUseable item)
		{
		}
		
		public void escape(Unit target, IUseable usable)
		{
		}
		
		public delegate void Action(Unit target, IUseable usable);
		
		public static int SpeedComparison(Unit x, Unit y) {
			return x.CurrentStats.speed.CompareTo (y.CurrentStats.speed);
		}
	}
}


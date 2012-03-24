using System;
using Microsoft.Xna.Framework;

namespace BattleEngine
{
	public class MoveTimer
	{
		float time = 0f;
		
		float delay;
		float timeForMove;
		Vector2 goalPosition;
		Vector2 currentPosition;
		Vector2 direction;
		
		float speed;
		
		public Vector2 CurrentPosition
		{
			get { return currentPosition;}
		}
		
		public MoveTimer(float delay, float timeForMove, Vector2 startPosition, Vector2 goalPosition)
		{
			this.delay = delay;
			this.timeForMove = timeForMove;
			this.goalPosition = goalPosition;
			this.currentPosition = startPosition;
			
			this.direction = getDirection(startPosition, goalPosition);
			
			this.speed = Vector2.Distance(goalPosition, startPosition) / timeForMove;
		}
		
		
		
		public bool Update( float secondsPassed )
		{
			time += secondsPassed;
			
			bool result = time >= (delay + timeForMove);
			
			if(result)
			{ currentPosition = goalPosition; }
			else if(time > delay)
			{ currentPosition = currentPosition + (direction * secondsPassed * speed); }
			
			return result;
		}
		
		private Vector2 getDirection(Vector2 start, Vector2 goal)
		{
			Vector2 direction = Vector2.Subtract(goal, start);
			direction.Normalize();
			
			return direction;
		}
	}
}


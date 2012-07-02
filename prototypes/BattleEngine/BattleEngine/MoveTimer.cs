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
		
		// Speed given?
		public MoveTimer(float delay, float timeForMove, Vector2 startPosition, Vector2 goalPosition, float speed)
		{
			Reset(delay, timeForMove, startPosition, goalPosition, speed);
		}
		
		
		/// <summary>
		/// Reset parameters for the entity's moment to prepare for next move
		/// </summary>
		/// <param name='delay'>
		/// Time unit remain idle before moving to target (seconds)
		/// </param>
		/// <param name='timeForMove'>
		/// Total time taken to move to goal (seconds)
		/// </param>
		/// <param name='startPosition'>
		/// Starting position
		/// </param>
		/// <param name='goalPosition'>
		/// Goal position
		/// </param>
		public void Reset(float delay, float timeForMove, Vector2 startPosition, Vector2 goalPosition, float speed)
		{
			// Add speed for input?
			
			time = 0f;
			
			this.delay = delay;
			this.timeForMove = timeForMove;
			this.goalPosition = goalPosition;
			this.currentPosition = startPosition;
			
			this.direction = getDirection(startPosition, goalPosition);
			
			//this.speed = Vector2.Distance(goalPosition, startPosition) / timeForMove;
			this.speed = speed;
			
			
		}
		
		
		/// <summary>
		/// Update the position of the entity according to the amount of time that have passed.
		/// Return true if it has finished moving
		/// </summary>
		/// <param name='secondsPassed'>
		/// Amount of time passed since last frame (seconds)
		/// </param>
		/// <param name='newTarget'>
		/// 
		/// </param>
		public bool Update( float secondsPassed, Vector2 newTarget )
		{
			time += secondsPassed;
			
			bool result = time > (delay + timeForMove);
			
			direction = getDirection(currentPosition, newTarget);
			
			if( !result )
			{
				Vector2 position = currentPosition + (
                    direction * secondsPassed * 
                    (speed*BattleConstants.METRE_TO_PX));
				
			}
			
			return result;
		}
		

		/// <summary>
		/// Update the position of the entity according to the amount of time that have passed.
		/// Return true if it has finished moving
		/// </summary>
		/// <param name='secondsPassed'>
		/// Amount of time passed since last frame (seconds)
		/// </param>
		/// <param name='direction'>
		/// 
		/// </param>
		public bool UpdateDirection( float secondsPassed, Vector2 newDirection )
		{
			time += secondsPassed;
			
			bool result = time > (delay + timeForMove);
			
			direction = newDirection;
			
			if( !result )
			{
				Vector2 position = currentPosition + (
                    direction * secondsPassed * 
                    (speed*BattleConstants.METRE_TO_PX));
				
			}
			
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


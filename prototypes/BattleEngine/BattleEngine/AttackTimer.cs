using System;
using Microsoft.Xna.Framework;

namespace BattleEngine
{
	/// <summary>
	/// Attack timer. When a Unit is performing a physical attack, there is a
	/// "swing" time where the Unit is immobile. In the future we may add a 
	/// delay to the damage calculation, which would allow for implicit "miss" 
	/// for attacks when recalculating if target is in range.
	/// </summary>
	public class AttackTimer
	{
		float time = 0f;
		
		float delay;
		float timeForAttack;
		
		public AttackTimer(float delay, float timeForAttack)
		{
			Reset(delay, timeForAttack);
		}
		
		
		/// <summary>
		/// Reset parameters for the entity's moment to prepare for next move
		/// </summary>
		/// <param name='delay'>
		/// Time before unit's swing hit's the target.
		/// </param>
		/// <param name='timeForAttack'>
		/// Total time taken to swing attack
		/// </param>
		public void Reset(float delay, float timeForAttack)
		{
			
			time = 0f;
			
			this.delay = delay;
			this.timeForAttack = timeForAttack;
		}
		
		
		/// <summary>
		/// Update the amount of time that has passed. 
		/// Return true if it has finished swinging.
		/// </summary>
		/// <param name='secondsPassed'>
		/// Amount of time passed since last frame (seconds)
		/// </param>
		public bool Update( float secondsPassed )
		{
			time += secondsPassed;
			
			bool result = time > (delay + timeForAttack);
			
			return result;
		}
	}
}



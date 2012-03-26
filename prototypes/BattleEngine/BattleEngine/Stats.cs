using System;

namespace BattleEngine
{
	public struct Stats
	{
		public Stats(float speed, int health, int defense, int strength) {
			this.speed = speed;
			this.health = health;
			this.defense = defense;
			this.strength = strength;
		}
		public float speed;
		public int health;
		public int defense;
		public int strength;
	}
}


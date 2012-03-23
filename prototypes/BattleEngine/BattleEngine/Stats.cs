using System;

namespace BattleEngine
{
	public struct Stats
	{
		public Stats(int speed, int health, int defense, int strength) {
			this.speed = speed;
			this.health = health;
			this.defense = defense;
			this.strength = strength;
		}
		public Stats(Stats stat) {
			this.speed = stat.speed;
			this.health = stat.health;
			this.defense = stat.defense;
			this.strength = stat.strength;
		}
		public int speed;
		public int health;
		public int defense;
		public int strength;
	}
}


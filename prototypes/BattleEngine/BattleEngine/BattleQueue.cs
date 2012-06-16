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
		}
		
		List<Turn> turnList;
		int length;
		int currentIndex;
		
		public BattleQueue (Game game, Unit[] party, Unit[] enemies, int length) : base(game)
		{
			this.length = length;
			turnList = new List<Turn>();
		}
		
		public void populate()
		{
			while (turnList.Count < length) {
				turnList.Add(new Turn());
			}
		}
		
		public void change(int newSlot)
		{
			Turn now = turnList[currentIndex];
			Turn later = turnList[newSlot];
			turnList.RemoveAt(currentIndex);
			turnList.Insert (currentIndex, later);
			turnList.Insert (newSlot, now);
		}
		
		public Turn[] getTurnBlock()
		{
			Turn[] block = turnList.GetRange (0, currentIndex).ToArray ();
			return block;
		}
		
		public void clearCompleted()
		{
			while (turnList[0].completed) {
				turnList.RemoveAt(0);
			}
		}
		 
		public void setTurn(Unit.Action action) {
			Turn t = turnList[currentIndex];
			t.action = action;
			currentIndex++;
		}
	}
}


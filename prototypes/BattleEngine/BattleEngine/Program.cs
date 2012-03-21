using System;

namespace BattleEngine
{
	public class Program
	{
		private static Game1 game;
		
		[STAThread]
		static void Main ()
		{
			game = new Game1();
			game.Run ();
		}
	}
}


using System;
using Microsoft.Xna.Framework;

namespace BattleEngine
{
	public static class BattleConstants
	{
        public static float SCREEN_WIDTH = 854f;
	    public static float SCREEN_HEIGHT = 480f;
		
		// In metres
		public static float AREA_WIDTH = 18f;
		public static float AREA_HEIGHT = 8.5f;		
	
		public static float METRE_TO_PX = SCREEN_WIDTH / AREA_WIDTH;
		// Assuming the width takes up the width of the screen
		
		// In pixel
		public static float AREA_WIDTH_PIX= SCREEN_WIDTH;
		public static float AREA_HEIGHT_PIX = AREA_HEIGHT * METRE_TO_PX;
		
		public static Vector2 AREA_CENTRE
			= new Vector2( AREA_WIDTH/2f, AREA_HEIGHT / 2f + 3f );
		// Approx height for now
		
		public static Vector2 AREA_CENTRE_POSITION
			= new Vector2( AREA_CENTRE.X * METRE_TO_PX, AREA_CENTRE.Y * METRE_TO_PX );
		
	}
}


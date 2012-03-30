using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BattleEngine
{
	public class StatsMenu : DrawableGameComponent
	{
		private Unit selectedUnit;
		bool statsVisible = false;
		SpriteFont font;
		Color textColor;
		
		//PropertyInfo[] statsProperties = Stats.GetType().GetProperties();
		
		public StatsMenu (Game game) : base(game)
		{
			font = Game.Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>( "Courier New" );
			
			textColor = Color.Black;
		}
		
		public bool StatsVisible
		{
			set{
				if(!value)
				{ selectedUnit = null; }
				statsVisible = value;
			}
			get{ return statsVisible; }
		}
		
		public void setSelectedUnit(Unit selectedUnit)
		{
			this.selectedUnit = selectedUnit;
		}
		
		public void Draw(SpriteBatch spriteBatch)
		{
			if(statsVisible && selectedUnit != null)
			{
				spriteBatch.Begin();
				
				Vector2 position = new Vector2(0,0);
				int yDiff = 26;
				
				// TODO: Correct this abomination coding & Offside Text?
				
				//if(selectedUnit.Position.Y < BattleConstants.AREA_HEIGHT_PIX / 2)
				//{ position.Y += 200; }
				
				spriteBatch.DrawString (font, "Health: " + selectedUnit.CurrentStats.health + " (" + selectedUnit.Stats.health + ")", position, textColor );
				position.Y += yDiff;

				spriteBatch.DrawString (font, "Strength: " + selectedUnit.CurrentStats.strength + " (" + selectedUnit.Stats.strength + ")", position, textColor);
				position.Y += yDiff;

				spriteBatch.DrawString (font, "Defense: " + selectedUnit.CurrentStats.defense + " (" + selectedUnit.Stats.defense + ")", position, textColor);
				position.Y += yDiff;

				spriteBatch.DrawString (font, "Speed: " + selectedUnit.CurrentStats.speed + " (" + selectedUnit.Stats.speed + ")", position, textColor);

			    spriteBatch.End();
			}
		}
		
	}
}


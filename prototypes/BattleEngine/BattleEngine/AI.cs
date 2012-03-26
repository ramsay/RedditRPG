using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BattleEngine
{
    public delegate void Intelligence(Unit me);

    class AI
    {
        private Unit[] team;
        private Unit[] opponents;

        public AI(Unit[] team, Unit[] opponents)
        {
            this.team = team;
            this.opponents = opponents;
        }

        /// <summary>
        /// Agressive Intellegence Dellegate
        /// This will blindly attack the closest living opponent, then charge
        /// after it.
        /// </summary>
        /// <param name="me">The unit this AI is making decisions for.</param>
        public void Agressive(Unit me)
        {
            Unit target = null;
            float targetDistance = float.MaxValue, nextDistance;
            foreach (Unit opp in this.opponents) {
                if (opp.CurrentStats.health > 0) {
                    if (target == null) {
                        target = opp;
                        targetDistance = Vector2.Distance(
                            me.Position, opp.Position);
                    } else {
                        nextDistance = Vector2.Distance(
                             me.Position, opp.Position);
                        if (nextDistance < targetDistance) {
                            targetDistance = nextDistance;
                            target = opp;
                        }
                    }
                }
            }
            me.setAttackTarget(target);
            me.setPositionState(PositionState.Charge);
            me.setPositionTarget(target);
        }
        
    }
}

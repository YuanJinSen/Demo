using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    [RequireComponent(typeof(Player))]
    public class PlayerStateManager : EntityStateManager<Player>
    {
        [ClassTypeName(typeof(PlayerState))]
        public string[] states;

        protected override List<EntityState<Player>> GetStateList()
        {
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = $"Odyssey.{states[i]}PlayerState";
            }
            return PlayerState.CreateStatesFromStringArray(states);
        }
    }
}
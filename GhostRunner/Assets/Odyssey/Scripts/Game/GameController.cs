using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class GameController : MonoBehaviour
    {
        protected Game _game => Game.Instance;
        protected GameLoader _loader => GameLoader.Instance;

        public void AddRetries(int amount)
        {
            _game.Retries += amount;
        }

        public void LoadScene(string scene)
        {
            _loader.Load(scene);
        }
    }
}
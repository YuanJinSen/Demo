using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class LevelController : MonoBehaviour
    {
        private LevelScore _score => LevelScore.Instance;
        private LevelFinisher _finisher => LevelFinisher.Instance;
		private LevelRespawner _respawner => LevelRespawner.Instance;
		private LevelPauser _pauser => LevelPauser.Instance;

		public void Finish() => _finisher.Finish();
		public void Exit() => _finisher.Exit();
		public void Respawn(bool consumeRetries) => _respawner.Respawn(consumeRetries);
		public void Restart() => _respawner.Restart();
		public void AddCoins(int amount) => _score.coins += amount;
		public void CollectStar(int index) => _score.CollectStar(index);
		public void ConsolidateScore() => _score.Consolidate();
		public void Pause(bool value) => _pauser.Pause(value);
	}
}
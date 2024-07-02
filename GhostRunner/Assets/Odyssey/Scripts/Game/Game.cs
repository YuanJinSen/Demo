using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class Game : Singleton<Game>
    {
        public int initRetries = 3;
        public List<GameLevel> levels;
        public UnityEvent<int> onRetriesSet;
        public UnityEvent onSavingRequested;

        protected int _retries;
        protected int _dataIndex;
        protected DateTime _createAt;
        protected DateTime _updateAt;

        public int Retries
        {
            get
            {
                return _retries;
            }
            set
            {
                _retries = value;
                onRetriesSet?.Invoke(value);
            }
        }

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            Retries = initRetries;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Private



        #endregion

        #region Public

        public GameLevel GetCurrentLevel()
        {
            string scene = GameLoader.Instance.currentScene;
            return levels.Find(level => level.scene == scene);
        }

        public int GetCurrentLevelIndex()
        {
            string scene = GameLoader.Instance.currentScene;
            return levels.FindIndex(level => level.scene == scene);
        }

        public void LoadState(int index, GameData data)
        {
            _dataIndex = index;
            _retries = data.retries;
            _createAt = DateTime.Parse(data.createAt);
            _updateAt = DateTime.Parse(data.updateAt);

            for (int i = 0; i < data.levels.Length; i++)
            {
                levels[i].LoadState(data.levels[i]);
            }
        }

        public LevelData[] LevelsData()
        {
            return levels.Select(level => level.ToData()).ToArray();
        }

        public void RequestSaving()
        {
            GameSaver.Instance.Save(ToData(), _dataIndex);
            onSavingRequested?.Invoke();
        }

        private GameData ToData()
        {
            return new GameData
            {
                retries = Retries,
                levels = LevelsData(),
                createAt = _createAt.ToString(),
                updateAt = DateTime.Now.ToString()
            };
        }

        public void UnlockLevelBySceneName(string sceneName)
        {
            var level = levels.Find((level) => level.scene == sceneName);

            if (level != null)
            {
                level.locked = false;
            }
        }

        public void UnlockNextLevel()
        {
            var index = GetCurrentLevelIndex() + 1;

            if (index >= 0 && index < levels.Count)
            {
                levels[index].locked = false;
            }
        }

        public static void LockCursor(bool value)
        {
#if UNITY_STANDALONE
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
        }

        #endregion
    }
}
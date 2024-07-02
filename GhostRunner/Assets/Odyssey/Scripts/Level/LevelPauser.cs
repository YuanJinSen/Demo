using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class LevelPauser : Singleton<LevelPauser>
    {
        public bool isPause { get; protected set; }
        public bool canPause { get; set; }
        public UIAinmator pauseScreen;
        public UnityEvent onPause;
        public UnityEvent onUnPause;

        public void Pause(bool value)
        {
            if (value != isPause)
            {
                if (!isPause)
                {
                    isPause = true;
                    Time.timeScale = 0;
                    pauseScreen.SetActive(true);
                    pauseScreen.Show();
                    onPause?.Invoke();
                    Game.LockCursor(false);
                }
                else
                {
                    isPause = false;
                    Time.timeScale = 1;
                    pauseScreen.SetActive(false);
                    pauseScreen.Hide();
                    onUnPause?.Invoke();
                    Game.LockCursor(true);
                }
            }
        }
    }
}
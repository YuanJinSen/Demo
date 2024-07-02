using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Odyssey
{
    public class UISaveCard : MonoBehaviour
    {
        public string nextScene;
        public string dateFormat = "MM/dd/y hh:mm";
        [Header("Containers")]
        public GameObject dataContainer;
        public GameObject emptyContainer;
        [Header("UI Elements")]
        public Text retries;
        public Text stars;
        public Text coins;
        public Text createAt;
        public Text updateAt;
        public Button load;
        public Button delete;
        public Button newGame;

        public bool isFill { get; protected set; }

        protected int _index;
        protected GameData _data;

        #region Unity

        private void Awake()
        {
            
        }

        private void Start()
        {
            load.onClick.AddListener(Load);
            delete.onClick.AddListener(Delete);
            newGame.onClick.AddListener(NewGame);
        }

        private void Update()
        {

        }

        #endregion

        #region Private

        private void Load()
        {
            Game.Instance.LoadState(_index, _data);
            GameLoader.Instance.Load(nextScene);
        }

        private void NewGame()
        {
            GameData data = GameData.Create();
            GameSaver.Instance.Save(data, _index);
            Fill(_index, data);
            EventSystem.current.SetSelectedGameObject(load.gameObject);
        }

        private void Delete()
        {
            GameSaver.Instance.Delete(_index);
            Fill(_index, null);
            EventSystem.current.SetSelectedGameObject(load.gameObject);
        }

        #endregion

        #region Public

        public void Fill(int index, GameData data)
        {
            _index = index;
            isFill = data != null;
            emptyContainer.SetActive(!isFill);
            dataContainer.SetActive(isFill);
            load.interactable = isFill;
            delete.interactable = isFill;
            newGame.interactable = !isFill;

            if (isFill)
            {
                _data = data;
                retries.text = data.retries.ToString();
                stars.text = data.TotalStars().ToString();
                coins.text = data.TotalCoins().ToString();
                createAt.text = DateTime.Parse(data.createAt).ToLocalTime().ToString(dateFormat);
                updateAt.text = DateTime.Parse(data.updateAt).ToLocalTime().ToString(dateFormat);
            }
        }

        #endregion
    }
}
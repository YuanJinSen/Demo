using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Odyssey
{
    public class UILevelList : MonoBehaviour
    {
        public bool focusFirstElement = true;
        public UILevelCard card;
        public RectTransform container;

        protected List<UILevelCard> _cardList;

        #region Unity

        private void Awake()
        {
            var levels = Game.Instance.levels;
            _cardList = new List<UILevelCard>();

            for (int i = 0; i < levels.Count; i++)
            {
                _cardList.Add(Instantiate(card, container));
                _cardList[i].Fill(levels[i]);
            }
            if (focusFirstElement)
            {
                EventSystem.current.SetSelectedGameObject(_cardList[0].play.gameObject);
            }
        }

        private void Start()
        {

        }

        //private void Update()
        //{
	//
       // }

        #endregion

        #region Private



        #endregion

        #region Public



        #endregion
    }
}
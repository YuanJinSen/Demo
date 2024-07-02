using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Odyssey
{
    public class UISaveList : MonoBehaviour
    {
        public bool forceFirstElement = true;
        public UISaveCard card;
        public RectTransform container;
        protected List<UISaveCard> _cardList;

        private void Awake()
        {
            GameData[] datas = GameSaver.Instance.LoadList();
            _cardList = new List<UISaveCard>();

            for (int i = 0; i < datas.Length; i++)
            {
                _cardList.Add(Instantiate(card, container));
                _cardList[i].Fill(i, datas[i]);
            }

            if (forceFirstElement)
            {
                if (_cardList[0].isFill)
                {
                    EventSystem.current.SetSelectedGameObject(_cardList[0].load.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(_cardList[0].newGame.gameObject);
                }
            }
        }
    }
}
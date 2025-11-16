using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Appodeal.Solitaire
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class StackController : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image m_Background;

        private readonly List<CardController> m_Cards = new();

        private RectTransform m_RectTransform;
        private float m_Spacing;
        private Color m_BaseColor;
        private Color m_HighlightColor;

        public int Index { get; private set; }

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_BaseColor = m_Background.color;
            m_HighlightColor = m_Background.color * 1.25f;
            m_HighlightColor.a = m_Background.color.a;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (!eventData.pointerDrag.TryGetComponent(out CardController card))
                return;

            CardDrop?.Invoke(card, Index);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Background.color = m_HighlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Background.color = m_BaseColor;
        }

        public void Construct(int index, float spacing)
        {
            Index = index;
            m_Spacing = spacing;
        }

        public void AddCard(CardController card)
        {
            if (m_Cards.Count > 0)
            {
                m_Cards[^1]
                    .SetInteractable(false);
            }

            card.SetInteractable(true);

            m_Cards.Add(card);
            RectTransform rect = card.RectTransform;
            rect.SetParent(m_RectTransform, false);
            rect.SetAsLastSibling();
            rect.anchoredPosition = new Vector2(0, -m_Spacing * (m_Cards.Count - 1));
            card.UpdateStack(Index);
        }

        public bool RemoveCard(CardController card)
        {
            bool removed = m_Cards.Remove(card);

            if (!removed)
                return false;

            if (m_Cards.Count > 0)
            {
                m_Cards[^1]
                    .SetInteractable(true);
            }

            card.UpdateStack(-1);
            return true;
        }

        public event Action<CardController, int> CardDrop;
    }
}
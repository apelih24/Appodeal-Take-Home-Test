using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Appodeal.Solitaire
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class CardController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private CanvasGroup m_CanvasGroup;

        [SerializeField]
        private Image m_FaceImage;

        [SerializeField]
        private TMP_Text m_LabelText;

        private RectTransform m_RectTransform;
        private RectTransform m_DragLayer;
        private bool m_IsDragging;
        private Vector2 m_DragOffset;

        public int CurrentStackIndex { get; private set; } = -1;

        public RectTransform RectTransform => m_RectTransform != null ? m_RectTransform : m_RectTransform = GetComponent<RectTransform>();

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_CanvasGroup.blocksRaycasts = false;

            m_IsDragging = true;
            m_RectTransform.SetParent(m_DragLayer, true);
            m_RectTransform.SetAsLastSibling();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragLayer, eventData.position, eventData.pressEventCamera,
                    out Vector2 localPoint))
                m_DragOffset = m_RectTransform.anchoredPosition - localPoint;

            BeginDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDragging)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragLayer, eventData.position, eventData.pressEventCamera,
                    out Vector2 localPoint))
                m_RectTransform.anchoredPosition = localPoint + m_DragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_IsDragging)
                return;

            m_IsDragging = false;
            m_CanvasGroup.blocksRaycasts = true;
            EndDrag?.Invoke(this);
        }

        public void Construct(RectTransform dragLayer, CardModel model)
        {
            m_DragLayer = dragLayer;
            m_LabelText.text = model.Label;
            m_LabelText.color = model.Color;
            Color darkerColor = model.Color * 0.5f;
            darkerColor.a = model.Color.a;
            m_FaceImage.color = darkerColor;
        }

        public void UpdateStack(int stackIndex)
        {
            CurrentStackIndex = stackIndex;
        }

        public void SetInteractable(bool isInteractable)
        {
            m_CanvasGroup.interactable = isInteractable;
            m_FaceImage.raycastTarget = isInteractable;
        }

        public event Action<CardController> BeginDrag;
        public event Action<CardController> EndDrag;
    }
}
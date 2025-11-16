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
        private Image m_FaceImage;

        [SerializeField]
        private TMP_Text m_LabelText;

        private RectTransform m_RectTransform;
        private RectTransform m_DragLayer;
        private bool m_IsDragging;
        private bool m_LastDropSucceeded;
        private Vector2 m_DragOffset;

        public int CurrentStackIndex { get; private set; } = -1;

        public RectTransform RectTransform => m_RectTransform != null ? m_RectTransform : m_RectTransform = GetComponent<RectTransform>();

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_DragLayer == null)
                return;

            BeginDrag?.Invoke(this);

            m_IsDragging = true;
            m_LastDropSucceeded = false;
            m_RectTransform.SetParent(m_DragLayer, true);
            m_RectTransform.SetAsLastSibling();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragLayer, eventData.position, eventData.pressEventCamera,
                    out Vector2 localPoint))
                m_DragOffset = m_RectTransform.anchoredPosition - localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDragging || m_DragLayer == null)
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
            EndDrag?.Invoke(this, m_LastDropSucceeded);
            m_LastDropSucceeded = false;
        }

        public void Construct(RectTransform dragLayer, CardConfig config)
        {
            m_DragLayer = dragLayer;
            m_LabelText.text = config.Label;
            m_LabelText.color = config.Color;
            Color darkerColor = config.Color * 0.5f;
            darkerColor.a = config.Color.a;
            m_FaceImage.color = darkerColor;
        }

        public void UpdateStackIndex(int stackIndex)
        {
            CurrentStackIndex = stackIndex;
        }

        public void NotifyDropResult(bool success)
        {
            m_LastDropSucceeded = success;
        }

        public event Action<CardController> BeginDrag;
        public event Action<CardController, bool> EndDrag;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Appodeal.Solitaire
{
    public sealed class SolitaireGameManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_BoardRoot;

        [SerializeField]
        private Canvas m_Canvas;

        [SerializeField]
        private CardController m_CardPrefab;

        [SerializeField]
        [Min(1)]
        private int m_CardsPerStack = 4;

        [SerializeField]
        private RectTransform m_DragLayer;

        [SerializeField]
        [Min(1)]
        private int m_MaxUndoActions = 3;

        [SerializeField]
        [Min(1)]
        private int m_StackCount = 3;

        [SerializeField]
        [Min(0f)]
        private float m_StackHorizontalSpacing = 40f;

        [SerializeField]
        private StackController m_StackPrefab;

        [SerializeField]
        [Min(0f)]
        private float m_StackSpacing = 32f;

        [SerializeField]
        private Button m_UndoButton;

        private readonly List<StackController> m_Stacks = new();

        private UndoManager m_UndoManager;
        private bool m_IsCardDroppedOnStack;

        private void Awake()
        {
            m_UndoManager = new UndoManager(m_MaxUndoActions, this);
            CreateStacks();
            List<CardModel> deck = GenerateDeckData(m_Stacks.Count * m_CardsPerStack);
            SpawnCards(deck);
            HookUndoButton();
            UpdateUndoButtonView();
        }

        public void PlaceCardOnStack(int stackIndex, CardController card)
        {
            if (card == null || stackIndex < 0 || stackIndex >= m_Stacks.Count)
                return;

            if (card.CurrentStackIndex >= 0 && card.CurrentStackIndex < m_Stacks.Count)
            {
                StackController originStack = m_Stacks[card.CurrentStackIndex];
                originStack.RemoveCard(card);
            }

            StackController stack = m_Stacks[stackIndex];
            stack.AddCard(card);
        }

        public void RestoreMove(MoveRecord move)
        {
            if (move.Card == null)
                return;

            if (move.ToStack >= 0 && move.ToStack < m_Stacks.Count)
            {
                m_Stacks[move.ToStack]
                    .RemoveCard(move.Card);
            }

            if (move.FromStack >= 0 && move.FromStack < m_Stacks.Count)
            {
                m_Stacks[move.FromStack]
                    .AddCard(move.Card);
            }
        }

        private void CreateStacks()
        {
            m_Stacks.Clear();

            RectTransform prefabRect = m_StackPrefab.GetComponent<RectTransform>();
            float stackWidth = prefabRect.sizeDelta.x;

            for (int stackIndex = 0; stackIndex < m_StackCount; stackIndex++)
            {
                StackController stackInstance = Instantiate(m_StackPrefab, m_BoardRoot);
                stackInstance.name = $"Stack_{stackIndex + 1}";
                RectTransform rect = stackInstance.GetComponent<RectTransform>();
                float offset = (stackIndex - (m_StackCount - 1) * 0.5f) * (stackWidth + m_StackHorizontalSpacing);
                rect.anchoredPosition = new Vector2(offset, 0f);

                stackInstance.Construct(stackIndex, m_StackSpacing);
                stackInstance.CardDrop += HandleCardDrop;
                m_Stacks.Add(stackInstance);
            }
        }

        private void SpawnCards(List<CardModel> deck)
        {
            if (m_Stacks.Count == 0)
                return;

            int index = 0;

            foreach (StackController stack in m_Stacks)
            {
                for (int cardIndex = 0; cardIndex < m_CardsPerStack; cardIndex++)
                {
                    CardController card = CreateCard(deck[index]);
                    stack.AddCard(card);
                    index++;
                }
            }
        }

        private CardController CreateCard(CardModel model)
        {
            CardController card = Instantiate(m_CardPrefab, m_BoardRoot);
            card.name = model.Label;
            card.Construct(m_DragLayer, model);
            card.BeginDrag += HandleBeginDrag;
            card.EndDrag += HandleEndDrag;
            return card;
        }

        private static List<CardModel> GenerateDeckData(int total)
        {
            var deck = new List<CardModel>(Mathf.Max(0, total));

            for (int i = 0; i < total; i++)
            {
                float hue = i / (float)total;
                Color color = Color.HSVToRGB(hue, 0.55f, 0.95f);
                CardModel cardModel = new($"Card_{i + 1}", color);
                deck.Add(cardModel);
            }

            return deck;
        }

        private void HandleCardDrop(CardController card, int targetStackIndex)
        {
            m_IsCardDroppedOnStack = true;
            bool result = TryPlaceCardOnStack(card, targetStackIndex);

            if (result)
                return;

            int originStackIndex = card.CurrentStackIndex;
            PlaceCardOnStack(originStackIndex, card);
        }

        private void HandleBeginDrag(CardController card)
        {
            m_IsCardDroppedOnStack = false;
        }

        private void HandleEndDrag(CardController card)
        {
            if (m_IsCardDroppedOnStack)
                return;

            PlaceCardOnStack(card.CurrentStackIndex, card);
        }

        private bool TryPlaceCardOnStack(CardController card, int targetStackIndex)
        {
            if (targetStackIndex < 0 || targetStackIndex >= m_Stacks.Count)
                return false;

            StackController originStack = m_Stacks[card.CurrentStackIndex];

            if (originStack.Index == targetStackIndex)
                return false;

            PlaceCardOnStack(targetStackIndex, card);
            MoveRecord moveRecord = new(card, originStack.Index, targetStackIndex);
            RecordMove(moveRecord);
            return true;
        }

        private void RecordMove(MoveRecord record)
        {
            m_UndoManager.RecordMove(record);
            UpdateUndoButtonView();
        }

        private void UpdateUndoButtonView()
        {
            m_UndoButton.interactable = m_UndoManager.RecordsCount > 0;
        }

        private void HookUndoButton()
        {
            m_UndoButton.onClick.AddListener(UndoLastMove);
        }

        private void UndoLastMove()
        {
            m_UndoManager.UndoMove();
            UpdateUndoButtonView();
        }
    }
}
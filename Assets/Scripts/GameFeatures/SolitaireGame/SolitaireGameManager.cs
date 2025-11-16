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
        private readonly Dictionary<CardController, int> m_DragSources = new();
        private readonly List<MoveRecord> m_MoveHistory = new();

        private void Awake()
        {
            CreateStacks();
            SpawnCards();
            HookUndoButton();
            UpdateUndoState();
        }

        private void HandleBeginDrag(CardController card)
        {
            if (card.CurrentStackIndex < 0)
                return;

            StackController sourceStack = m_Stacks[card.CurrentStackIndex];

            if (!sourceStack.IsTop(card) || m_DragSources.ContainsKey(card))
                return;

            sourceStack.RemoveCard(card);
            m_DragSources[card] = sourceStack.Index;
            card.UpdateStackIndex(-1);
        }

        private void HandleEndDrag(CardController card, bool dropSucceeded)
        {
            if (!m_DragSources.TryGetValue(card, out int originStack))
                return;

            if (!dropSucceeded)
                PlaceCardOnStack(originStack, card);

            m_DragSources.Remove(card);
        }

        private void HandleCardDrop(CardController card, int targetStackIndex)
        {
            bool result = TryPlaceCardOnStack(card, targetStackIndex, true);
            card.NotifyDropResult(result);
        }

        private void CreateStacks()
        {
            m_Stacks.Clear();

            if (m_StackPrefab == null || m_BoardRoot == null)
                return;

            RectTransform prefabRect = m_StackPrefab.GetComponent<RectTransform>();
            float stackWidth = prefabRect != null ? prefabRect.sizeDelta.x : 220f;

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

        private void SpawnCards()
        {
            if (m_Stacks.Count == 0)
                return;

            if (m_CardPrefab == null)
                return;

            List<CardConfig> deck = GenerateDeckData(m_Stacks.Count * m_CardsPerStack);
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

        private CardController CreateCard(CardConfig config)
        {
            CardController card = Instantiate(m_CardPrefab, m_BoardRoot);
            card.name = config.Label;
            card.Construct(m_DragLayer, config);
            card.BeginDrag += HandleBeginDrag;
            card.EndDrag += HandleEndDrag;
            return card;
        }

        private static List<CardConfig> GenerateDeckData(int total)
        {
            var deck = new List<CardConfig>(Mathf.Max(0, total));

            for (int i = 0; i < total; i++)
            {
                float hue = i / (float)total;
                Color color = Color.HSVToRGB(hue, 0.55f, 0.95f);
                deck.Add(new CardConfig($"Card {i + 1}", color));
            }

            return deck;
        }

        private bool TryPlaceCardOnStack(CardController card, int targetStackIndex, bool recordHistory)
        {
            if (!m_DragSources.TryGetValue(card, out int originStack))
                return false;

            if (targetStackIndex < 0 || targetStackIndex >= m_Stacks.Count)
                return false;

            if (originStack == targetStackIndex)
                return false;

            PlaceCardOnStack(targetStackIndex, card);

            if (recordHistory)
                RegisterMove(new MoveRecord(card, originStack, targetStackIndex));

            return true;
        }

        private void PlaceCardOnStack(int stackIndex, CardController card)
        {
            if (stackIndex < 0 || stackIndex >= m_Stacks.Count)
                return;

            StackController stack = m_Stacks[stackIndex];
            stack.AddCard(card);
        }

        private void RegisterMove(MoveRecord record)
        {
            m_MoveHistory.Add(record);

            if (m_MoveHistory.Count > m_MaxUndoActions)
                m_MoveHistory.RemoveAt(0);

            UpdateUndoState();
        }

        private void UpdateUndoState()
        {
            if (m_UndoButton == null)
                return;

            m_UndoButton.interactable = m_MoveHistory.Count > 0;
        }

        private void HookUndoButton()
        {
            m_UndoButton.onClick.AddListener(UndoLastMove);
        }

        private void UndoLastMove()
        {
            if (m_MoveHistory.Count == 0)
                return;

            int lastIndex = m_MoveHistory.Count - 1;
            MoveRecord move = m_MoveHistory[lastIndex];
            m_MoveHistory.RemoveAt(lastIndex);

            if (move.Card.CurrentStackIndex >= 0 && move.Card.CurrentStackIndex < m_Stacks.Count)
            {
                m_Stacks[move.Card.CurrentStackIndex]
                    .RemoveCard(move.Card);
            }

            PlaceCardOnStack(move.FromStack, move.Card);
            UpdateUndoState();
        }
    }
}
using System.Collections.Generic;

namespace Appodeal.Solitaire
{
    public class UndoManager
    {
        private readonly int m_MaxUndoActions;
        private readonly SolitaireGameManager m_GameManager;
        private readonly List<MoveRecord> m_MoveHistory;

        public int RecordsCount => m_MoveHistory.Count;

        public UndoManager(int maxUndoActions, SolitaireGameManager gameManager)
        {
            m_MaxUndoActions = maxUndoActions;
            m_GameManager = gameManager;
            m_MoveHistory = new List<MoveRecord>(m_MaxUndoActions);
        }

        public void RecordMove(MoveRecord record)
        {
            m_MoveHistory.Add(record);

            if (m_MoveHistory.Count > m_MaxUndoActions)
                m_MoveHistory.RemoveAt(0);
        }

        public void UndoMove()
        {
            if (m_MoveHistory.Count == 0)
                return;

            int lastIndex = m_MoveHistory.Count - 1;
            MoveRecord move = m_MoveHistory[lastIndex];
            m_MoveHistory.RemoveAt(lastIndex);

            m_GameManager.PlaceCardOnStack(move.FromStack, move.Card);
        }
    }
}
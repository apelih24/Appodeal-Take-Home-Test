namespace Appodeal.Solitaire
{
    public readonly struct MoveRecord
    {
        public MoveRecord(CardController card, int fromStack, int toStack)
        {
            Card = card;
            FromStack = fromStack;
            ToStack = toStack;
        }

        public CardController Card { get; }
        public int FromStack { get; }
        public int ToStack { get; }
    }
}
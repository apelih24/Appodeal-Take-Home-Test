using UnityEngine;

namespace Appodeal.Solitaire
{
    public readonly struct CardModel
    {
        public CardModel(string label, Color accent)
        {
            Label = label;
            Color = accent;
        }

        public string Label { get; }
        public Color Color { get; }
    }
}
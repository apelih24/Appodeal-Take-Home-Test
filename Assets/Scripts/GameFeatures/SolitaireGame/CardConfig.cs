using UnityEngine;

namespace Appodeal.Solitaire
{
    public readonly struct CardConfig
    {
        public CardConfig(string label, Color accent)
        {
            Label = label;
            Color = accent;
        }

        public string Label { get; }
        public Color Color { get; }
    }
}
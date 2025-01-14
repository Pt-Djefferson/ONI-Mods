﻿using System.Collections.Generic;
using System.Linq;

namespace BetterInfoCards
{
    public class ColumnInfo
    {
        private const float isOverlappedThreshold = 10f;

        public float offsetX = 0f;
        public List<DisplayCard> displayCards = new List<DisplayCard>();
        public float maxXInCol = 0f;
        public float YMin { get { return displayCards.Last().YMin; } }

        public void MoveAndResize(float colToRightYMin)
        {
            foreach (DisplayCard card in displayCards)
            {
                card.Translate(offsetX);

                if (colToRightYMin < card.YMax - isOverlappedThreshold)
                    card.Resize(maxXInCol);
                else
                    card.Resize(card.Width);
            }
        }
    }
}

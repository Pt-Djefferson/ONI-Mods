﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterInfoCards
{
    public class DisplayCards
    {
        private List<DisplayCard> displayCards = new List<DisplayCard>();
        private GridInfo gridInfo;

        public DisplayCards(List<InfoCard> infoCards)
        {
            var nameSplit = new Dictionary<string, List<InfoCard>>();
            var displaySplit = new List<List<InfoCard>>();
            var detailSplit = new Dictionary<string, List<InfoCard>>();

            foreach (InfoCard card in infoCards)
            {
                string key = card.Title;
                if (!nameSplit.TryGetValue(key, out List<InfoCard> value))
                    value = new List<InfoCard>();
                value.Add(card);
                nameSplit[key] = value;
            }

            foreach (var kvp in nameSplit)
            {
                if (kvp.Value.Count > 1)
                {
                    foreach (InfoCard card in kvp.Value)
                    {
                        string key = card.Title + card.GetTextKey();
                        if (!detailSplit.TryGetValue(key, out List<InfoCard> value))
                            value = new List<InfoCard>();
                        value.Add(card);
                        detailSplit[key] = value;
                    }
                }
                else
                {
                    detailSplit[kvp.Key] = kvp.Value;
                }
            }

            var redirects = new List<int>();

            bool isFirst = true;
            foreach (var kvp in detailSplit)
            {
                DisplayCard card = new DisplayCard(kvp.Value);
                displayCards.Add(card);

                if(!(DetectRunStart_Patch.isUnreachableCard && isFirst))
                    redirects.Add(card.TopCardIndex(infoCards));

                isFirst = false;
            }
            ModifyHits.Instance.Reset(redirects);
            gridInfo = new GridInfo(displayCards, infoCards[0].YMax);
        }

        public void Update()
        {
            gridInfo.MoveAndResizeInfoCards();
        }
    }
}

using SimpleSolitaire.Model.Config;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class TripeaksCard : Card
    {
        public TripeaksCardPositionInfo Info = new TripeaksCardPositionInfo();
        public bool InLayout => Info.Id > 0;
        public bool OverlapsByAny => Info.OverlapsId != null && Info.OverlapsId.Count > 0;
        public bool OverlapsById(int id) => Info.OverlapsId != null && Info.OverlapsId.Any(x => x == id);

        /// <summary>
        /// Initialize card by number.
        /// </summary>
        /// <param name="cardNum">Card number.</param>
        public override void InitWithNumber(int cardNum)
        {
            CardNumber = cardNum;

            CardType = Mathf.FloorToInt(cardNum / Public.CARD_NUMS_OF_SUIT);

            if (CardType == 1 || CardType == 3)
            {
                CardColor = 1;
            }
            else
            {
                CardColor = 0;
            }

            Number = (cardNum % Public.CARD_NUMS_OF_SUIT) + 1;
            CardStatus = 0;

            var path = GetTexture();
            SetBackgroundImg(path);
        }

        /// <summary>
        ///Called when user click on card double times in specific interval
        /// </summary>
        protected override void OnTapToPlace()
        {
            CardLogicComponent.HintManagerComponent.HintAndSetByClick(this);
        }
    }

    public static class TripeaksCardExtensions
    {
        public static bool OverlapsAlreadyInWaste(this TripeaksCard card, HashSet<int> idsInWaste)
        {
            bool result = true;

            for (int i = 0; i < card.Info.OverlapsId.Count; i++)
            {
                if (!idsInWaste.Contains(card.Info.OverlapsId[i]))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
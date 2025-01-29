using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class OverlapingTool : MonoBehaviour
    {
        public float DeckWidth;
        public float DeckHeight;
        public float IntersectSpaceX = 30f;
        public float IntersectSpaceY = 30f;

        public Sprite OverlappedSprite;
        public Sprite NotOverlappedSprite;

        public Color OverlappedInfoColor;
        public Color NotOverlappedInfoColor;

        public void UpdateOverlapsForCard(List<TripeaksLayoutCard> cards = null)
        {
            List<TripeaksLayoutCard> orderedCards = cards.OrderByDescending(x => x.CardInfo.Layer).ToList();

            for (int i = 0; i < orderedCards.Count; i++)
            {
                TripeaksLayoutCard card = orderedCards[i];
                float x1 = card.transform.position.x;
                float x2 = x1 + DeckWidth;
                float y1 = card.transform.position.y;
                float y2 = y1 + DeckHeight;

                List<TripeaksLayoutCard> cardsForCheck = new List<TripeaksLayoutCard>(orderedCards);
                cardsForCheck.Remove(card);

                for (int j = 0; j < cardsForCheck.Count; j++)
                {
                    var otherCard = cardsForCheck[j];

                    float x11 = otherCard.transform.position.x;
                    float x21 = x11 + DeckWidth;
                    float y11 = otherCard.transform.position.y;
                    float y21 = y11 + DeckHeight;

                    if ((x2 >= (x11 + IntersectSpaceX) && x1 <= x11) || (x1 >= x11 && x1 <= (x21 - IntersectSpaceX)))
                    {
                        if ((y2 >= (y11 + IntersectSpaceY) && y1 <= y11) || (y1 >= y11 && y1 <= (y21 - IntersectSpaceY)))
                        {
                            if (otherCard.CardInfo.OverlapsId == null)
                            {
                                otherCard.CardInfo.OverlapsId = new List<int>();
                            }

                            if (otherCard.CardInfo.Layer - 1 == card.CardInfo.Layer && !otherCard.CardInfo.OverlapsId.Contains(card.CardInfo.Id))
                            {
                                otherCard.CardInfo.OverlapsId.Add(card.CardInfo.Id);
                            }
                        }
                    }
                }
            }
        }

        public void VisualizeOverlappedCards(List<TripeaksLayoutCard> cards)
        {
            if (cards == null || !cards.Any())
            {
                return;
            }

            cards.ForEach(Visualize);

            void Visualize(TripeaksLayoutCard card)
            {
                bool isOverlappedByAny = card.CardInfo.OverlapsId != null && card.CardInfo.OverlapsId.Any();

                card.Img.sprite = isOverlappedByAny ? OverlappedSprite : NotOverlappedSprite;
                card.Info.color = isOverlappedByAny ? OverlappedInfoColor : NotOverlappedInfoColor;
            }
        }

        public void RemoveCardOverlaping(List<TripeaksLayoutCard> cards, int cardId)
        {
            if (cards == null || !cards.Any())
            {
                return;
            }

            cards.ForEach(x =>
            {
                if (x.CardInfo.OverlapsId != null && x.CardInfo.OverlapsId.Contains(cardId))
                {
                    x.CardInfo.OverlapsId.Remove(cardId);
                }
            });
        }

        public void ClearCardOverlaps(TripeaksLayoutCard card)
        {
            if (card == null)
            {
                return;
            }

            card.CardInfo.OverlapsId = new List<int>();
        }

        // public void UpdateOverlapsForCard(TripeaksLayoutCard card, List<TripeaksLayoutCard> cards)
        // {
        //     List<TripeaksLayoutCard> cardForCheck = new List<TripeaksLayoutCard>(cards);
        //     List<TripeaksLayoutCard> excludeFromCheck = new List<TripeaksLayoutCard>();
        //     if (excludeFromCheck.Count > 0)
        //     {
        //         for (int i = 0; i < excludeFromCheck.Count; i++)
        //         {
        //             cardForCheck.Remove(excludeFromCheck[i]);
        //         }
        //     }
        //
        //     cardForCheck.Remove(card);
        //
        //     Vector3[] cardCorners = new Vector3[4];
        //     card.Img.rectTransform.GetWorldCorners(cardCorners);
        //     Rect cardRec = new Rect(cardCorners[0].x, cardCorners[0].y, cardCorners[2].x - cardCorners[0].x, cardCorners[2].y - cardCorners[0].y);
        //
        //
        //     for (int i = 0; i < cardForCheck.Count; i++)
        //     {
        //         var otherCard = cardForCheck[i];
        //         Vector3[] otherCardCorners = new Vector3[4];
        //         otherCard.Img.rectTransform.GetWorldCorners(otherCardCorners);
        //         Rect otherCardRec = new Rect(otherCardCorners[0].x, otherCardCorners[0].y, otherCardCorners[2].x - otherCardCorners[0].x, otherCardCorners[2].y - otherCardCorners[0].y);
        //
        //         if (cardRec.Overlaps(otherCardRec))
        //         {
        //           
        //         }
        //     }
        // }
    }
}
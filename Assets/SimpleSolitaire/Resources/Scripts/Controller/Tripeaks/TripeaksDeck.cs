using SimpleSolitaire.Model.Enum;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class TripeaksDeck : Deck
    {
        private TripeaksCardLogic _tripeaksCardLogic => CardLogicComponent as TripeaksCardLogic;

        protected override void UpdateCardsActiveStatus()
        {
            if (Type == DeckType.DECK_TYPE_WASTE)
            {
                int compareNum = 2;

                if (HasCards)
                {
                    int j = 0;

                    for (int i = CardsArray.Count - 1; i >= 0; i--)
                    {
                        Card card = CardsArray[i];
                        if (j < compareNum)
                        {
                            card.gameObject.SetActive(true);
                            j++;
                        }
                        else
                        {
                            card.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int i = CardsArray.Count - 1; i >= 0; i--)
                {
                    CardsArray[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Update card position in game by solitaire style
        /// </summary>
        /// <param name="firstTime">If it first game update</param>
        public override void UpdateCardsPosition(bool firstTime)
        {
            if (CardsCount == 0)
            {
                return;
            }

            for (int i = 0; i < CardsArray.Count; i++)
            {
                TripeaksCard card = CardsArray[i] as TripeaksCard;
                card.transform.SetAsLastSibling();
                if (Type == DeckType.DECK_TYPE_PACK)
                {
                    var packHorizontalSpace = CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_PACK_HORIZONTAL);

                    card.gameObject.transform.position = gameObject.transform.position +
                                                         new Vector3(packHorizontalSpace * (CardsArray.Count - 1 - i), 0, 0);
                    card.CardStatus = 0;
                    card.RestoreBackView();
                }
                else if (Type == DeckType.DECK_TYPE_WASTE)
                {
                    card.gameObject.transform.position = gameObject.transform.position;
                    card.CardStatus = 1;
                    card.UpdateCardImg();
                }
                else if (Type == DeckType.DECK_TYPE_TRIPEAKS)
                {
                    card.CardRect.anchoredPosition = card.Info.AnchoredPos.VectorPos;
                }
            }

            UpdateDraggableStatus();
            UpdateCardsActiveStatus();
        }


        /// <summary>
        /// If we can drop card to other card it will be true.
        /// </summary>
        /// <param name="card">Checking card</param>
        /// <returns>We can drop or no</returns>
        public override bool AcceptCard(Card card)
        {
            Card topCard = GetTopCard();
            switch (Type)
            {
                case DeckType.DECK_TYPE_WASTE:
                    if (topCard != null)
                    {
                        if (topCard.Number == 1 && (card.Number == 2 || card.Number == 13))
                        {
                            return true;
                        }
                        else if (topCard.Number == 13 && (card.Number == 12 || card.Number == 1))
                        {
                            return true;
                        }
                        else if (topCard.Number == card.Number - 1 || topCard.Number == card.Number + 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
            }

            return false;
        }

        public override void UpdateDraggableStatus()
        {
            for (int i = 0; i < CardsArray.Count; i++)
            {
                TripeaksCard card = CardsArray[i] as TripeaksCard;

                if (Type == DeckType.DECK_TYPE_TRIPEAKS)
                {
                    bool isOverlapped = _tripeaksCardLogic.CheckTripeaksOverlaping(card) && !card.OverlapsAlreadyInWaste(_tripeaksCardLogic.IdsInWaste);
                    card.CardStatus = isOverlapped ? 0 : 1;
                    card.IsDraggable = !isOverlapped;

                    card.UpdateCardImg();
                }

                if (Type == DeckType.DECK_TYPE_PACK || Type == DeckType.DECK_TYPE_WASTE)
                {
                    card.IsDraggable = false;
                }
            }
        }

        public override void UpdateBackgroundColor()
        {
        }

        public override void SetCardsToTop(Card card)
        {
            card.transform.SetAsLastSibling();
        }

        public override void SetPositionFromCard(Card card, float x, float y)
        {
            card.SetPosition(new Vector3(x, y, 0));
        }

        public override void RestoreInitialState()
        {
            for (int i = 0; i < CardsArray.Count; i++)
            {
                TripeaksCard card = CardsArray[i] as TripeaksCard;
                card.Info = new TripeaksCardPositionInfo();
                card.RestoreBackView();
            }
            
            CardsArray.Clear();
        }
    }
}
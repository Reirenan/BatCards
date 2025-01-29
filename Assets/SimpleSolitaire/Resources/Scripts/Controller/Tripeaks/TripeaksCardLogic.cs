using System.Collections.Generic;
using SimpleSolitaire.Model.Config;
using SimpleSolitaire.Model.Enum;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class TripeaksCardLogic : CardLogic
    {
        protected override int CardNums => Public.TRIPEAKS_CARD_NUMS;

        [Space, Header("Tripeaks logic fields:")]
        public TripeaksLayoutContainer LayoutContainer;
        public Deck TripeaksDeck;
        public float IntersectSpace = 0f;
        public HashSet<int> IdsInWaste = new HashSet<int>();

        private bool isFirstGeneration = true;

        public override void InitializeSpacesDictionary()
        {
            base.InitializeSpacesDictionary();

            SpacesDict.Add(DeckSpacesTypes.DECK_PACK_HORIZONTAL, -DeckWidth / 25f);
        }

        public override void InitCardLogic()
        {
            InitCurrentLayout();

            base.InitCardLogic();
        }

        public void InitCurrentLayout()
        {
        }

        public override void SubscribeEvents()
        {
        }

        public override void UnsubscribeEvents()
        {
        }

        public override void OnNewGameStart()
        {
            IsGameStarted = true;
        }

        public override void Shuffle(bool bReplay)
        {
            if (!bReplay)
            {
                if (isFirstGeneration)
                {
                    LayoutContainer.SetDefaultLayout();
                    isFirstGeneration = false;
                }
                else
                {
                    LayoutContainer.SetRandomLayout();
                }
            }

            base.Shuffle(bReplay);
        }


        protected override void InitAllDeckArray()
        {
            int j = 0;
            for (int i = 0; i < AceDeckArray.Length; i++)
            {
                AceDeckArray[i].Type = DeckType.DECK_TYPE_ACE;
                AllDeckArray[j++] = AceDeckArray[i];
            }

            if (TripeaksDeck != null)
            {
                TripeaksDeck.Type = DeckType.DECK_TYPE_TRIPEAKS;
                AllDeckArray[j++] = TripeaksDeck;
            }

            if (WasteDeck != null)
            {
                WasteDeck.Type = DeckType.DECK_TYPE_WASTE;
                AllDeckArray[j++] = WasteDeck;
            }

            if (PackDeck != null)
            {
                PackDeck.Type = DeckType.DECK_TYPE_PACK;
                AllDeckArray[j++] = PackDeck;
            }

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                AllDeckArray[i].DeckNum = i;
            }
        }

        /// <summary>
        /// Initialize deck of cards.
        /// </summary>
        protected override void InitDeckCards()
        {
            if (WasteDeck != null)
            {
                IdsInWaste.Clear();
                WasteDeck.PushCard(PackDeck.Pop());
                WasteDeck.UpdateCardsPosition(true);
                WasteDeck.UpdateDraggableStatus();
            }

            if (TripeaksDeck != null)
            {
                for (int j = 0; j < LayoutContainer.CurrentLayout.Infos.Count; j++)
                {
                    TripeaksCard card = PackDeck.Pop() as TripeaksCard;
                    TripeaksCardPositionInfo layoutInfo = LayoutContainer.GetInfoByIndex(j);

                    card.Info = layoutInfo;
                    TripeaksDeck.PushCard(card);
                }

                TripeaksDeck.UpdateCardsPosition(true);
            }

            PackDeck.UpdateCardsPosition(true);
            PackDeck.UpdateDraggableStatus();
        }

        /// <summary>
        /// Call when we drop card.
        /// </summary>
        /// <param name="card">Dropped card</param>
        public override void OnDragEnd(Card card)
        {
            bool isPackWasteNotFound = false;
            bool isHasTarget = false;

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                TripeaksDeck targetDeck = AllDeckArray[i] as TripeaksDeck;
                if (targetDeck == null)
                {
                    continue;
                }

                if (targetDeck.Type == DeckType.DECK_TYPE_WASTE)
                {
                    if (targetDeck.OverlapWithCard(card))
                    {
                        isHasTarget = true;
                        Deck srcDeck = card.Deck;

                        if (targetDeck.AcceptCard(card))
                        {
                            TripeaksCard tripeaksCard = card as TripeaksCard;

                            if (tripeaksCard.InLayout)
                                IdsInWaste.Add(tripeaksCard.Info.Id);

                            WriteUndoState();
                            srcDeck.RemoveCard(card);
                            targetDeck.PushCard(card);
                            targetDeck.UpdateCardsPosition(false);
                            srcDeck.UpdateCardsPosition(false);

                            ActionAfterEachStep();

                            GameManagerComponent.AddScoreValue(Public.SCORE_MOVE_TO);
                            AudioCtrl.Play(AudioController.AudioType.Move);

                            return;
                        }
                    }
                }
                else
                {
                    isPackWasteNotFound = true;
                }
            }

            if (isPackWasteNotFound &&
                (card.Deck.Type != DeckType.DECK_TYPE_PACK) ||
                isHasTarget)
            {
                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.Error);
                }
            }
        }

        /// <summary>
        /// Call when we click on pack with cards.
        /// </summary>
        public override void OnClickPack()
        {
            bool cantPush = !PackDeck.HasCards;

            if (cantPush)
            {
                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.Error);
                }

                return;
            }

            WriteUndoState();

            if (PackDeck.HasCards)
            {
                WasteDeck.PushCard(PackDeck.Pop());
                PackDeck.UpdateCardsPosition(false);
                WasteDeck.UpdateCardsPosition(false);
                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.MoveToWaste);
                }
            }

            ActionAfterEachStep();
        }

        protected override void CheckWinGame()
        {
            bool hasWin = !TripeaksDeck.HasCards;

            if (hasWin)
            {
                GameManagerComponent.HasWinGame();
                IsGameStarted = false;
            }
        }

        // public bool CheckTripeaksOverlaping(TripeaksCard card, List<Card> excludeFromCheck = null)
        // {
        //     List<Card> cardForCheck = new List<Card>(TripeaksDeck.CardsArray);
        //     if (excludeFromCheck != null && excludeFromCheck.Count > 0)
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
        //     card.BackgroundImage.rectTransform.GetWorldCorners(cardCorners);
        //     Rect cardRec = new Rect(cardCorners[0].x,cardCorners[0].y,cardCorners[2].x-cardCorners[0].x,cardCorners[2].y-cardCorners[0].y);
        //
        //     
        //     for (int i = 0; i < cardForCheck.Count; i++)
        //     {
        //         var otherCard = cardForCheck[i] as TripeaksCard;
        //         Vector3[] otherCardCorners = new Vector3[4];
        //         otherCard.BackgroundImage.rectTransform.GetWorldCorners(otherCardCorners);
        //         Rect otherCardRec = new Rect(cardCorners[0].x,cardCorners[0].y,cardCorners[2].x-cardCorners[0].x,cardCorners[2].y-cardCorners[0].y);
        //         
        //         if (cardRec.Overlaps(otherCardRec))
        //         {
        //             return true;
        //         }
        //     }
        //
        //     return false;
        // }

        public bool CheckTripeaksOverlaping(TripeaksCard card) => card.OverlapsByAny;
        
        public void UpdateIdsInWasteDeck()
        {
            IdsInWaste.Clear();

            for (int i = 0; i < WasteDeck.CardsArray.Count; i++)
            {
                TripeaksCard tripeaksCard = WasteDeck.CardsArray[i] as TripeaksCard;
                if (tripeaksCard.InLayout)
                {
                    IdsInWaste.Add(tripeaksCard.Info.Id);
                }
            }
        }
    }
}
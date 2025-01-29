using Newtonsoft.Json.Utilities;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class TripeaksUndoPerformer : UndoPerformer
    {
        public TripeaksLayoutContainer LayoutContainer;
        protected override string LastGameKey => "TripeaksLastGame";
        public override UndoData StatesData
        {
            get => _statesData;
            set => _statesData = (TripeaksUndoData)value;
        }
        private TripeaksUndoData _statesData = new TripeaksUndoData();

        private TripeaksCardLogic Logic => _cardLogicComponent as TripeaksCardLogic;

        public override void Initialize()
        {
            AotHelper.EnsureList<UndoStates>();
            AotHelper.EnsureList<TripeaksUndoStates>();
            AotHelper.EnsureList<TripeaksCardRecord>();
            AotHelper.EnsureList<TripeaksDeckRecord>();
        }

        /// <summary>
        /// Save game with current game state.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="steps"></param>
        /// <param name="score"></param>
        public override void SaveGame(int time, int steps, int score)
        {
            _statesData.IsCountable = IsCountable;
            _statesData.AvailableUndoCounts = AvailableUndoCounts;
            _statesData.Time = time;
            _statesData.Steps = steps;
            _statesData.Score = score;
            _statesData.CardsNums = Logic.CardNumberArray;
            _statesData.LayoutId = Logic.LayoutContainer.CurrentLayout.LayoutId;

            string game = SerializeData(_statesData);
            PlayerPrefs.SetString(LastGameKey, game);
        }

        public override void AddUndoState(Deck[] allDeckArray, bool isTemp = false)
        {
            StatesData.States.Add(new TripeaksUndoStates(allDeckArray, isTemp));
        }

        public override void Undo(bool removeOnlyState = false)
        {
            if (StatesData.States.Count > 0)
            {
                if (removeOnlyState)
                {
                    StatesData.States.RemoveAt(StatesData.States.Count - 1);
                    ActivateUndoButton();
                    return;
                }

                if (IsCountable && AvailableUndoCounts > 0)
                {
                    AvailableUndoCounts--;
                    _undoAvailableCountsText.text = AvailableUndoCounts.ToString();
                }
                else if (IsCountable && AvailableUndoCounts == 0)
                {
                    _gameMgrComponent.OnClickGetUndoAdsBtn();
                    return;
                }

                _hintComponent.IsHintWasUsed = false;
                _cardLogicComponent.IsNeedResetPack = false;

                for (int i = 0; i < _cardLogicComponent.AllDeckArray.Length; i++)
                {
                    _cardLogicComponent.AllDeckArray[i].Clear();
                }

                _cardLogicComponent.PackDeck.PushCardArray(_cardLogicComponent.CardsArray.ToArray(), false, 0);

                UndoProcess();

                _hintComponent.UpdateAvailableForDragCards();
                _cardLogicComponent.GameManagerComponent.CardMove();
                StatesData.States.RemoveAt(StatesData.States.Count - 1);
                ActivateUndoButton();
            }

            if (!removeOnlyState)
            {
                for (int i = 0; i < _cardLogicComponent.AllDeckArray.Length; i++)
                {
                    Deck deck = _cardLogicComponent.AllDeckArray[i];

                    deck.UpdateBackgroundColor();
                }
            }
        }

        /// <summary>
        /// Load game if it exist.
        /// </summary>
        public override void LoadGame()
        {
            if (PlayerPrefs.HasKey(LastGameKey))
            {
                string lastGameData = PlayerPrefs.GetString(LastGameKey);

                StatesData = DeserializeData<TripeaksUndoData>(lastGameData);

                if (_statesData.States.Count > 0)
                {
                    Logic.PackDeck.PushCardArray(Logic.CardsArray.ToArray(), false, 0);
                    Logic.LayoutContainer.SetCurrentLayout(_statesData.LayoutId);

                    _hintComponent.IsHintWasUsed = false;
                    Logic.IsNeedResetPack = false;
                    IsCountable = _statesData.IsCountable;
                    AvailableUndoCounts = _statesData.AvailableUndoCounts;

                    InitCardsNumberArray();

                    UndoProcess();

                    _statesData.States.RemoveAll(x => x.IsTemp);
                    _hintComponent.UpdateAvailableForDragCards();
                    ActivateUndoButton();
                }
            }
        }

        protected override void UndoProcess()
        {
            int statesCount = _statesData.States.Count;
            for (int i = 0; i < _statesData.States[statesCount - 1].DecksRecord.Count; i++)
            {
                TripeaksDeckRecord deckRecord = _statesData.States[statesCount - 1].DecksRecord[i] as TripeaksDeckRecord;
                Deck deck = Logic.AllDeckArray.FirstOrDefault(x => x.DeckNum == deckRecord.DeckNum);

                if (deck == null)
                {
                    return;
                }

                for (int j = 0; j < deckRecord.CardsRecord.Count; j++)
                {
                    Card card = Logic.PackDeck.Pop();
                    card.Deck = deck;
                    deck.CardsArray.Add(card);
                }

                if (deck.HasCards)
                {
                    for (int j = 0; j < deckRecord.CardsRecord.Count; j++)
                    {
                        TripeaksCard card = deck.CardsArray[j] as TripeaksCard;
                        TripeaksCardRecord cardRecord = deckRecord.CardsRecord[j] as TripeaksCardRecord;

                        card.CardType = cardRecord.CardType;
                        card.CardNumber = cardRecord.CardNumber;
                        card.Number = cardRecord.Number;
                        card.CardStatus = cardRecord.CardStatus;
                        card.CardColor = cardRecord.CardColor;
                        card.IsDraggable = cardRecord.IsDraggable;
                        card.IndexZ = cardRecord.IndexZ;
                        card.Deck = deck;
                        card.transform.localPosition = cardRecord.Pos.VectorPos;
                        card.transform.SetSiblingIndex(cardRecord.SiblingIndex);
                        TripeaksCardPositionInfo info = LayoutContainer.GetInfoById(cardRecord.InfoId);
                        card.Info = info;
#if UNITY_EDITOR
                        string cardName = $"{card.GetTypeName()}_{card.Number}";
                        card.gameObject.name = $"CardHolder ({cardName})";
                        card.BackgroundImage.gameObject.name = $"Card_{cardName}";
#endif
                    }
                }

                deck.UpdateCardsPosition(false);
            }

            Logic.UpdateIdsInWasteDeck();
            Logic.TripeaksDeck.UpdateCardsPosition(false);
        }

        /// <summary>
        /// Is has saved game process.
        /// </summary>
        public override bool IsHasGame()
        {
            bool isHasGame = false;

            if (PlayerPrefs.HasKey(LastGameKey))
            {
                string lastGameData = PlayerPrefs.GetString(LastGameKey);
                UndoData data = DeserializeData<TripeaksUndoData>(lastGameData);

                if (data != null && data.States.Count > 0)
                {
                    isHasGame = true;
                }
            }

            return isHasGame;
        }
    }
}
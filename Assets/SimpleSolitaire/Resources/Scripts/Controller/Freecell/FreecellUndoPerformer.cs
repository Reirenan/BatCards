using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class FreecellUndoPerformer : UndoPerformer
    {
        protected override string LastGameKey => "FreecellLastGame";

        public override UndoData StatesData
        {
            get => _statesData;
            set => _statesData = (FreecellUndoData) value;
        }

        private FreecellUndoData _statesData = new FreecellUndoData();

        private FreecellCardLogic Logic => _cardLogicComponent as FreecellCardLogic;

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
            _statesData.AmountType = Logic.CurrentFreecellAmountType;
            _statesData.Difficulty = Logic.CurrentDifficultyType;
            
            string game = SerializeData(_statesData);
            PlayerPrefs.SetString(LastGameKey, game);
        }

        public override void Undo(bool removeOnlyState = false)
        {
            base.Undo(removeOnlyState);

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

                StatesData = DeserializeData<FreecellUndoData>(lastGameData);

                if (_statesData.States.Count > 0)
                {
                    Logic.PackDeck.PushCardArray(Logic.CardsArray.ToArray(), false, 0);
                    Logic.SetFreecellAmountImmediately(_statesData.AmountType);
                    Logic.SetFreecellDifficultyImmediately(_statesData.Difficulty);

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

        /// <summary>
        /// Is has saved game process.
        /// </summary>
        public override bool IsHasGame()
        {
            bool isHasGame = false;

            if (PlayerPrefs.HasKey(LastGameKey))
            {
                string lastGameData = PlayerPrefs.GetString(LastGameKey);
                UndoData data = DeserializeData<FreecellUndoData>(lastGameData);

                if (data != null && data.States.Count > 0)
                {
                    isHasGame = true;
                }
            }

            return isHasGame;
        }
    }
}
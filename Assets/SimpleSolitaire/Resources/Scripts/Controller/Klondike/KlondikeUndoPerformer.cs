using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class KlondikeUndoPerformer : UndoPerformer
    {
        protected override string LastGameKey => "KlondikeLastGame";
        public override UndoData StatesData
        {
            get => _statesData;
            set => _statesData = (KlondikeUndoData)value;
        }

        private KlondikeUndoData _statesData = new KlondikeUndoData();

        private KlondikeCardLogic Logic => _cardLogicComponent as KlondikeCardLogic;

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
            _statesData.Rule = Logic.CurrentRule;

            string game = SerializeData(_statesData);
            PlayerPrefs.SetString(LastGameKey, game);
        }

        /// <summary>
        /// Load game if it exist.
        /// </summary>
        public override void LoadGame()
        {
            if (PlayerPrefs.HasKey(LastGameKey))
            {
                string lastGameData = PlayerPrefs.GetString(LastGameKey);

                StatesData = DeserializeData<KlondikeUndoData>(lastGameData);

                if (_statesData.States.Count > 0)
                {
                    Logic.PackDeck.PushCardArray(Logic.CardsArray.ToArray(), false, 0);

                    _hintComponent.IsHintWasUsed = false;
                    Logic.IsNeedResetPack = false;
                    IsCountable = _statesData.IsCountable;
                    AvailableUndoCounts = _statesData.AvailableUndoCounts;
                    Logic.SetRuleImmediately(_statesData.Rule);

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
                UndoData data = DeserializeData<KlondikeUndoData>(lastGameData);

                if (data != null && data.States.Count > 0)
                {
                    isHasGame = true;
                }
            }

            return isHasGame;
        }
    }
}
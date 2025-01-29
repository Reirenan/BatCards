namespace SimpleSolitaire.Controller
{
    public class KlondikeGameManager : GameManager
    {
        private KlondikeCardLogic _klondikeCardLogic => _cardLogic as KlondikeCardLogic;

        protected override void InitCardLogic()
        {
            _klondikeCardLogic.InitRuleToggles();
        }

        protected override void OnStatisticsLayerClosed()
        {
            StatisticsController.Instance.InitRuleToggle(_klondikeCardLogic.CurrentRule);

            base.OnStatisticsLayerClosed();
        }
    }
}
namespace SimpleSolitaire.Controller
{
    public class SpiderGameManager : GameManager
    {
        private SpiderCardLogic _spiderCardLogic => _cardLogic as SpiderCardLogic;

        protected override void InitCardLogic()
        {
            _spiderCardLogic.InitSuitsToggles();
        }
    }
}
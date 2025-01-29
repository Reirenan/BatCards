namespace SimpleSolitaire.Controller
{
    public class FreecellGameManager : GameManager
    {
        private FreecellCardLogic _freecellCardLogic => _cardLogic as FreecellCardLogic;

        protected override void InitCardLogic()
        {
            _freecellCardLogic.InitFreecellToggles();
        }
    }
}
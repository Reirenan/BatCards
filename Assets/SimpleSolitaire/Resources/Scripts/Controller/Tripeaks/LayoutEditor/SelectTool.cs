using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public class SelectTool : MonoBehaviour
    {
        public TripeaksLayoutCard Card { get; private set; }

        public void SelectCard(TripeaksLayoutCard card)
        {
            if (card == null)
            {
                return;
            }

            Card = card;
            Card.Select();
        }

        public void DeselectCard()
        {
            if (Card == null)
            {
                return;
            }

            Card.Deselect();
            Card = null;
        }
    }
}
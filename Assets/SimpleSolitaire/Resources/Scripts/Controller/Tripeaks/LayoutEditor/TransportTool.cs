using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class TransportTool : MonoBehaviour
    {
        public RectTransform Rect;
        public Vector2 AnchoredPos => Rect.anchoredPosition;
        public TripeaksLayoutCard Card { get; private set; }

        [SerializeField] private Vector2 _bordersX;
        [SerializeField] private Vector2 _bordersY;
        
        private Vector3 _offsetBetweenCardCenterAndMouse;

        public void SetCard(TripeaksLayoutCard card, int siblingIndex)
        {
            Card = card;
            Card.transform.SetSiblingIndex(siblingIndex);

            _offsetBetweenCardCenterAndMouse = Input.mousePosition - Card.transform.position;
        }

        public void ResetCard()
        {
            Card = null;
        }

        public void Update()
        {
            if (Card == null)
            {
                return;
            }

            Vector3 mousePosition = Input.mousePosition - _offsetBetweenCardCenterAndMouse;

            Vector2 truncatedMousePos = new Vector2Int(
                Mathf.Clamp((int)mousePosition.x, (int)_bordersX.x, (int)_bordersX.y),
                Mathf.Clamp((int)mousePosition.y, (int)_bordersY.x, (int)_bordersY.y)
            );
            Rect.transform.position = truncatedMousePos;
            Card.Rect.anchoredPosition = AnchoredPos;
            UpdateCardLayoutPosition();
        }

        public void SetPosition(TripeaksLayoutCard card, Vector2Int anchoredPos)
        {
            if (card == null)
            {
                Debug.LogError("Has no card for transport. Select first.");
                return;
            }

            card.Rect.anchoredPosition = anchoredPos;

            SetCardAnchoredPos(card, anchoredPos);
        }

        private void UpdateCardLayoutPosition()
        {
            if (Card == null)
            {
                return;
            }

            Vector2Int anchoredPosInt = new Vector2Int((int)AnchoredPos.x, (int)AnchoredPos.y);

            SetCardAnchoredPos(Card, anchoredPosInt);
        }

        private void SetCardAnchoredPos(TripeaksLayoutCard card, Vector2Int anchoredPos)
        {
            card.CardInfo.AnchoredPos = new CardPosition(anchoredPos);
            card.UpdateInfo();
        }
    }
}
using SimpleSolitaire.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleSolitaire
{
    public class TripeaksLayoutCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public RectTransform Rect;
        public Image Img;
        public CanvasGroup ContentGroup;
        public GameObject Selected;
        public Text Info;

        public Vector2 AnchoredPos => Rect.anchoredPosition;
        public bool IsSelected => Selected.activeInHierarchy;

        public TripeaksCardPositionInfo CardInfo;
        private bool _isDragging;

        public void Init(TripeaksCardPositionInfo info)
        {
            CardInfo = info;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            if (!IsSelected)
            {
                TripeaksLayoutCreator.Instance.SelectCard(this);
                return;
            }

            TripeaksLayoutCreator.Instance.SetTransport(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            TripeaksLayoutCreator.Instance.ResetTransport();

            _isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging)
            {
                return;
            }

            TripeaksLayoutCreator.Instance.SelectCard(this);
        }

        public void UpdateInfo()
        {
            if (CardInfo == null)
                return;

            Info.text = CardInfo.ToInterpolatedFormat;
        }

        public void Select()
        {
            Selected.SetActive(true);
        }

        public void Deselect()
        {
            Selected.SetActive(false);
        }

        public void SetPreviewMode(bool state)
        {
            ContentGroup.alpha = state ? 0 : 1;
        }
    }
}
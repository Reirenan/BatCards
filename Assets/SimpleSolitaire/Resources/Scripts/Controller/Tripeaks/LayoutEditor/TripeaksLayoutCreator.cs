using SimpleSolitaire.Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SimpleSolitaire.Model.Config;

namespace SimpleSolitaire
{
    public class TripeaksLayoutCreator : MonoBehaviour
    {
        public static TripeaksLayoutCreator Instance;

        [Header("Settings:")] public string RelativePreviewsFolderPath = $"/SimpleSolitaire/Resources/ScriptableObjects/Tripeaks/Previews/";
        public string PreviewsResourcesFolderPath = "ScriptableObjects/Tripeaks/Previews/";
        [Header("References:")] public TripeaksLayoutContainer Container;

        public TripeaksLayoutCard Card;
        public RectTransform CardsContainer;

        public List<TripeaksLayoutCard> Cards = new List<TripeaksLayoutCard>();
        public Text CardInfo;

        [Header("Tools:")] public TransportTool Transport;
        public SelectTool Select;
        public LayerTool Layer;
        public PreviewTool Preview;
        public OverlapingTool Overlaping;

        public TripeaksLayoutCard LastCard { get; private set; }

        [Header("Inputs:")][SerializeField] private InputField _xInput;
        [SerializeField] private InputField _yInput;
        [SerializeField] private InputField _layerInput;
        [SerializeField] private InputField _layoutIdInput;

        [Header("Outputs: ")][SerializeField] private Text _currentLayoutText;

        [Header("Buttons:")][SerializeField] private Button _removeCardBtn;
        [SerializeField] private Button _deselectAreaBtn;

        private TripeaksLayoutData _tempLayoutData;
        private int _currentPuzzleId;

        private void Awake()
        {
            Instance = this;
            _tempLayoutData = null;
            SetCurrentPuzzleId(-1);

            if (_deselectAreaBtn != null)
                _deselectAreaBtn.onClick.AddListener(DeselectCard);
        }

        private void OnDestroy()
        {
            if (_deselectAreaBtn != null)
                _deselectAreaBtn.onClick.RemoveListener(DeselectCard);
        }

        private void SetCurrentPuzzleId(int id)
        {
            _currentPuzzleId = id;
            _currentLayoutText.text = $"CURRENT LAYOUT: {_currentPuzzleId}";
        }

        public void Update()
        {
            UpdateLastCardInfo();
            UpdateLastCardDependencies();

            if (Input.GetKeyDown(KeyCode.P))
            {
                MakeLayoutPreview();
            }
        }

        public void SetTransport(TripeaksLayoutCard card)
        {
            if (card == null)
            {
                return;
            }

            LastCard = card;

            int lastSiblingIndexByLayer = 0;

            for (int i = Cards.Count - 1; i >= 0; i--)
            {
                if (Cards[i].CardInfo.Layer == LastCard.CardInfo.Layer)
                {
                    lastSiblingIndexByLayer = i;
                    break;
                }
            }

            Transport.SetCard(card, lastSiblingIndexByLayer);
        }

        public void SelectCard(TripeaksLayoutCard card)
        {
            if (card == null)
            {
                return;
            }

            if (card == LastCard)
            {
                DeselectCard();
                return;
            }

            if (LastCard != null)
            {
                DeselectCard();
            }

            LastCard = card;

            Select.SelectCard(card);

            UpdateInputsValues();
        }

        public void DeselectCard()
        {
            Select.DeselectCard();
            LastCard = null;
        }

        public void ResetTransport()
        {
            Transport.ResetCard();

            UpdateInputsValues();
            UpdateOverlaping();

            for (int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];
                card.transform.SetSiblingIndex(i);
            }
        }

        private void UpdateInputsValues()
        {
            float x = LastCard != null ? LastCard.CardInfo.AnchoredPos.X : 0;
            float y = LastCard != null ? LastCard.CardInfo.AnchoredPos.Y : 0;
            float layer = LastCard != null ? LastCard.CardInfo.Layer : 0;

            _xInput.text = x.ToString();
            _yInput.text = y.ToString();
            _layerInput.text = layer.ToString();
        }

        public void ResetLayout()
        {
            ClearLayout();

            _tempLayoutData.Infos.Clear();
            _tempLayoutData.Preview = null;
        }

        public void ClearLayout()
        {
            if (!Cards.Any())
            {
                Debug.LogError("Has no cards for refreshing.");
                return;
            }

            DeselectCard();
            for (int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];

                Destroy(card.gameObject);
            }

            Cards.Clear();
        }

        public void AddCard()
        {
            if (Cards.Count >= Public.TRIPEAKS_CARD_NUMS)
            {
                Debug.LogError($"Cards limit reached. Maximum cards ampunt = {Public.TRIPEAKS_CARD_NUMS}.");
                return;
            }

            if (_tempLayoutData == null)
            {
                Debug.LogError($"Create layout at first.");
                return;
            }

            TripeaksLayoutCard card = Instantiate(Card, CardsContainer);

            if (card != null)
            {
                int maxId = Cards.Any() ? Cards.Select(x => x.CardInfo.Id).Max() : 0;
                int newId = maxId + 1;
                TripeaksCardPositionInfo cardInfo = new TripeaksCardPositionInfo(newId);
                card.Init(cardInfo);
                card.UpdateInfo();
                card.name = $"Card {cardInfo.Id}";
                if (!card.gameObject.activeInHierarchy)
                {
                    card.gameObject.SetActive(true);
                }

                Cards.Add(card);
                _tempLayoutData.Infos.Add(cardInfo);

                InternalSetLayer(card, shouldUpdateOverlaping: false);
                InternalSetPosition(card, new Vector2Int(0, 500), shouldUpdateOverlaping: false);
                UpdateOverlaping();
            }
        }

        public void RemoveCard()
        {
            if (_tempLayoutData == null)
            {
                Debug.LogError($"Create layout at first.");
                return;
            }

            if (LastCard == null)
            {
                Debug.LogError("Has no card for remove.");
                return;
            }

            var card = LastCard;

            UpdateOverlaping();

            _tempLayoutData.Infos.RemoveAll(x => x.Id == card.CardInfo.Id);
            Cards.RemoveAll(x => x.CardInfo.Id == card.CardInfo.Id);

            DeselectCard();
            Destroy(card.gameObject);
        }

        public void UpdateLastCardInfo()
        {
            if (LastCard == null)
            {
                CardInfo.text = $"Please choose a card.";
                return;
            }

            CardInfo.text = LastCard.CardInfo.ToOneLineFormat;
        }

        public void UpdateLastCardDependencies()
        {
            if (LastCard == null)
            {
                if (_removeCardBtn.interactable)
                {
                    _removeCardBtn.interactable = false;
                }

                return;
            }

            if (!_removeCardBtn.interactable)
            {
                _removeCardBtn.interactable = true;
            }
        }

        public void SetPosition()
        {
            InternalSetPosition(LastCard);
        }

        private void InternalSetPosition(TripeaksLayoutCard card, Vector2Int? customPosition = null, bool shouldUpdateOverlaping = true)
        {
            if (_tempLayoutData == null)
            {
                Debug.LogError($"Create layout at first.");
                return;
            }

            if (!int.TryParse(_xInput.text, out int x))
            {
                x = 0;
            }

            if (!int.TryParse(_yInput.text, out int y))
            {
                y = 0;
            }

            Vector2Int position = customPosition ?? new Vector2Int(x, y);

            Transport.SetPosition(card, position);
            Overlaping.ClearCardOverlaps(card);
            if (shouldUpdateOverlaping)
            {
                UpdateOverlaping();
            }
        }

        public void SetLayer()
        {
            InternalSetLayer(LastCard);
        }

        public void ValidateLayers()
        {
            if (_tempLayoutData == null)
            {
                Debug.LogError($"Choose layout first.");
                return;
            }

            for (int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];

                Layer.SetLayer(card, card.CardInfo.Layer);
                Layer.Reorganize(_tempLayoutData, ref Cards);
                Overlaping.ClearCardOverlaps(card);
                Overlaping.RemoveCardOverlaping(Cards, card.CardInfo.Id);
                Overlaping.UpdateOverlapsForCard(Cards);
                Overlaping.VisualizeOverlappedCards(Cards);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Container);
#endif
        }

        private void InternalSetLayer(TripeaksLayoutCard card, int? customLayer = null, bool shouldUpdateOverlaping = true)
        {
            if (_tempLayoutData == null)
            {
                Debug.LogError($"Create layout at first.");
                return;
            }

            if (!int.TryParse(_layerInput.text, out int layer))
            {
                layer = 0;
            }

            Layer.SetLayer(card, customLayer ?? layer);
            Layer.Reorganize(_tempLayoutData, ref Cards);
            Overlaping.ClearCardOverlaps(card);
            if (shouldUpdateOverlaping)
            {
                UpdateOverlaping();
            }
        }

        public void MakeLayoutPreview()
        {
            if (_tempLayoutData == null)
            {
                Debug.LogError($"Create layout at first.");
                return;
            }

            string pathToPreview = $"{RelativePreviewsFolderPath}Preview_{_tempLayoutData.LayoutId}.png";
            Preview.MakePreview(Cards, Application.dataPath + pathToPreview, OnPreviewCreated);

            void OnPreviewCreated(Texture2D texture)
            {
                string spriteAssetPath = $"{PreviewsResourcesFolderPath}Preview_{_tempLayoutData.LayoutId}";
                Sprite sprite = Resources.Load<Sprite>(spriteAssetPath);
                _tempLayoutData.Preview = sprite;
            }
        }

        public void LoadLayout()
        {
            if (!int.TryParse(_layoutIdInput.text, out int id))
            {
                id = -1;
            }

            TripeaksLayoutData data = Container.LoadLayout(id);
            if (data != null)
            {
                if (_tempLayoutData != null)
                {
                    ClearLayout();
                }

                _tempLayoutData = data;
                SetCurrentPuzzleId(id);

                for (int i = 0; i < _tempLayoutData.Infos.Count; i++)
                {
                    TripeaksLayoutCard card = Instantiate(Card, CardsContainer);
                    if (card)
                    {
                        TripeaksCardPositionInfo cardInfo = _tempLayoutData.Infos[i];

                        card.Init(cardInfo);
                        card.UpdateInfo();

                        Transport.SetPosition(card, (Vector2Int)cardInfo.AnchoredPos.VectorPosInt);
                        Layer.SetLayer(card, cardInfo.Layer);

                        if (!card.gameObject.activeInHierarchy)
                        {
                            card.gameObject.SetActive(true);
                        }

                        Cards.Add(card);

                        card.name = $"Card {cardInfo.Id}";
                    }
                }

                Overlaping.VisualizeOverlappedCards(Cards);
            }
            else
            {
                Debug.LogError($"Can't find layout with given id: {id}.");
            }
        }

        public void CreateNewLayout()
        {
            _tempLayoutData = Container.CreateNewPuzzle();
            SetCurrentPuzzleId(_tempLayoutData.LayoutId);
            ResetLayout();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Container);
#endif
        }

        private void UpdateOverlaping()
        {
            if (LastCard == null)
            {
                return;
            }

            Overlaping.RemoveCardOverlaping(Cards, LastCard.CardInfo.Id);
            Overlaping.UpdateOverlapsForCard(Cards);
            Overlaping.VisualizeOverlappedCards(Cards);
        }
    }
}
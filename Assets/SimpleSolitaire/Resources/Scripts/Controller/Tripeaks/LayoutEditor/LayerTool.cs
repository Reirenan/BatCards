using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class LayerTool : MonoBehaviour
    {
        public void SetLayer(TripeaksLayoutCard card, int layer)
        {
            if (card == null)
            {
                Debug.LogError("Has no card for change layer. Select first.");

                return;
            }

            card.CardInfo.Layer = layer;
            card.UpdateInfo();
        }

        public void Reorganize(TripeaksLayoutData layout, ref List<TripeaksLayoutCard> cards)
        {
            layout.Infos = layout.Infos.OrderByDescending(x => x.Layer).ToList();
            cards = cards.OrderByDescending(x => x.CardInfo.Layer).ToList();
            
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.transform.SetSiblingIndex(i);
            }
        }
    }
}
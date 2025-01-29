using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    [CreateAssetMenu(fileName = "Tripeaks Layout", menuName = "Solitaire/Tripeaks/Tripeaks Layout")]
    public class TripeaksLayout : ScriptableObject
    {
        public int LayoutId;
        public bool IsDefault;
        public Sprite Preview;
        public Texture2D PreviewTex;
        public List<TripeaksCardPositionInfo> Infos;

        private TripeaksCardPositionInfo DefaultInfo = new TripeaksCardPositionInfo(0, Vector3.zero);

        public TripeaksCardPositionInfo GetInfoById(int id)
        {
            if (Infos == null)
            {
                return DefaultInfo;
            }

            TripeaksCardPositionInfo info = DefaultInfo;

            for (int i = 0; i < Infos.Count; i++)
            {
                TripeaksCardPositionInfo layoutInfo = Infos[i];

                if (Infos[i].Id == id)
                {
                    info = layoutInfo;
                    break;
                }
            }

            return info;
        }

        public TripeaksCardPositionInfo GetInfoByindex(int positionIndex)
        {
            if (Infos == null || Infos.Count - 1 < positionIndex)
            {
                return DefaultInfo;
            }

            return Infos[positionIndex];
        }

        [ContextMenu("Order by Overlaps")]
        public void OrderByOverlaps()
        {
            Infos = Infos.OrderByDescending(x => x.OverlapsId.Count).ToList();
        }


        [ContextMenu("Add +1")]
        public void CopyPos()
        {
            foreach (var item in Infos)
            {
                item.Id++;
                for (int i = 0; i < item.OverlapsId.Count; i++)
                {
                    item.OverlapsId[i]++;
                }
            }
        }

        [ContextMenu("-150")]
        public void ChangePos()
        {
            foreach (var item in Infos)
            {
                item.AnchoredPos.Y -= 150;
            }
        }

        //[ContextMenu("Copypos")]
        //public void CopyPos()
        //{
        //    foreach (var item in Infos)
        //    {
        //        item.AnchoredPos = new CardPosition(item.Position);
        //    }
        //}


        [ContextMenu("Order by Layer")]
        public void OrderByLayers()
        {
            Infos = Infos.OrderByDescending(x => x.Layer).ToList();
        }
    }
}
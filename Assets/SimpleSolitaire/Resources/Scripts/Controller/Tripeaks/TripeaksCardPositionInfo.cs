using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    [Serializable]
    public class TripeaksCardPositionInfo
    {
        public int Layer;
        public int Id;
        public CardPosition AnchoredPos;
        public List<int> OverlapsId;

        public TripeaksCardPositionInfo()
        {
            Layer = 0;
            Id = 0;
            AnchoredPos = new CardPosition(Vector3.zero);
            OverlapsId = null;
        }

        public TripeaksCardPositionInfo(int id)
        {
            Id = id;
            AnchoredPos = new CardPosition();
        }
        
        public TripeaksCardPositionInfo(int id, Vector3 position)
        {
            Id = id;
            AnchoredPos = new CardPosition(position);
        }

        public TripeaksCardPositionInfo(TripeaksCardPositionInfo info)
        {
            Layer = info.Layer;
            Id = info.Id;
            AnchoredPos = new CardPosition(info.AnchoredPos);
            OverlapsId = info.OverlapsId;
        }

        public string ToOneLineFormat => $"ID:{Id} L:{Layer} X:{AnchoredPos.X} Y:{AnchoredPos.Y}";

        public string ToInterpolatedFormat => $"ID:{Id}\nL:{Layer}\nX:{AnchoredPos.X}\nY:{AnchoredPos.Y}";
    }
}
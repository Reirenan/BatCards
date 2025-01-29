using Newtonsoft.Json;
using System;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    [Serializable]
    public class CardPosition
    {
        public float X;
        public float Y;
        public float Z;

        [JsonIgnore]
        public Vector3 VectorPos => new Vector3(X, Y, Z);
        public Vector3Int VectorPosInt => new Vector3Int((int)X, (int)Y, (int)Z);

        public CardPosition()
        {
        }

        public CardPosition(Vector2Int pos)
        {
            X = pos.x;
            Y = pos.y;
        }

        public CardPosition(Vector3 pos)
        {
            X = pos.x;
            Y = pos.y;
            Z = pos.z;
        }

        public CardPosition(CardPosition pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }
    }
}
using UnityEngine;


namespace SimpleSolitaire
{
    public class HandOrientationElement : MonoBehaviour
    {
        public RectTransform RectRoot;

        [Header("Orientation data refs:")]
        public RectTransform LeftTransformRef;
        public RectTransform RightTransformRef;
    }
}
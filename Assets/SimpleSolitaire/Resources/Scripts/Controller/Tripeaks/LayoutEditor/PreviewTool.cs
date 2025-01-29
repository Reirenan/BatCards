using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public class PreviewTool : MonoBehaviour
    {
        [SerializeField] private RectTransform _area;
        [SerializeField] private Image _areaView;

        public void MakePreview(List<TripeaksLayoutCard> cards, string path, Action<Texture2D> onPreviewCreated = null)
        {
            StartCoroutine(TakeSnapShotAndSave(cards, path, onPreviewCreated));
        }

        private IEnumerator TakeSnapShotAndSave(List<TripeaksLayoutCard> cards, string path, Action<Texture2D> onPreviewCreated = null)
        {
            _areaView.enabled = false;
            cards.ForEach(x => x.SetPreviewMode(true));

            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            Vector3[] corners = new Vector3[4];
            _area.GetWorldCorners(corners);

            int width = ((int)corners[3].x - (int)corners[0].x);
            int height = (int)corners[1].y - (int)corners[0].y;
            var startX = corners[0].x;
            var startY = corners[0].y;
            Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, false);
            preview.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
            preview.Apply();

            byte[] byteArray = preview.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, byteArray);

            cards.ForEach(x => x.SetPreviewMode(false));
            _areaView.enabled = true;

            Debug.Log("Screenshot Path : " + path);
            Destroy(preview);

            onPreviewCreated?.Invoke(preview);
        }
    }
}
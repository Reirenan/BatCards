using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    [CreateAssetMenu(fileName = "TripeaksLayoutContainer", menuName = "Tripeaks/TripeaksLayoutContainer")]
    public class TripeaksLayoutContainer : ScriptableObject
    {
        public TripeaksLayoutData CurrentLayout { get; set; }

        public List<TripeaksLayoutData> Layouts = new List<TripeaksLayoutData>();
        public HashSet<int> ActiveLayouts = new HashSet<int>();

        private const string TRIPEAKS_LAYOUTS = "TripeaksLayouts";

        public TripeaksCardPositionInfo GetInfoById(int id)
        {
            return CurrentLayout.GetInfoById(id);
        }

        public TripeaksCardPositionInfo GetInfoByIndex(int positionIndex)
        {
            return CurrentLayout.GetInfoByindex(positionIndex);
        }

        public void SetRandomLayout()
        {
            int randomId = Random.Range(0, ActiveLayouts.Count);
            int layoutId = ActiveLayouts.ElementAt(randomId);

            CurrentLayout = Layouts.FirstOrDefault(x=>x.LayoutId == layoutId);
        }

        public void SetCurrentLayout(int id)
        {
            for (int i = 0; i < Layouts.Count; i++)
            {
                if (Layouts[i].LayoutId == id)
                {
                    CurrentLayout = Layouts[i];
                    return;
                }
            }
        }

        public void SetDefaultLayout()
        {
            CurrentLayout = Layouts.FirstOrDefault(x => x.IsDefault);
        }

        public void GetLayoutsSettings()
        {
            if (PlayerPrefs.HasKey(TRIPEAKS_LAYOUTS))
            {
                string layoutsData = PlayerPrefs.GetString(TRIPEAKS_LAYOUTS);
                ActiveLayouts = JsonConvert.DeserializeObject<HashSet<int>>(layoutsData);
            }
            else
            {
                Layouts.ForEach(x => ActiveLayouts.Add(x.LayoutId));
                SaveLayouts();
            }
        }

        public void SaveLayouts()
        {
            string layoutsData = JsonConvert.SerializeObject(ActiveLayouts);
            PlayerPrefs.SetString(TRIPEAKS_LAYOUTS, layoutsData);
        }

        public TripeaksLayoutData CreateNewPuzzle()
        {
            if (Layouts == null)
            {
                Layouts = new List<TripeaksLayoutData>();
            }

            var created = new TripeaksLayoutData()
            {
                LayoutId = Layouts.Count + 1,
                IsDefault = Layouts.Count == 0, 
                IsEnabled = true, 
                Infos = new List<TripeaksCardPositionInfo>(),
                Preview = null
            };

            Layouts.Add(created);

            return created;
        }

        public TripeaksLayoutData LoadLayout(int id)
        {
            return Layouts.FirstOrDefault(x => x.LayoutId == id);
        }

        public void ClearLayoutById(int id)
        {
            var layout = LoadLayout(id);
            if (layout != null)
            {
                layout.Infos = new List<TripeaksCardPositionInfo>();
            }
        }

        public bool IsActiveLayout(int id) => ActiveLayouts.Contains(id);
        public bool HasOneOrLessLayout() => ActiveLayouts.Count <= 1;

        public bool RemoveLayout(int id) => ActiveLayouts.Remove(id);
        public bool AddLayout(int id) => ActiveLayouts.Add(id);
    }
}
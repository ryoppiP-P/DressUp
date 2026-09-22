using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GroupTabs : MonoBehaviour {
    [System.Serializable]
    public class Tab {
        public Button button;
        public CategoryGroup group;
    }

    [SerializeField] private List<Tab> tabs;
    [SerializeField] private SubCategoryTabs subTabs;

    void Start() {
        foreach (var t in tabs) {
            var captured = t;
            captured.button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Click);
                OnClicked(captured);
            });
        }
        if (tabs.Count > 0) OnClicked(tabs[0]);
    }

    void OnClicked(Tab tab) {
        // 選択タブを手前に出す
        tab.button.transform.SetAsLastSibling();
        subTabs.Build(tab.group);
    }
}

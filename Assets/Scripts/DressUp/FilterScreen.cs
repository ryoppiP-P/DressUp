using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FilterScreen : MonoBehaviour {
    [SerializeField] private DressupGrid grid;

    [SerializeField] private SortToggleGroup sortGroup;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private List<RarityToggle> rarityToggles;
    [SerializeField] private List<ColorToggle> colorToggles;
    [SerializeField] private List<YearToggle> yearToggles;

    [SerializeField] private Button applyButton;
    [SerializeField] private Button clearButton;

    void Start() {
        applyButton.onClick.AddListener(Apply);
        clearButton.onClick.AddListener(Clear);
    }

    // フィルターの適用
    void Apply() {
        var cond = new FilterCondition();

        cond.sort = sortGroup.Current;
        cond.nameKeyword = nameInput.text;

        foreach (var t in rarityToggles)
            if (t.Toggle.isOn) cond.rarities.Add(t.rarity);

        foreach (var t in colorToggles)
            if (t.Toggle.isOn) cond.colors.Add(t.color);

        foreach (var t in yearToggles)
            if (t.Toggle.isOn) cond.releaseYears.Add(t.year);

        grid.ApplyFilter(cond);
    }

    // クリア（全部リセット）。押した時点で一覧にも反映する（「適用」を押さなくてよい）
    void Clear() {
        nameInput.text = "";

        // 絞り込みチェックは全部OFFに（= 集合が空 = 全部表示）
        foreach (var t in rarityToggles) t.Toggle.isOn = false;
        foreach (var t in colorToggles) t.Toggle.isOn = false;
        foreach (var t in yearToggles) t.Toggle.isOn = false;

        // 並べ替えは既定（入手順 新しい）に戻す
        foreach (var st in sortGroup.GetComponentsInChildren<SortToggle>())
            st.Toggle.isOn = (st.option == SortOption.AcquiredNew);

        // 戻した内容をそのまま一覧へ反映する。
        // Apply() は今のUIを読み直すだけなので、ここでは条件が空になった状態が渡る。
        Apply();
    }
}

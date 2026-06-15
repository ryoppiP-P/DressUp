//
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class SubCategoryButton : MonoBehaviour {
    public FilterKind kind;            // All / Equipped / Category
    public CategoryType category;      // kind == Category のときだけ使う
    public CategoryGroup[] groups;     // このボタンを表示する大分類（複数可）

    private Button _button;
    private CanvasGroup _canvasGroup;

    public Button Button {
        get {
            if (_button == null) _button = GetComponent<Button>();
            return _button;
        }
    }

    public CanvasGroup CanvasGroup {
        get {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }

    public void SetAlpha(float alpha) {
        CanvasGroup.alpha = alpha;
    }
}

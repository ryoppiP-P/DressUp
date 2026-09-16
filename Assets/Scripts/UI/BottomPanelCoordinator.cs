using System;

// ボトムバーから開くパネル(Shop/Mission/Gacha等)の排他制御。
// 「開く」たびに、直前に開いていた別のパネルがあればそれを閉じる。
public static class BottomPanelCoordinator {
    private static Action _closeCurrent;

    // いずれかのパネルが開いている/いないが変わった時に呼ばれる(例: ButtonArrowの表示切り替え)
    public static event Action<bool> OnAnyOpenChanged;

    // 自分を開く直前に呼ぶ。自分以外が開いていればそれを閉じる。
    public static void NotifyOpened(Action closeSelf) {
        if (_closeCurrent != null && !_closeCurrent.Equals(closeSelf)) _closeCurrent();
        _closeCurrent = closeSelf;
        OnAnyOpenChanged?.Invoke(true);
    }

    // 自分を閉じた時に呼ぶ。今開いている記録が自分なら消す。
    public static void NotifyClosed(Action closeSelf) {
        if (_closeCurrent == null || !_closeCurrent.Equals(closeSelf)) return;
        _closeCurrent = null;
        OnAnyOpenChanged?.Invoke(false);
    }
}

using System;

// ボトムバーから開くパネル(Shop/Mission/Gacha等)の排他制御。
// 「開く」たびに、直前に開いていた別のパネルがあればそれを閉じる。
public static class BottomPanelCoordinator {
    private static Action _closeCurrent;

    // 自分を開く直前に呼ぶ。自分以外が開いていればそれを閉じる。
    public static void NotifyOpened(Action closeSelf) {
        if (_closeCurrent != null && !_closeCurrent.Equals(closeSelf)) _closeCurrent();
        _closeCurrent = closeSelf;
    }

    // 自分を閉じた時に呼ぶ。今開いている記録が自分なら消す。
    public static void NotifyClosed(Action closeSelf) {
        if (_closeCurrent != null && _closeCurrent.Equals(closeSelf)) _closeCurrent = null;
    }
}

//==============================================================================
//  File   : SaveManager.cs
//  Brief  : セーブ機能の管理（セーブデータ読み込み/書き込み・初期ロード）
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//  セーブデータの初期適応は SaveApplier に任せる。
//==============================================================================
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    private readonly Dictionary<string, EquipState> _equipStates = new();

    [Header("Debug")]
    [SerializeField] private bool useEncryption = true;
    [SerializeField] private bool verboseLog = false;

    private const string FileName = "save.dat";
    private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    // 現在メモリ上のデータ（アクセス用）
    public SaveData Current { get; private set; }

    // セーブデータの変更を通知するイベント（コイン等の変化をUIに反映させるために使う）
    public event Action OnCurrencyChanged;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 起動時に自動ロード（無ければ新規）
        Load();
    }

    // ===== 公開API =====

    /// <summary>手動セーブ（メニュー等から）</summary>
    public void SaveManual() {
        WriteToDisk();
        if (verboseLog) Debug.Log("[SaveManager] Manual save");
    }

    /// <summary>オートセーブ（チェックポイント等から呼ぶ）</summary>
    public void SaveAuto() {
        WriteToDisk();
        if (verboseLog) Debug.Log("[SaveManager] Auto save");
    }

    /// <summary>ロード（起動時に呼ばれるが、外部から再ロードしたいときも使える）</summary>
    public void Load() {
        if (!SaveStorage.Exists(SavePath)) {
            Current = new SaveData();
            if (verboseLog) Debug.Log("[SaveManager] セーブデータなし → 新規作成");
            SaveApplier.ApplyAll();
            return;
        }

        try {
            string raw = SaveStorage.Read(SavePath);
            string json = useEncryption ? SaveCrypto.Decrypt(raw) : raw;
            Current = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            if (verboseLog) Debug.Log($"[SaveManager] Loaded\n{json}");
        } catch (Exception e) {
            Debug.LogError($"[SaveManager] ロード失敗: {e.Message} → 新規作成");
            Current = new SaveData();
        }
        SaveApplier.ApplyAll();
    }

    /// <summary>セーブデータ削除（タイトル画面の「最初から」等で）</summary>
    public void DeleteSave() {
        SaveStorage.Delete(SavePath);
        Current = new SaveData();
    }

    public bool HasSaveData() => SaveStorage.Exists(SavePath);

    // ===== 内部 =====

    private void WriteToDisk() {
        if (Current == null) Current = new SaveData();
        string json = JsonUtility.ToJson(Current, false);
        string output = useEncryption ? SaveCrypto.Encrypt(json) : json;
        SaveStorage.Write(SavePath, output);
        if (verboseLog) Debug.Log($"[SaveManager] Saved to {SavePath}");
    }


    // ===== 便利メソッド（よく使う操作のショートカット） =====

    // キャラIDごとの現在装備を取得（無ければ作る）
    public EquipState GetEquipState(string characterId) {
        if (!_equipStates.TryGetValue(characterId, out var state) || state == null) {
            state = new EquipState();
            _equipStates[characterId] = state;
        }
        return state;
    }

    // ===== 通貨 =====
    public int GetCurrency(CurrencyType type) {
        if (Current == null) return 0;
        var p = Current.playerData;
        return type switch {
            CurrencyType.Nut => p.nutCurrency,
            CurrencyType.Honey => p.honeyCurrency,
            _ => 0,
        };
    }

    // 加算（マイナスを渡せば減算にも使える）
    public void AddCurrency(CurrencyType type, int amount) {
        if (Current == null) Current = new SaveData();
        var p = Current.playerData;
        switch (type) {
            case CurrencyType.Nut: p.nutCurrency = Mathf.Max(0, p.nutCurrency + amount); break;
            case CurrencyType.Honey: p.honeyCurrency = Mathf.Max(0, p.honeyCurrency + amount); break;
        }
        SaveAuto();                  // 変更を保存（オートセーブ扱い）
        OnCurrencyChanged?.Invoke(); // UIへ通知
    }

    // 支払い（足りれば減らして true、足りなければ false）
    public bool TrySpendCurrency(CurrencyType type, int cost) {
        if (GetCurrency(type) < cost) return false;
        AddCurrency(type, -cost);
        return true;
    }


    // ===== キャラ名 =====

    public string GetCharacterName(string characterId) {
        if (Current == null) return "";
        return Current.dressUp.GetOrCreate(characterId).characterName;
    }

    public void SetCharacterName(string characterId, string name) {
        if (Current == null) Current = new SaveData();
        Current.dressUp.GetOrCreate(characterId).characterName = name;
        SaveAuto();
    }


    // ===== 所持アイテム =====
    // ガチャの排出やショップの購入で手に入れたアイテムを itemId で記録する。
    // アイテム一覧画面はこの記録を見て「今まで集めたアイテム」を表示する。

    /// <summary>そのアイテムを所持しているか</summary>
    public bool IsItemOwned(string itemId) {
        if (Current == null || string.IsNullOrEmpty(itemId)) return false;
        return Current.itemData.ownedItemIds.Contains(itemId);
    }

    /// <summary>そのアイテムを所持しているか（初期所持アイテムは常に true）</summary>
    public bool IsItemOwned(GameItem item) {
        if (item == null) return false;
        if (item.ownedByDefault) return true;
        return IsItemOwned(item.itemId);
    }

    /// <summary>アイテムを所持リストに追加する（既に持っていれば何もしない）</summary>
    public void AddOwnedItem(string itemId) {
        if (string.IsNullOrEmpty(itemId)) return;
        if (Current == null) Current = new SaveData();
        if (Current.itemData.ownedItemIds.Contains(itemId)) return;

        Current.itemData.ownedItemIds.Add(itemId);
        SaveAuto();
    }

    // ===== 親密度 =====
    // 2人1組で管理する。どちらを先に渡しても同じ組として扱う。

    private IntimacyEntry FindIntimacyEntry(string idA, string idB) {
        if (Current == null) return null;
        foreach (var e in Current.intimacyData.entries) {
            if ((e.charaIdA == idA && e.charaIdB == idB) || (e.charaIdA == idB && e.charaIdB == idA))
                return e;
        }
        return null;
    }

    /// <summary>2人の間の親密度を取得する(未記録なら0)</summary>
    public int GetIntimacy(string idA, string idB) {
        var e = FindIntimacyEntry(idA, idB);
        return e != null ? e.value : 0;
    }

    /// <summary>
    /// 2人の間の親密度を加算する(0-100にクランプ)。
    /// immediateSave を false にすると、値はメモリ上だけ更新してディスクへは書き込まない。
    /// (同じ建物に一緒にいる間など、毎フレーム呼ぶような加算で毎回書き込むと重いため)
    /// </summary>
    public void AddIntimacy(string idA, string idB, int delta, bool immediateSave = true) {
        if (Current == null || string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB) || idA == idB) return;

        var e = FindIntimacyEntry(idA, idB);
        if (e == null) {
            e = new IntimacyEntry { charaIdA = idA, charaIdB = idB, value = 0 };
            Current.intimacyData.entries.Add(e);
        }
        e.value = Mathf.Clamp(e.value + delta, 0, 100);

        if (immediateSave) SaveAuto();
    }

    /// <summary>AddIntimacy(immediateSave: false)でメモリ上に貯まった変更をまとめてディスクへ保存する</summary>
    public void FlushIntimacySave() {
        SaveAuto();
    }
}

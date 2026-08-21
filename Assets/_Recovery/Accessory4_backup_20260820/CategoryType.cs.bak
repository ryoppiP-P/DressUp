public enum CategoryGroup {
    Fashion,
    Accessory,
    Face,
}

public enum FilterKind {
    All,        // その大分類の全部
    Equipped,   // 着用中
    Category,   // 特定の小分類
}

public enum CategoryType {
    BackGround,
    Body,
    HairAll,
    HairFront,
    HairBack,
    Tops,
    Dress,
    Bottoms,
    Outers,
    Shoes,
    HeadAccessory,
    GlassesAccessory,
    BodyAccessory,
    FaceEyes,
    FaceMouth,
}

public enum CharaState {
    Idle = 0,       // 立っている
    Walk = 1,       // 歩く
    Sit = 2,        // 座る
    Sitting = 3,    // 座っている
    Study = 4,      // 勉強する
    CloseBook = 5,      // 本をしまう
    CloseTools = 6,   // 勉強道具をしまう（座りながら）
    OpenTools = 7,   // 勉強道具を出す（座りながら）
    Reading = 8,    // 読書する
    Standup = 9,    // 立ち上がる
    Play = 10,      // 公園で遊ぶ
    OpenBook = 11,   // 本を開く
}

public enum Rarity {
    Normal,
    Rare,
    SuperRare,
}

public static class CategoryMap {
    public static CategoryType[] GetCategories(CategoryGroup group) {
        switch (group) {
            case CategoryGroup.Fashion:
                return new[] {
                    CategoryType.HairAll, CategoryType.HairFront,
                    CategoryType.HairBack, CategoryType.Tops,
                    CategoryType.Dress, CategoryType.Bottoms,
                    CategoryType.Outers, CategoryType.Shoes,
                };
            case CategoryGroup.Accessory:
                return new[] {
                    CategoryType.HeadAccessory, CategoryType.GlassesAccessory,
                    CategoryType.BodyAccessory
                };
            case CategoryGroup.Face:
                return new[] {
                    CategoryType.FaceEyes, CategoryType.FaceMouth
                };
            default:
                return new CategoryType[0];
        }
    }
}
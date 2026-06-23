public enum CategoryGroup {
    Fashion,
    Accessory,
    Face,
}

public enum FilterKind {
    All,        // ÇªÇÃëÂï™óﬁÇÃëSïî
    Equipped,   // íÖópíÜ
    Category,   // ì¡íËÇÃè¨ï™óﬁ
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
    Idle,
    Walk,
    Sit,
    Study,
    Reading,
    Jump,
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
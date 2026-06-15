using UnityEngine;

public class OutfitCapture : MonoBehaviour {
    [SerializeField] private Camera captureCamera; // キャラだけ写す専用カメラ
    [SerializeField] private int size = 256;       // サムネ解像度

    // 現在のキャラを1枚のSpriteに焼いて返す
    public Sprite Capture() {
        var rt = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32);

        captureCamera.targetTexture = rt;
        captureCamera.enabled = false; // 念のため画面出力させない
        captureCamera.Render();        // 手動で1回だけ描画

        RenderTexture.active = rt;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        tex.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}

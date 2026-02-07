using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 플레이스홀더 스프라이트 자동 생성 에디터 메뉴.
/// </summary>
public static class CreatePlaceholderSprites
{
    private const int CharacterWidth = 400;
    private const int CharacterHeight = 800;
    private const int BackgroundWidth = 1080;
    private const int BackgroundHeight = 1920;

    [MenuItem("LoveSimulation/Create Placeholder Sprites")]
    public static void CreateSprites()
    {
        // 폴더 생성
        string characterPath = "Assets/Resources/CharacterSprites";
        string backgroundPath = "Assets/Resources/Backgrounds";

        if (!Directory.Exists(characterPath))
        {
            Directory.CreateDirectory(characterPath);
        }
        if (!Directory.Exists(backgroundPath))
        {
            Directory.CreateDirectory(backgroundPath);
        }

        // 캐릭터 스프라이트 생성
        CreateCharacterSprite("me_neutral", new Color(0.3f, 0.5f, 0.8f), "ME");
        CreateCharacterSprite("adelin_neutral", new Color(0.9f, 0.4f, 0.5f), "Adelin");
        CreateCharacterSprite("adelin_smile", new Color(0.95f, 0.5f, 0.55f), "Adelin :)");
        CreateCharacterSprite("adelin_sad", new Color(0.7f, 0.3f, 0.4f), "Adelin :(");
        CreateCharacterSprite("duke_neutral", new Color(0.4f, 0.4f, 0.5f), "Duke");
        CreateCharacterSprite("leon_neutral", new Color(0.6f, 0.7f, 0.4f), "Leon");
        CreateCharacterSprite("princess_neutral", new Color(0.8f, 0.6f, 0.9f), "Princess");

        // 배경 스프라이트 생성
        CreateBackgroundSprite("castle_hall", new Color(0.2f, 0.15f, 0.25f), "Castle Hall");
        CreateBackgroundSprite("garden", new Color(0.2f, 0.4f, 0.2f), "Garden");
        CreateBackgroundSprite("bedroom", new Color(0.25f, 0.2f, 0.15f), "Bedroom");

        AssetDatabase.Refresh();
        Debug.Log("[CreatePlaceholderSprites] 플레이스홀더 스프라이트 생성 완료!");
    }

    private static void CreateCharacterSprite(string name, Color baseColor, string label)
    {
        string path = $"Assets/Resources/CharacterSprites/{name}.png";
        if (File.Exists(path))
        {
            return;
        }

        Texture2D texture = new Texture2D(CharacterWidth, CharacterHeight);

        // 배경색 채우기
        Color[] pixels = new Color[CharacterWidth * CharacterHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = baseColor;
        }
        texture.SetPixels(pixels);

        // 테두리 그리기
        DrawBorder(texture, Color.white, 4);

        // 간단한 얼굴 형태 그리기 (원)
        DrawCircle(texture, CharacterWidth / 2, CharacterHeight - 150, 80, Color.white);

        // 몸통 (사각형)
        DrawRect(texture, CharacterWidth / 2 - 100, CharacterHeight - 350, 200, 300,
                 new Color(baseColor.r * 0.8f, baseColor.g * 0.8f, baseColor.b * 0.8f));

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(texture);

        // Sprite 설정
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }
    }

    private static void CreateBackgroundSprite(string name, Color baseColor, string label)
    {
        string path = $"Assets/Resources/Backgrounds/{name}.png";
        if (File.Exists(path))
        {
            return;
        }

        Texture2D texture = new Texture2D(BackgroundWidth, BackgroundHeight);

        // 그라데이션 배경
        for (int y = 0; y < BackgroundHeight; y++)
        {
            float t = (float)y / BackgroundHeight;
            Color color = Color.Lerp(baseColor * 0.7f, baseColor * 1.2f, t);
            for (int x = 0; x < BackgroundWidth; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        // 테두리
        DrawBorder(texture, new Color(1f, 1f, 1f, 0.3f), 8);

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(texture);

        // Sprite 설정
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }
    }

    private static void DrawBorder(Texture2D texture, Color color, int thickness)
    {
        int w = texture.width;
        int h = texture.height;

        for (int i = 0; i < thickness; i++)
        {
            // 상단
            for (int x = 0; x < w; x++) texture.SetPixel(x, h - 1 - i, color);
            // 하단
            for (int x = 0; x < w; x++) texture.SetPixel(x, i, color);
            // 좌측
            for (int y = 0; y < h; y++) texture.SetPixel(i, y, color);
            // 우측
            for (int y = 0; y < h; y++) texture.SetPixel(w - 1 - i, y, color);
        }
    }

    private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                int px = x + dx;
                int py = y + dy;
                if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }
}

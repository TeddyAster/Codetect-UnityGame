using UnityEngine;

public class CodeBlockShape : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void GenerateBlockShape()
    {
        Texture2D texture = new Texture2D(200, 100);
        Color[] colors = new Color[200 * 100];

        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 200; x++)
            {
                if (y > 80 && (x < 40 || x > 160))
                {
                    colors[x + y * 200] = Color.clear;
                }
                else
                {
                    colors[x + y * 200] = Color.green;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 200, 100), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
}

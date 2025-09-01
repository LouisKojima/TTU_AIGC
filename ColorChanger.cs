using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Test class for generating textures by editing pixels.
/// </summary>
public class ColorChanger : MonoBehaviour
{
    //Only Rawimage can be edited, not Image
    private RawImage image;

    private int textureWidth = 128;
    private int textureHeight = 128;

    [Range(0,360)]
    public int hue = 0;

    private void Reset()
    {
        Start();
    }

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        float huef = hue / 360f;

        var texture = new Texture2D(textureWidth, textureHeight);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        for (int s = 0; s < textureWidth; s++)
        {
            Color[] colors = new Color[textureHeight];
            for (int v = 0; v < textureHeight; v++)
            {
                colors[v] = Color.HSVToRGB(huef, (float)s / textureWidth, (float)v / textureHeight);
            }
            texture.SetPixels(s, 0, 1, textureHeight, colors);
        }
        texture.Apply();

        image.texture = texture;
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GradientBG : MonoBehaviour
{
    public RawImage target;
    public Gradient gradient;
    public Vector2Int resolution = new(128, 4);

    [Button]
    public void ApplyGradient()
    {
        var texture = new Texture2D(resolution.x, resolution.y);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        for (int Oy = 0; Oy < resolution.y; Oy++)
        {
            Color[] row = new Color[resolution.x];
            //Generate pixels
            row = row.Select((c, Ox) =>
            {
                return gradient.Evaluate((float)Ox / resolution.x);
            }).ToArray();
            texture.SetPixels(0, Oy, resolution.x, 1, row);
        }
        //Make transparent in editor
#if UNITY_EDITOR
        texture.alphaIsTransparency = true;
#endif
        texture.Apply();

        target.texture = texture;
    }

    private void Start()
    {
        ApplyGradient();
    }
}

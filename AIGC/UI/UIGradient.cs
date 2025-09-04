// UIGradient.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    public Color top = new Color(0.65f, 0.4f, 1f, 1f);    // 顶部色（默认紫）
    public Color bottom = new Color(0.3f, 0.1f, 0.6f, 1f); // 底部色

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        // 找到最上/最下的 y 用于插值
        float minY = float.MaxValue, maxY = float.MinValue;
        UIVertex v = default;
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            minY = Mathf.Min(minY, v.position.y);
            maxY = Mathf.Max(maxY, v.position.y);
        }
        float range = Mathf.Max(0.0001f, maxY - minY);

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            float t = (v.position.y - minY) / range; // 0..1 自下而上
            v.color = Color.Lerp(bottom, top, t);
            vh.SetUIVertex(v, i);
        }
    }
}

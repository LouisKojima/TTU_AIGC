using UnityEngine;

/// <summary>
/// 运行时在平面上生成实时反射贴图，供 Built-In 渲染管线使用。
/// 将脚本挂在需要反射的平面（Quad / Plane）对象上。
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlanarReflection : MonoBehaviour
{
    [Header("Main / Source Camera")]
    [Tooltip("若留空，则自动寻找 Camera.main")]  
    public Camera sourceCamera;

    [Header("Reflection Settings")]
    [Tooltip("哪些层会被渲入倒影")]  
    public LayerMask reflectionMask = ~0;
    [Range(0.1f, 1f)]
    [Tooltip("RenderTexture 分辨率缩放 (越低性能越好)")]  
    public float textureScale = 0.5f;

    [Header("Material")]
    [Tooltip("材质中用于接收反射贴图的属性名")]  
    public string textureProperty = "_RefTexture";

    [Header("Distance Blur (Mipmaps)")]
    [Tooltip("启用基于距离的模糊效果（使用反射 RT 的 Mipmap 与三线性过滤）")]
    public bool enableDistanceBlur = true;
    [Tooltip("对采样的全局 Mip 偏移。>0 更模糊，<0 更锐利。")]
    [Range(-2f, 2f)] public float mipMapBias = 0f;
    [Tooltip("反射贴图过滤模式（建议 Trilinear）")]
    public FilterMode filterMode = FilterMode.Trilinear;
    [Tooltip("各向异性等级（0-16）。较低值在远处更模糊，较高值更清晰。")]
    [Range(0, 16)] public int anisoLevel = 0;

    Camera _reflectionCamera;
    RenderTexture _reflectionRT;
    Renderer _renderer;
    static readonly int _shaderPropID = Shader.PropertyToID("_RefTexture");

    const string REFLECTION_CAM_NAME = "__PlanarReflectionCamera__";

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (sourceCamera == null) sourceCamera = Camera.main;
        CreateObjects();
    }

    void OnEnable()
    {
        if (_reflectionCamera) _reflectionCamera.enabled = true;
    }

    void OnDisable()
    {
        if (_reflectionCamera) _reflectionCamera.enabled = false;
    }

    void LateUpdate()
    {
        if (!sourceCamera || !_reflectionCamera) return;

        UpdateCameraProperties();
        _reflectionCamera.Render();

        // 如果已启用自动 Mip 生成，则无需手动生成
        if (enableDistanceBlur && _reflectionRT && _reflectionRT.useMipMap && _reflectionRT.autoGenerateMips)
        {
            return;
        }

        // 生成 Mipmap，便于距离越远采样更高 Mip，形成自然的远处模糊
        if (enableDistanceBlur && _reflectionRT && _reflectionRT.useMipMap)
        {
            try { _reflectionRT.GenerateMips(); } catch { /* 某些平台会自动生成 */ }
        }
    }

    #region Setup
    void CreateObjects()
    {
        if (!_reflectionCamera)
        {
            GameObject go = new GameObject(REFLECTION_CAM_NAME);
            go.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera = go.AddComponent<Camera>();
            _reflectionCamera.enabled = false;
        }

        UpdateRenderTexture();
        _reflectionCamera.targetTexture = _reflectionRT;
        _reflectionCamera.cullingMask = reflectionMask;
        _reflectionCamera.clearFlags = sourceCamera.clearFlags;
        _reflectionCamera.backgroundColor = sourceCamera.backgroundColor;
        _renderer.sharedMaterial.SetTexture(textureProperty, _reflectionRT);
    }

    void UpdateRenderTexture()
    {
        int width = Mathf.FloorToInt(Screen.width * textureScale);
        int height = Mathf.FloorToInt(Screen.height * textureScale);
        if (_reflectionRT && (_reflectionRT.width != width || _reflectionRT.height != height || _reflectionRT.useMipMap != enableDistanceBlur))
        {
            _reflectionRT.Release();
            DestroyImmediate(_reflectionRT);
            _reflectionRT = null;
        }
        if (_reflectionRT == null)
        {
            _reflectionRT = new RenderTexture(width, height, 16, RenderTextureFormat.DefaultHDR)
            {
                name = "PlanarReflectionRT",
                hideFlags = HideFlags.HideAndDontSave,
                useMipMap = enableDistanceBlur,
                autoGenerateMips = enableDistanceBlur
            };
            _reflectionRT.wrapMode = TextureWrapMode.Clamp;
            _reflectionRT.filterMode = filterMode;
            _reflectionRT.anisoLevel = anisoLevel;
            _reflectionRT.mipMapBias = mipMapBias;
        }
        else
        {
            // 同步运行时可变参数
            _reflectionRT.filterMode = filterMode;
            _reflectionRT.anisoLevel = anisoLevel;
            _reflectionRT.mipMapBias = mipMapBias;
        }
    }
    #endregion

    void OnValidate()
    {
        // 在编辑器中参数变更时，同步更新 RT 配置
        if (!Application.isPlaying)
        {
            _renderer = GetComponent<Renderer>();
        }
        UpdateRenderTexture();
        if (_renderer && _reflectionRT)
        {
            _renderer.sharedMaterial.SetTexture(textureProperty, _reflectionRT);
        }
    }

    #region Camera Update
    void UpdateCameraProperties()
    {
        // 计算平面
        Vector3 pos = transform.position;
        Vector3 normal = transform.up; // 平面法线 (对象 +Y)

        // 反射矩阵
        Matrix4x4 reflectionMat = CalculateReflectionMatrix(normal, pos);

        // 更新位置与方向
        Vector3 srcPos = sourceCamera.transform.position;
        Vector3 newPos = reflectionMat.MultiplyPoint(srcPos);
        _reflectionCamera.transform.position = newPos;

        Vector3 srcDir = sourceCamera.transform.forward;
        Vector3 newDir = reflectionMat.MultiplyVector(srcDir);
        _reflectionCamera.transform.rotation = Quaternion.LookRotation(newDir, reflectionMat.MultiplyVector(sourceCamera.transform.up));

        // 同步相机参数
        _reflectionCamera.fieldOfView = sourceCamera.fieldOfView;
        _reflectionCamera.aspect = sourceCamera.aspect;
        _reflectionCamera.nearClipPlane = sourceCamera.nearClipPlane;
        _reflectionCamera.farClipPlane = sourceCamera.farClipPlane;

        // 设置斜裁剪平面以避免渲到面后
        Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, pos, normal, 1.0f);
        Matrix4x4 proj = sourceCamera.CalculateObliqueMatrix(clipPlane);
        _reflectionCamera.projectionMatrix = proj;
    }

    static Matrix4x4 CalculateReflectionMatrix(Vector3 normal, Vector3 pos)
    {
        float d = -Vector3.Dot(normal, pos);
        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = 1 - 2 * plane.x * plane.x;
        m.m01 = -2 * plane.x * plane.y;
        m.m02 = -2 * plane.x * plane.z;
        m.m03 = -2 * plane.x * plane.w;

        m.m10 = -2 * plane.y * plane.x;
        m.m11 = 1 - 2 * plane.y * plane.y;
        m.m12 = -2 * plane.y * plane.z;
        m.m13 = -2 * plane.y * plane.w;

        m.m20 = -2 * plane.z * plane.x;
        m.m21 = -2 * plane.z * plane.y;
        m.m22 = 1 - 2 * plane.z * plane.z;
        m.m23 = -2 * plane.z * plane.w;

        return m;
    }

    static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * 0.07f; // 防止裁剪伪影
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cPos = m.MultiplyPoint(offsetPos);
        Vector3 cNormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
    }
    #endregion

    void OnDestroy()
    {
        if (_reflectionRT)
        {
            _reflectionRT.Release();
            DestroyImmediate(_reflectionRT);
        }
        if (_reflectionCamera)
        {
            DestroyImmediate(_reflectionCamera.gameObject);
        }
    }
}

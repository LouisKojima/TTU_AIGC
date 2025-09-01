using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformVisibility : MonoBehaviour
{
    [System.Flags]
    public enum Platforms 
    {
        Windows = 1 <<1,
        Android = 1<<2,
        All = Windows | Android
    }
    [ToggleLeft]
    public bool visibleOn = false;
    [EnumToggleButtons, HideLabel, EnableIf(nameof(visibleOn))]
    public Platforms visiblePlatforms;
    [ToggleLeft]
    public bool invisibleOn = false;
    [EnumToggleButtons, HideLabel, EnableIf(nameof(invisibleOn))]
    public Platforms invisiblePlatforms;

    [ShowInInspector,DisplayAsString]
    [InfoBox("Conflicted!",nameof(conflict), InfoMessageType =InfoMessageType.Warning)]
    private bool conflict => visibleOn && invisibleOn && visiblePlatforms != 0 && invisiblePlatforms != 0 && (visiblePlatforms & invisiblePlatforms) != 0;

    [Button]
    private void Awake()
    {
#if UNITY_ANDROID
        if (visiblePlatforms.HasFlag(Platforms.Android))
        {
            gameObject.SetActive(true);
        }
        if (invisiblePlatforms.HasFlag(Platforms.Android))
        {
            gameObject.SetActive(false);
        }
#else
        if (visiblePlatforms.HasFlag(Platforms.Windows))
        {
            gameObject.SetActive(true);
        }
        if (invisiblePlatforms.HasFlag(Platforms.Windows))
        {
            gameObject.SetActive(false);
        }
#endif
    }
}

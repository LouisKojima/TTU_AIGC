using System;
using UnityEngine;

namespace IAV.AIGC
{
    // 全局 AIGC 生成状态（未生成/生成中/已生成）
    public enum AIGCCreateState
    {
        NotGenerated = 0,
        Generating = 1,
        Generated = 2
    }
}

namespace IAV.AIGC.UI
{
    // 挂载到 SkyboxPopup 上：根据全局状态切换不同 Button 的显隐
    public class SkyboxPopupStateController : MonoBehaviour
    {
        [Header("State (Inspector 实时可调)")]
        [SerializeField] private IAV.AIGC.AIGCCreateState _stateInInspector = IAV.AIGC.AIGCCreateState.NotGenerated;

        [Header("State -> Button Mapping (Inspector 配置)")]
        [SerializeField] private GameObject _notGeneratedButton;
        [SerializeField] private GameObject _generatingButton;
        [SerializeField] private GameObject _generatedButton;

        // 全局状态（其他脚本可直接读取）
        public static IAV.AIGC.AIGCCreateState CurrentState { get; private set; } = IAV.AIGC.AIGCCreateState.NotGenerated;

        // 状态变更事件（实例用于订阅以更新 UI）
        public static event Action<IAV.AIGC.AIGCCreateState> StateChanged;

        private void OnEnable()
        {
            StateChanged += OnGlobalStateChanged;
            // 激活时同步 Inspector 字段与当前全局状态，并刷新一次
            _stateInInspector = CurrentState;
            ApplyState(CurrentState);
        }

        private void OnDisable()
        {
            StateChanged -= OnGlobalStateChanged;
        }

        private void OnGlobalStateChanged(IAV.AIGC.AIGCCreateState state)
        {
            ApplyState(state);
            // 反向同步到 Inspector 字段，便于观察当前状态
            _stateInInspector = state;
        }

        // 全局切换状态的接口（其他脚本直接调用）
        public static void SetState(IAV.AIGC.AIGCCreateState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            var handler = StateChanged;
            if (handler != null) handler.Invoke(newState);
        }

        // 便于在 Inspector 的 Button.OnClick() 里直接调用
        public void SetNotGenerated() => SetState(IAV.AIGC.AIGCCreateState.NotGenerated);
        public void SetGenerating()   => SetState(IAV.AIGC.AIGCCreateState.Generating);
        public void SetGenerated()    => SetState(IAV.AIGC.AIGCCreateState.Generated);

        // 根据状态切换 Button：新状态对应按钮 active，其余 inactive
        private void ApplyState(IAV.AIGC.AIGCCreateState state)
        {
            if (_notGeneratedButton) _notGeneratedButton.SetActive(false);
            if (_generatingButton)   _generatingButton.SetActive(false);
            if (_generatedButton)    _generatedButton.SetActive(false);

            switch (state)
            {
                case IAV.AIGC.AIGCCreateState.NotGenerated:
                    if (_notGeneratedButton) _notGeneratedButton.SetActive(true);
                    break;
                case IAV.AIGC.AIGCCreateState.Generating:
                    if (_generatingButton) _generatingButton.SetActive(true);
                    break;
                case IAV.AIGC.AIGCCreateState.Generated:
                    if (_generatedButton) _generatedButton.SetActive(true);
                    break;
            }
        }

#if UNITY_EDITOR
        // 开发期提醒：避免一个 Button 被配置到多个状态
        private void OnValidate()
        {
            // 在编辑器中，Inspector 值变更时实时应用到全局状态和 UI
            // 仅当物体在层级中激活时才驱动 UI，避免误改隐藏对象
            if (isActiveAndEnabled)
            {
                // 在编辑模式和运行时都可响应（运行时也会触发 OnValidate）
                SetState(_stateInInspector);
                ApplyState(_stateInInspector);
            }

            if (_notGeneratedButton && (_notGeneratedButton == _generatingButton || _notGeneratedButton == _generatedButton))
            {
                Debug.LogWarning("[SkyboxPopupStateController] 同一 Button 被分配给多个状态，请确保一对一映射。");
            }
            if (_generatingButton && _generatingButton == _generatedButton)
            {
                Debug.LogWarning("[SkyboxPopupStateController] 同一 Button 被分配给多个状态，请确保一对一映射。");
            }
        }
#endif
    }
}

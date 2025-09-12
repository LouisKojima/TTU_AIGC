using System;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using IAV.AIGC.API; // BlockadeLabsSkyboxApi

// Bridge: wires ChineseRecognizer (ASR) -> Skybox generation (BlockadeLabs)
// - Realtime: mirrors recognizer.receivedMsg.result to a TMP_Text for live preview
// - Generate: uses the current text as prompt and calls the provided callback (AIGCManager or SkyboxApi wrapper)
namespace IAV.AIGC.Bridge
{
    public class AIGCVoiceBridge : MonoBehaviour
    {
        [Header("ASR Source")]
        [Required, SerializeField] private ASRFromBaidu recognizer;

        [Header("UI Bindings")]
        [Required, SerializeField] private TMP_Text asrText; // Introduction/ASR input/ASR_Text

        [Header("Generate Hook")]
        [Tooltip("Optional: If assigned, bridge calls this action with the current prompt when Generate is clicked.")]
        public Action<string> OnGeneratePrompt;

        [Header("Direct API (Simplest)")]
        [Tooltip("Drag your BlockadeLabsSkyboxApi here to call CreateSkybox(prompt) directly.")]
        [SerializeField] private BlockadeLabsSkyboxApi skyboxApi;

        [InfoBox("Button can call GenerateFromAsr() directly when SkyboxApi is assigned; no extra wiring needed. Alternatively, use GenerateWithTarget to reflect to CreateSkybox(string).", InfoMessageType.Info)]
        [SerializeField] private bool setGeneratingStateOnClick = true;

        private string lastShown = null;

        // Expose current prompt for buttons to query
        public string CurrentPrompt { get; private set; } = string.Empty;

        void Update()
        {
            if (recognizer == null) return;

            // Prefer the structured message result if available
            string latest = recognizer?.receivedMsg != null ? recognizer.receivedMsg.result : null;
            if (string.IsNullOrEmpty(latest))
            {
                // Fall back to empty when nothing recognized yet
                latest = string.Empty;
            }

            // Update UI only when changed to reduce GC/overdraw
            if (!string.Equals(latest, lastShown, StringComparison.Ordinal))
            {
                lastShown = latest;
                CurrentPrompt = latest;
                if (asrText != null)
                {
                    asrText.text = latest;
                }
            }
        }

        // Button OnClick target: Introduction/ASR input/Generate Btn
        [Button]
        public void GenerateFromAsr()
        {
            string prompt = (CurrentPrompt ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.LogWarning("[AIGCVoiceBridge] CurrentPrompt is empty; abort generate.");
                return;
            }

            if (setGeneratingStateOnClick)
            {
                // Optional: reflect UI state to Generating if your project uses it
                IAV.AIGC.UI.SkyboxPopupStateController.SetState(IAV.AIGC.AIGCCreateState.Generating);
            }

            if (OnGeneratePrompt != null)
            {
                try { OnGeneratePrompt.Invoke(prompt); }
                catch (Exception ex) { Debug.LogError("[AIGCVoiceBridge] OnGeneratePrompt threw: " + ex.Message); }
            }
            else if (skyboxApi != null)
            {
                try { skyboxApi.CreateSkybox(prompt); }
                catch (Exception ex) { Debug.LogError("[AIGCVoiceBridge] skyboxApi.CreateSkybox failed: " + ex.Message); }
            }
            else
            {
                Debug.LogWarning("[AIGCVoiceBridge] No generate target: assign SkyboxApi or set OnGeneratePrompt.");
            }
        }

        // Optional relay for UnityEvent if you prefer drag-and-drop without code
        // Bind BlockadeLabsSkyboxApi.CreateSkybox(string) to this via the target parameter.
        public void GenerateWithTarget(MonoBehaviour target)
        {
            if (target == null)
            {
                Debug.LogWarning("[AIGCVoiceBridge] GenerateWithTarget target is null");
                return;
            }
            var method = target.GetType().GetMethod("CreateSkybox", new Type[] { typeof(string) });
            if (method == null)
            {
                Debug.LogWarning("[AIGCVoiceBridge] Target has no CreateSkybox(string) method.");
                return;
            }
            string prompt = (CurrentPrompt ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.LogWarning("[AIGCVoiceBridge] CurrentPrompt is empty; abort generate.");
                return;
            }
            try { method.Invoke(target, new object[] { prompt }); }
            catch (Exception ex) { Debug.LogError("[AIGCVoiceBridge] Invoke error: " + ex.Message); }
        }
    }
}

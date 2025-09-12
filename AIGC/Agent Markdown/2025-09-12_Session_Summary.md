# 2025-09-12 · 语音→AIGC 全链路打通与精简 ASR

## What We Built/Fixed Today
- 新增桥接：`Assets/IAV/Scripts/AIGC/Bridge/AIGCVoiceBridge.cs`
  - 实时把 `ASRFromBaidu.receivedMsg.result` 显示到 `ASR_Text`
  - Generate 支持两种触发：
    - 直接调用 `BlockadeLabsSkyboxApi.CreateSkybox(prompt)`（Inspector 绑定 `skyboxApi`）
    - 或外部回调 `OnGeneratePrompt`（可代码注入）
  - 生成前可切换 `SkyboxPopupStateController` 到 `Generating`
- 精简 ASR：`Assets/IAV/Scripts/AIGC/API/ASRFromBaidu.cs`
  - 移除 Avatar 动画、Volume 图标、BaiduTTS、AudioPlayer、NLPAnalyzer 等耦合，仅保留“麦克风→Baidu ASR→流式文本”
  - 解决语音结束时 `FINISH` 发送的状态异常：增加 `finishSent` 幂等与连接状态检查
- 健壮性：`Assets/IAV/Scripts/SparkAPI/ChineseRecognizer.cs`
  - 同步加上 `finishSent` 与 `START/FINISH` 发送的状态保护，避免连接非 Open 时抛错（仅本地修改，未提交）
- 中文显示修复（TMP 字库）：
  - 建议引入中文字体（如 Noto Sans SC）动态 Font Asset，设置为 `ASR_Text` 字体或配置为 TMP 全局/局部 Fallback，解决方块字与缺字告警
- 远端同步与规范：
  - 之前将仓库推送到 GitHub，并在 `AGENTS.md` 增补“提交需明确批准、按功能拆分”的规范；后续严格遵守

## How To Use (End-to-End)
- 层级入口：`AIGCManager/SkyboxPopup/SkyboxAIGContext`
  - 语音启动：点击 `Introduction/Voice Input Btn` 显示 `ASR input` 并激活挂有 `ASRFromBaidu` 的对象
  - 实时显示：`AIGCVoiceBridge` 每帧读取 `ASRFromBaidu.receivedMsg.result`，同步到 `ASR_Text`
  - 生成触发：点击 `ASR input/Generate Btn` → 绑定 `AIGCVoiceBridge.GenerateFromAsr()`
    - Bridge 直接调用 `BlockadeLabsSkyboxApi.CreateSkybox(CurrentPrompt)` 并切到 `Generating`
  - 生成完成：`BlockadeLabsPusherTracker` 订阅完成事件并触发下载；可用 `SkyboxController.ApplyLatestFromStreamingAssets()` 应用最新天空盒

## Inspector Pitfalls / Wiring Notes
- AIGCVoiceBridge（挂在 AIGCManager 或 SkyboxAIGContext）
  - `Recognizer` → 赋值 `ASRFromBaidu` 组件实例
  - `Asr Text` → 赋值 `Introduction/ASR input/ASR_Text (TMP_Text)`
  - `Skybox Api` → 赋值场景内 `BlockadeLabsSkyboxApi` 组件实例
  - `Set Generating State On Click` → 勾选以切换到 `Generating`
- 按钮绑定
  - `ASR input/Generate Btn` → OnClick 绑定 `AIGCVoiceBridge.GenerateFromAsr()`（无需额外参数）
  - `ASR input/Cancel Btn` → 关闭 `ASRFromBaidu` 容器并返回 `Introduction`
- 字体
  - `ASR_Text` 使用包含中文的 TMP Font Asset，或在 TMP Fallback 中添加中文字体（动态字库 + Multi Atlas 推荐）

## Pusher / Networking Notes
- BlockadeLabs 实时链路保持不变：`SkyboxApi` → `OnCreateAccepted` → `PusherTracker` 订阅 `status_update` → `Downloader` 下载
- 建议始终使用 HTTPS/WSS；私钥与 API Key 后续抽到配置而非硬编码

## File/Runtime Notes
- ASR 采集：`ASRFromBaidu` 使用 NAudio.Wave 进行 16k 单声道流式采样并发送 WebSocket 帧
- 结束信令：由协程在停止录音后发送 `FINISH`；若连接非 Open 会跳过发送并记录告警，避免抛错
- 识别文本：从 `receivedMsg.result` 读取（流式更新，静音结束后为本段最终文本）

## Known Limitations / Ideas
- 中文 Prompt 兼容性：当前 BlockadeLabs 接口对中文不友好，生成结果与语义偏差较大
  - 备选路径：
    - 在 Bridge 或 Skybox 层加入“中文→英文”翻译（LLM 或翻译 API），再调用 `CreateSkybox`
    - 增加可选的 Prompt 增强/风格模板（英文），提升可控性
- ASR 源抽象：可定义 `IASRSource` 接口，`ASRFromBaidu/ChineseRecognizer` 共用，Bridge 仅依赖接口
- 秘钥与地址：将 ASR/Skybox/API 配置抽出到 ScriptableObject/环境变量，避免硬编码

## Next Steps (Suggestions)
- 在 `AIGCVoiceBridge` 中增加可选“翻译/润色”步骤：中文识别后先转英文再生成（提供开关）
- 为 `ASR_Text` 增加“识别完成”提示与 Generate 按钮可用态控制（静音结束后可点）
- 统一 FINISH/Close 时序：由协程负责收尾，避免多处关闭竞争
- 可选：将 `BlockadeLabsSkyboxApi` 的默认风格、negative_text 暴露在 UI 以微调生成效果

---

附：关键文件路径
- `Assets/IAV/Scripts/AIGC/Bridge/AIGCVoiceBridge.cs`
- `Assets/IAV/Scripts/AIGC/API/ASRFromBaidu.cs`
- `Assets/IAV/Scripts/AIGC/API/BlockadeLabsSkyboxApi.cs`
- `Assets/IAV/Scripts/AIGC/API/BlockadeLabsPusherTracker.cs`
- `Assets/IAV/Scripts/AIGC/API/BlockadeLabsDownloader.cs`
- `Assets/IAV/Scripts/SparkAPI/ChineseRecognizer.cs`（本地加了保护，未提交）

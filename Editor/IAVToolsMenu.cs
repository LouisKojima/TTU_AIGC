using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using IAVTools;
using static SaveTexture2FileUtility;

namespace UnityEditor.UI
{
    public class Menu001 : OdinMenuEditorWindow
    {

        [MenuItem("IAV Tools/001")]
        private static void OpenWindow()
        {
            GetWindow<Menu001>("IAVToolsMenu").Show();
        }

        private OdinMenuTree tree;
        protected override OdinMenuTree BuildMenuTree()
        {
            tree = new();

            //InitializeObjectWindow();

            tree.Add("Scene Objects", new OnSceneScriptFinderWindow());

            //tree.Add("ButtonCtrl", new ButtonControllerMenu(tree));

            //tree.Add("Vehicle Control", VehicleControlWindow.Instance);
            //tree.Add("Vehicle Control", new VehicleControlWindow());
            tree.Add("Scene Objects/Vehicle Control", new GameObjWindow(typeof(CarControl)));

            tree.Add("Scene Objects/Music Control", new GameObjWindow(typeof(MusicPlayer)));

            tree.Add("Scene Objects/Video Control", new GameObjWindow(typeof(VideoCtrl)));

            tree.Add("Scene Objects/Navigation Control", new GameObjWindow(typeof(NavigationCtrls)));

            tree.Add("Tools/Speech Mockup", new SpeechMockupWindow());

            tree.Add("Tools/Circular Formatter", CircularFormatterWindow.Instance);

            tree.Add("Tools/SaveTexture2File", new SaveTexture2FileWindow());

            //tree.Add("Light Band Sim", ScriptableObject.CreateInstance<LightBandSimWindow>());

            tree.Add("Tools/LayoutGroup Re-Order-er", new LayoutOrdererWindow());

            return tree;
        }

        private void InitializeObjectWindow()
        {
            var panel = GameObject.FindGameObjectWithTag("Panel");
            Debug.Log(panel.name);
            AddObjectToMenu("Data/", panel);
            var obj = GameObject.FindObjectsOfType<Button>();
            foreach (var ob in obj)
            {
                Debug.Log(ob.name);
                AddObjectToMenu("Data/", ob.gameObject);
            }
        }
        private void AddObjectToMenu(string path, GameObject obj)
        {
            tree.Add(path + obj.name, new ObjectWindow(obj));
            var components = obj.GetComponents<Component>();
            //foreach (Component comp in components)
            for (int i = 0; i < components.Length; i++)
            {
                Component comp = components[i];
                if (comp.GetType().Equals(typeof(RectTransform))) continue;
                Debug.Log(comp.GetType().ToString());
                var typeName = comp.GetType().ToString().Split(".");
                tree.Add(path + obj.name + "/" + i + ". " + typeName[typeName.Length - 1], new ObjectWindow(comp));
            }
        }
    }
    //no use
    public class ObjectWindow
    {
        [HideIf("@obj == null")]
        [InlineEditor(inlineEditorMode: InlineEditorModes.FullEditor, objectFieldMode: InlineEditorObjectFieldModes.Hidden)]
        public GameObject obj;
        [HideIf("@obj == null")]
        [InlineEditor(inlineEditorMode: InlineEditorModes.GUIOnly, objectFieldMode: InlineEditorObjectFieldModes.Foldout)]
        public Component[] components;
        [HideIf("@component == null")]
        [InlineEditor(inlineEditorMode: InlineEditorModes.FullEditor, objectFieldMode: InlineEditorObjectFieldModes.Hidden)]
        public Component component;

        public ObjectWindow(GameObject obj)
        {
            this.obj = obj;
            components = obj.GetComponents<Component>();
        }
        public ObjectWindow(Component comp)
        {
            this.component = comp;
        }
    }
    //no use
    public class ButtonControllerMenu
    {
        public enum SomeEnum { one, two, x3, si }

        [BoxGroup("Topbar", ShowLabel = false)]
        [HideLabel, EnumToggleButtons]
        public SomeEnum se;

        [BoxGroup("Topbar")]
        [ShowIf("se", SomeEnum.one)]
        public string oneTest;

        public OdinMenuTree tree;
        public ButtonControllerMenu(OdinMenuTree tree)
        {
            this.tree = tree;
        }

        public List<Button> buttons;
        [Button]
        public void FindButtons()
        {
            buttons = new(GameObject.FindObjectsOfType<Button>());
        }
    }

    public class CircularFormatterWindow
    {
        private bool rad = true;
        private string buttonTitle = "radius";
        [HorizontalGroup("Rad-Dia")]
        [Button("$buttonTitle")]
        private void SwitchRaiDia() { rad = !rad; buttonTitle = rad ? "radius" : "diameter"; }
        [HorizontalGroup("Rad-Dia")]
        [ShowIf("rad"), MinValue(1)]
        public int radius;
        [HorizontalGroup("Rad-Dia")]
        [HideIf("rad"), ShowInInspector]
        public int diameter { get { return radius * 2; } set { this.radius = value / 2; } }
        [PropertyRange(0, "radius"), MaxValue("radius")]
        public int width;
        [PropertyRange(0, 360)]
        public int angle;

        [HideInInspector]
        public Color color;
        public Gradient gradient;

        [HorizontalGroup("Toggles")]
        [ToggleLeft, GUIColor("@radial? Color.yellow : Color.cyan")]
        public bool radial = false;
        [HorizontalGroup("Toggles")]
        [ToggleLeft, GUIColor("@openingUpwards? Color.yellow : Color.cyan")]
        public bool openingUpwards = false;

        [PreviewField(200, Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Center), ReadOnly, HideLabel]
        public Texture2D texture = new(256, 256, TextureFormat.ARGB32, false);

        [Button("Generate"), VerticalGroup("GenerateButton")]
        public void GeneratePic()
        {
            if (openingUpwards)
                texture = CircularFormatter.GenerateArcTexture(
                    radius, width, angle, gradient,
                    new(-Mathf.Sin(Mathf.Deg2Rad * angle / 2f), -Mathf.Cos(Mathf.Deg2Rad * angle / 2f)),
                    radial);
            else
                texture = CircularFormatter.GenerateArcTexture(radius, width, angle, gradient, radial);
        }

        [FoldoutGroup("Save", GroupName = "Save File", Expanded = false)]
        [FolderPath(ParentFolder = "Assets/IAV/Visuals/Generated", AbsolutePath = true)]
        public string filePath = "";
        [FoldoutGroup("Save", GroupName = "Save File", Expanded = false), SuffixLabel(".png"), InlineButton("SaveImg", Label = "Save")]
        public string fileName = "";

        private static string defaultPath = "/IAV/Visuals/Generated";
        private void SaveImg()
        {
            if (filePath == "") filePath = Application.dataPath + defaultPath;
            if (fileName == "") fileName = texture.GetHashCode() + "";
            CircularFormatter.Save2PNG(texture, filePath, fileName);
        }

        [Button]
        private void ResetValues()
        {
            radius = 128;
            width = 36;
            angle = 360;
            color = Color.cyan;
            gradient = new();
            GradientColorKey[] c = { new(color, 0f), new(color, 1f) };
            GradientAlphaKey[] a = { new(1f, 0f), new(0f, 1f) };
            gradient.SetKeys(c, a);
        }
        public static CircularFormatterWindow Instance { get; } = new();
        private CircularFormatterWindow()
        {
            if (radius == 0) ResetValues();
            if (texture == null) { texture = new(diameter, diameter, TextureFormat.ARGB32, false); }
        }
    }

    //public class VehicleControlWindow
    //{
    //    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    //    public CarControl controller;
    //    public VehicleControlWindow()
    //    {
    //        Initialize();
    //    }

    //    [Button]
    //    public void Initialize()
    //    {
    //        controller = GameObject.FindObjectOfType<CarControl>();
    //    }
    //    //public static VehicleControlWindow Instance { get; } = new();
    //}

    public class GameObjWindow
    {
        private Type targetType;
        [ShowInInspector]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public object controller;
        public GameObjWindow(Type targetType)
        {
            this.targetType = targetType;
            Initialize();
        }

        [Button]
        public void Initialize()
        {
            controller = Convert.ChangeType(GameObject.FindObjectOfType(targetType, true), targetType);
        }
        //public static VehicleControlWindow Instance { get; } = new();
    }

    public class OnSceneScriptFinderWindow
    {
        [ShowInInspector]
        public Type toFind = typeof(CarControl);
        [InlineEditor, HideLabel]
        public List<UnityEngine.Object> result;

        [Button]
        private void Search()
        {
            result = GameObject.FindObjectsOfType(toFind, true).ToList();
        }
    }

    public class SpeechMockupWindow
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public TextRecognition textRecog;
        public SpeechMockupWindow()
        {
            Initialize();
        }

        [Button]
        public void Initialize()
        {
            textRecog = GameObject.FindObjectOfType<TextRecognition>();
        }
    }

    public class SaveTexture2FileWindow
    {
        [Button(Expanded = true, Name = "Save RenderTexture")]
        public void SaveRenderTexture(RenderTexture renderTexture, [Sirenix.OdinInspector.FilePath(AbsolutePath = true)] string filePath, SaveTextureFileFormat fileFormat = SaveTextureFileFormat.PNG, int jpgQuality = 95)
        {
            SaveRenderTextureToFile(renderTexture, filePath, fileFormat, jpgQuality);
        }
    }

    public class LightBandSimWindow : OdinEditorWindow
    {
        public PortControl portControl;

        public List<Color> lights = new();

        [ShowInInspector, DisplayAsString]
        private int length { get => lights.Count; }

        [OnInspectorInit]
        private void Init()
        {
            //Color[] temp = new Color[120];
            //Array.Fill(temp, Color.black);
            //lights = temp.ToList();
            if (!portControl)
            {
                portControl = FindObjectOfType<PortControl>();
            }
        }

        public string[] s;

        [OnInspectorGUI]
        private void OnUpdate()
        {
            ReadColors();
            DrawLights();
        }

        //[OnInspectorGUI]
        private void ReadColors()
        {
            if (portControl)
            {
                s = portControl.str.Remove(0, PortControl.head.Length).ToUpper().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < s.Length / 3; i++)
                {
                    int g = Convert.ToInt32(s[3 * i], 16);
                    int r = Convert.ToInt32(s[3 * i + 1], 16);
                    int b = Convert.ToInt32(s[3 * i + 2], 16);
                    Color result = new Color(r / 255f, g / 255f, b / 255f);
                    if (lights.Count > i)
                    {
                        lights[i] = result;
                    }
                    else
                    {
                        lights.Add(result);
                    }
                }
            }
        }

        public int width = 10;
        public int height = 10;

        //[OnInspectorGUI]
        private void DrawLights()
        {
            Rect rect = EditorGUILayout.BeginHorizontal();
            //Debug.Log(rect.ToString());
            //Debug.Log("?" + EditorGUILayout.GetControlRect().ToString());
            //rowCount = Mathf.CeilToInt(EditorGUILayout.GetControlRect().width) / width + 1;
            int rowCount = Mathf.CeilToInt(EditorGUILayout.GetControlRect().width) / width + 1;
            for (int i = 0; i < lights.Count; i++)
            {
                int x = i % rowCount;
                int y = i / rowCount;
                EditorGUI.DrawRect(new(x * width + rect.x, y * height + rect.y, width, height), lights[i]);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    //public class AvatarStateWindow
    //{
    //    public Animator avatar;
    //    public List<GameObject> Size_L_Views;
    //    public List<GameObject> Size_M_Views;
    //    public List<GameObject> Off_Views;

    //    public void Apply()
    //    {
    //        avatar.animator
    //    }
    //}

    public class LayoutOrdererWindow
    {
        [OnValueChanged(nameof(LoadElements))]
        public HorizontalOrVerticalLayoutGroup layoutGroup;

        [ListDrawerSettings(
            Expanded = true,
            HideAddButton = true, 
            HideRemoveButton = true, 
            ShowItemCount = true, 
            NumberOfItemsPerPage = 100,
            OnTitleBarGUI = nameof(RefreshBtn))]
        public List<RectTransform> elements;

        private void LoadElements()
        {
            if (!layoutGroup) elements = new();
            elements = layoutGroup.GetComponentsInChildren<RectTransform>(true)
            .Where(x => x.parent == layoutGroup.transform).ToList();
        }

        private void RefreshBtn()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                LoadElements();
            }
        }

        [Button]
        public void ApplyOrder()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetSiblingIndex(i);
            }
        }
    }
}

using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

public class ListSelection : MonoBehaviour
{
    public IndexSource indexSource;
    [InlineEditor]
    public MoviePlayList playList;

    [Serializable]
    public class ImageContent
    {
        [HideInInspector]
        public MoviePlayList playList;

        public ImageContent(MoviePlayList playList)
        {
            this.playList = playList;
        }

        public Image targetImage;

        [ShowInInspector]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetValueDropdown), AppendNextDrawer = true, DisableListAddButtonBehaviour = false)]
#endif
        public List<Sprite> images;

#if UNITY_EDITOR
        private Sprite[] GetValueDropdown(InspectorProperty property)
        {
            int index = property.Parent.Index;
            if (property.Name == nameof(images))
                index = images.Count;
            if (index >= playList.playList.Count)
                return null;
            return playList.playList[index].images;
        }
#endif
        internal void Deconstruct(out Image targetImage, out List<Sprite> images)
        {
            targetImage = this.targetImage;
            images = this.images;
        }
    }

    [ShowInInspector]
    [TableList]
#if UNITY_EDITOR
    [ListDrawerSettings(CustomAddFunction = nameof(CustomAddImage), OnTitleBarGUI = nameof(RefreshImage))]
#endif
    public List<ImageContent> images = new();

#if UNITY_EDITOR
    private ImageContent CustomAddImage()
    {
        return new(playList);
    }

    private void RefreshImage()
    {
        if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
        {
            images.ForEach(x => x.playList = playList);
        }
    }
#endif

    [Serializable]
    public class TextContent
    {
        [HideInInspector]
        public MoviePlayList playList;

        public TextContent(MoviePlayList playList)
        {
            this.playList = playList;
        }

        public TMP_Text text;

        private string[] fieldNames => typeof(MoviePlayList.MovieEntry)
            .GetFields()
            .Where(x => x.FieldType == typeof(string))
            .Select(x => x.Name)
            .ToArray();


        [ValueDropdown(nameof(fieldNames)), ShowInInspector]
        public string field;

        [ShowInInspector]
        public List<string> contents => playList?.playList.Select(x => (string)typeof(MoviePlayList.MovieEntry).GetField(field ?? fieldNames[0]).GetValue(x)).ToList();

        internal void Deconstruct(out TMP_Text text, out List<string> contents)
        {
            text = this.text;
            contents = this.contents;
        }
    }

    [ShowInInspector]
    [TableList]
#if UNITY_EDITOR
    [ListDrawerSettings(CustomAddFunction = nameof(CustomAddText), OnTitleBarGUI = nameof(RefreshText))]
#endif
    public List<TextContent> texts = new();

#if UNITY_EDITOR
    private TextContent CustomAddText()
    {
        return new(playList);
    }

    private void RefreshText()
    {
        if(SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
        {
            texts.ForEach(x => x.playList = playList);
        }
    }
#endif

    public List<GameObject> showOnFavourite = new();
    public List<GameObject> showNoFavourite = new();

    [Button]
    public void SelectText(int index)
    {
        if (index < 0) return;
        foreach ((TMP_Text text, List<string> contents) in texts)
        {
            text.text = contents[index % contents.Count];
        }
    }

    public void SelectText()
    {
        if (!indexSource) return;
        SelectText(indexSource.getIndex());
    }

    [Button]
    public void SelectImage(int index)
    {
        if (index < 0) return;
        foreach((Image targetImage, List<Sprite>images) in images)
        targetImage.sprite = images[index % images.Count];
    }

    public void SelectImage()
    {
        if (!indexSource) return;
        SelectImage(indexSource.getIndex());
    }

    public void SelectFavourite(int index)
    {
        if (index < 0) return;
        foreach (GameObject go in showOnFavourite)
            go.SetActive(playList.playList[index % playList.playList.Count].isFavourite);
        foreach (GameObject go in showNoFavourite)
            go.SetActive(!playList.playList[index % playList.playList.Count].isFavourite);
    }

    [Button]
    public void SelectFavourite()
    {
        if (!indexSource) return;
        SelectFavourite(indexSource.getIndex());
    }
}

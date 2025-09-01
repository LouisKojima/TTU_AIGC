using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "MoviePlayList.asset", menuName = "IAV/MoviePlayList", order = 1)]
[Serializable]
public class MoviePlayList : ScriptableObject
{
    [Serializable]
    public class MovieEntry
    {
        public VideoClip videoclip;
        public string title;
        public string director;
        [MultiLineProperty]
        public string intro;
        public bool isFavourite;
        public Sprite[] images;
    }

    public List<MovieEntry> playList;
}

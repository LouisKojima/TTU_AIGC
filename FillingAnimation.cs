using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillingAnimation : MonoBehaviour
{
    [Required]
    public Image image1, image2;

    [Range(0, 1)]
    public float progress = 0f;
    [Range(0, 1)]
    public float width = 0.1f;
    [LabelText("Speed %")]
    public float speed = 10f;
    public bool isPlaying = true;

    private void OnValidate()
    {
        if (!image1 || !image2) return;
        float fill1 = Mathf.Clamp(progress + width / 2, 0, 1);
        float fill2 = Mathf.Clamp(1 - progress + width / 2, 0, 1);
        if (image1.fillAmount != fill1)
            image1.fillAmount = fill1;
        if (image2.fillAmount != fill2)
            image2.fillAmount = fill2;
    }

    // Update is called once per frame
    void Update()
    {
        OnValidate();

        if (isPlaying)
        {
            progress += Time.deltaTime * speed / 100;
            if (progress > 1) progress -= 1;
        }
    }
}

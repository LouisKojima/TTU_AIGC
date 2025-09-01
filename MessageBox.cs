using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;


public class MessageBox : MonoBehaviour
{
    public static MessageBox instance;
    [Tooltip("框")]
    [SerializeField] private RectTransform box;

    [Tooltip("文字")]
    [SerializeField] private Text content;

    [Tooltip("确认")]
    [SerializeField] private Button confirmButton;

    [Tooltip("取消")]
    [SerializeField] private Button cancelButton;

    [Tooltip("关闭")]
    [SerializeField] private Button closeButton;

    [Tooltip("弹出时间")]
    [SerializeField] private float moveTime;

    private float startY;
    public bool isShow;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }



    // Start is called before the first frame update
    private void Start()
    {
        closeButton.gameObject.SetActive(false);
        box.gameObject.SetActive(false);
        startY = box.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowMessageBox(string text, UnityAction confirmAction, UnityAction cancelAction, bool isUseClose)
    {
        ShowMessageBoxConfirmCancel(moveTime, confirmAction, cancelAction, isUseClose, text);
        isShow = true;
    }
    public void ShowMessageBox(string text, UnityAction confirmAction)
    {
        ShowMessageBoxConfirm(moveTime, confirmAction, text);
        isShow = true;
    }

    public void ShowMessageBox(string text, float waitTime)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageBoxTimeIE(moveTime, waitTime, text));
        isShow = true;
    }

    public void HideMessageBox()
    {
        StopAllCoroutines();
        StartCoroutine(HideMessageBoxIE(moveTime));
    }

    private void ShowMessageBoxConfirm(float time, UnityAction confirmAction, string text)
    {
        closeButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        EnableButtons();

        confirmButton.onClick.RemoveAllListeners();
        if (confirmAction != null)
        {
            confirmButton.onClick.AddListener(confirmAction);
        }
        confirmButton.onClick.AddListener(DisableButtons);
        confirmButton.onClick.AddListener(HideMessageBox);
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(DisableButtons);
        cancelButton.onClick.AddListener(HideMessageBox);
        content.text = text;
        StopAllCoroutines();
        StartCoroutine(ShowMessageBoxIE(time));
    }
    private void ShowMessageBoxConfirmCancel(float time, UnityAction confirmAction, UnityAction cancelAction, bool isUseClose, string text)
    {
        closeButton.gameObject.SetActive(isUseClose);
        cancelButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        EnableButtons();

        confirmButton.onClick.RemoveAllListeners();
        if (confirmAction != null)
        {
            confirmButton.onClick.AddListener(confirmAction);
        }
        confirmButton.onClick.AddListener(DisableButtons);
        confirmButton.onClick.AddListener(HideMessageBox);
        if (cancelAction != null)
        {
            cancelButton.onClick.AddListener(cancelAction);
        }
        cancelButton.onClick.AddListener(DisableButtons);
        cancelButton.onClick.AddListener(HideMessageBox);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(DisableButtons);
        closeButton.onClick.AddListener(HideMessageBox);
        content.text = text;
        StopAllCoroutines();
        StartCoroutine(ShowMessageBoxIE(time));
    }

    private void DisableButtons()
    {
        confirmButton.interactable = false;
        cancelButton.interactable = false;
        closeButton.interactable = false;
    }
    private void EnableButtons()
    {
        confirmButton.interactable = true;
        cancelButton.interactable = true;
        closeButton.interactable = true;
    }
    private IEnumerator ShowMessageBoxTimeIE(float time, float waitTime, string text)
    {
        closeButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        content.text = text;
        yield return StartCoroutine(ShowMessageBoxIE(time));
        yield return new WaitForSecondsRealtime(waitTime);
        HideMessageBox();
    }
    private IEnumerator ShowMessageBoxIE(float time)
    {
        float count = 0;
        box.gameObject.SetActive(true);
        while (count < time)
        {
            count += Time.unscaledDeltaTime;
            box.localPosition = new Vector2(box.localPosition.x, Mathf.Lerp(startY, 0, count / time));
            yield return 0;
        }
        box.localPosition = new Vector2(box.localPosition.x, 0);
    }
    private IEnumerator HideMessageBoxIE(float time)
    {
        float count = 0;
        float lastY = box.localPosition.y;
        while (count < time)
        {
            count += Time.unscaledDeltaTime;
            box.localPosition = new Vector2(box.localPosition.x, Mathf.Lerp(lastY, startY, count / time));
            yield return 0;
        }
        box.localPosition = new Vector2(box.localPosition.x, startY);
        box.gameObject.SetActive(false);
        isShow = false;
    }
}
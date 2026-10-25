using UnityEngine;
using TMPro;
using System.Collections;

public class InfoPopup : MonoBehaviour
{
    public static InfoPopup Instance;

    [Header("UI References")]
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text messageText;

    [Header("Settings")]
    [SerializeField] private float displayTime = 2.5f;

    private Coroutine popupCoroutine;

    private void Awake()
    {
        Instance = this;

        if (popup != null)
            popup.SetActive(false);
    }

    public static void Show(string message)
    {
        if (Instance == null)
        {
            Debug.LogError("INFO POPUP: No InfoPopup instance found in the scene!");
            return;
        }

        Instance.ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (popup == null)
        {
            Debug.LogError("INFO POPUP: Popup GameObject is not assigned!");
            return;
        }

        if (messageText == null)
        {
            Debug.LogError("INFO POPUP: Message Text is not assigned!");
            return;
        }

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(DisplayPopup(message));
    }

    private IEnumerator DisplayPopup(string message)
    {
        messageText.text = message;
        popup.SetActive(true);

        Debug.Log("INFO POPUP SHOWN: " + message);

        yield return new WaitForSeconds(displayTime);

        popup.SetActive(false);
        popupCoroutine = null;
    }
}
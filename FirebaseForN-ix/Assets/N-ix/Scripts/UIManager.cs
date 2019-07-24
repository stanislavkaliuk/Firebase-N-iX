using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Blockers")]
    public GameObject AuthBlocker;
    public GameObject DatabaseBlocker;
    public GameObject StorageBlocker;
    public GameObject MessagesBlocker;
    public GameObject RemoteConfigBlocker;
    public GameObject AnalyticsBlocker;

    [Header("Auth")]
    public GameObject SignInButon;

    [Header("Database")]
    public GameObject WriteButton;
    public GameObject ReadButton;

    public TextMeshProUGUI WriteValue;
    public TextMeshProUGUI ReadValue;

    [Header("Storage")]
    public GameObject LoadBundleButton;

    [Header("Messages")]
    public GameObject RequestMessage;

    [Header("Remote Config")]
    public GameObject GetValueButton;
    public TextMeshProUGUI ConfigValue;

    [Header("Analytics")]
    public GameObject SendEventButton;

    [Header("Boxes")]
    public MeshRenderer Box;

    private void Awake()
    {
        FirebaseManager manager = GetComponent<FirebaseManager>();

        manager.OnSignIn += OnSignedIn;
        manager.OnRetrieveData += OnGetDatabaseData;
    }

    private void OnDestroy()
    {
        FirebaseManager manager = GetComponent<FirebaseManager>();
        manager.OnSignIn -= OnSignedIn;
        manager.OnRetrieveData -= OnGetDatabaseData;
    }

    private void OnGetDatabaseData(string message)
    {
        SetText(ReadValue, message);
    }

    private void OnSignedIn()
    {
        TurnOff(SignInButon);
        TurnOff(DatabaseBlocker);
        TurnOff(StorageBlocker);
    }

    public void TurnOff(GameObject thisObject)
    {
        thisObject.SetActive(false);
    }

    public void SetText(TextMeshProUGUI field, string text)
    {
        field.text = text;
    }

    public void SetMaterial(Material material)
    {
        Box.sharedMaterial = material;
    }
}

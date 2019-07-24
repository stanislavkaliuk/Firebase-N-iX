using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using System;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Messaging;
using Firebase.Unity.Editor;
using Firebase.RemoteConfig;
using Firebase.Storage;
using UnityEngine.Networking;
using Firebase.Functions;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseApp App;

    private FirebaseAuth Auth;
    private FirebaseUser CurrentUser;
    public event Action OnSignIn;

    private DatabaseReference dbReference;
    public event Action<string> OnRetrieveData;

    private string messageToken;

    private Dictionary<string, object> defaults = new Dictionary<string,object>();
    private string RemoteConfigResult;

    private FirebaseStorage storage;

    private FirebaseFunctions functions;
    private HttpsCallableReference functionReference;

    private UIManager manager;

    private void Awake()
    {
        manager = GetComponent<UIManager>();
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread<DependencyStatus>((task)=> 
        {
            DependencyStatus status = task.Result;
            if (status == DependencyStatus.Available)
            {
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.DefaultInstance;
                functionReference = functions.GetHttpsCallable("RequestMessage");
                manager.TurnOff(manager.AuthBlocker);
                manager.TurnOff(manager.AnalyticsBlocker);
                App.SetEditorDatabaseUrl("https://fir-n-ix.firebaseio.com/");
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
                FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
                defaults.Add("RemoteTest", 25);
                FirebaseRemoteConfig.SetDefaults(defaults);
                FirebaseRemoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread((remoteConfigTask)=> 
                {
                    FirebaseRemoteConfig.ActivateFetched();
                    RemoteConfigResult = FirebaseRemoteConfig.GetValue("RemoteTest").StringValue;
                    manager.TurnOff(manager.RemoteConfigBlocker);
                });
                storage = FirebaseStorage.DefaultInstance;
            }
            else
            {
                Debug.Log(string.Format("Can't resolve all Firebase dependencies: {0}", status));
            }
        }); 
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Get message; " + e.Message);
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        messageToken = e.Token;
        manager.TurnOff(manager.MessagesBlocker);
    }

    /// <summary>
    /// Test analytics
    /// </summary>
    public void LogEvent()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectContent, FirebaseAnalytics.ParameterContent, "Click");
    }

    /// <summary>
    /// Test sign in
    /// </summary>
    public void SignIn()
    {
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread<FirebaseUser>((task)=> 
        {
            CurrentUser = task.Result;
            OnSignIn?.Invoke();
        });    
    }

    /// <summary>
    /// Test database writing
    /// </summary>
    public void WriteToFirebaseDatabase()
    {
        string message = "Hello " + CurrentUser.DisplayName + "_" + CurrentUser.UserId;
        manager.SetText(manager.WriteValue, message);
        dbReference.Child("users").Child(CurrentUser.UserId).Child("Word").SetValueAsync("Hello " + CurrentUser.DisplayName + "_" + CurrentUser.UserId);
        manager.TurnOff(manager.WriteButton);
    }

    /// <summary>
    /// Test database reading
    /// </summary>
    public void ReadFirebaseDatabase()
    {
        dbReference.Child("users").Child(CurrentUser.UserId).Child("Word").GetValueAsync().ContinueWithOnMainThread<DataSnapshot>((task)=>
        {
            DataSnapshot snapshot = task.Result;
            string value = snapshot.GetValue(false).ToString();
            OnRetrieveData?.Invoke(value);
        });
    }

    public void GetConfig()
    {
        manager.SetText(manager.ConfigValue, RemoteConfigResult);
    }


    /// <summary>
    /// Storage implementation test
    /// </summary>
    public void LoadBundle()
    {
        StartCoroutine(GetAssetBundle(ApplyBundle));
    }

    private void ApplyBundle(AssetBundle bundle)
    {
        Material asset = bundle.LoadAsset<Material>("bundlematerial");
        manager.SetMaterial(asset);
    }

    private IEnumerator GetAssetBundle(Action<AssetBundle> OnComplete)
    {
        string url = "https://firebasestorage.googleapis.com/v0/b/fir-n-ix.appspot.com/o/bundlematerial?alt=media&token=8ae0747b-1888-4295-8210-7a0dfc49486d";
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                OnComplete(null);
                yield break;
            }
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            OnComplete(bundle);
        }
    }

    public void RequestFunction()
    {
        functionReference.CallAsync(messageToken).ContinueWithOnMainThread<HttpsCallableResult>((task) =>
        {
            var res = task.Result;
            Debug.Log("functions success: " + res.Data.ToString());
        });
    }
}

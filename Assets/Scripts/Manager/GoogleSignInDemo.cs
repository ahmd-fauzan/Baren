using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoogleSignInDemo : MonoBehaviour
{
    public Text infoText;
    public string webClientId = "<your client id here>";

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;

    public bool inUnity;

    public string userId;

    string tokenId;

    int signType;

    Firebase.Auth.FirebaseUser user;

    [SerializeField]
    InputField usernameIF;

    [SerializeField]
    GameObject usernameInput;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();

        usernameInput.SetActive(false);

        
    }

    public void StartGame()
    {

        PlayerController controller = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        DatabaseManager manager = ScriptableObject.CreateInstance<DatabaseManager>();
        manager.UsernameExist(controller.DbReference, usernameIF.text).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (!task.Result)
                    SignInWithGoogleOnFirebase(tokenId, false);
                else
                    usernameIF.text = "";
            }
        });
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;

                    OnSignInSilently();
                }
                else
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    public void UserExist(string email, string tokenId)
    {
        Debug.Log("Called : " + email + " Auth : " + auth);
        auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Caleed Inside : " + task.Result.ToString());
            if (task.IsCanceled)
            {
                Debug.Log("Task is cancelled");
            }

            if (task.IsCompleted)
            {
                int lenght = 0;
               foreach(string str in task.Result)
                {
                    Debug.Log("Exist : " + str);
                    lenght++;
                }
               if(lenght == 0)
                    usernameInput.SetActive(true);
                else
                    SignInWithGoogleOnFirebase(tokenId, true);
            }
            if (task.IsFaulted)
            {
                Debug.Log("Exception : " + task.Exception);
            }
        });
    }

    public void SignInWithGoogle() {
        if (!inUnity)
        {
            OnSignIn();
        }
        else
        {
            PlayerController controller = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;
            controller.UserID = userId;

            SceneManager.LoadScene("Menu");
        }
    }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        signType = 1;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        infoText.text = "";
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        AddToInformation("Signing In...");
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);

                    if (signType == 2)
                        OnSignOut();
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
        }
        else
        {
            AddToInformation("SignIn Success");

            AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            AddToInformation("Email = " + task.Result.Email);
            AddToInformation("Google ID Token = " + task.Result.IdToken);
            AddToInformation("Email = " + task.Result.Email);

            tokenId = task.Result.IdToken;

            if (signType == 2)
                SignInWithGoogleOnFirebase(task.Result.IdToken, true);
            else
                UserExist(task.Result.Email, task.Result.IdToken);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken, bool isLogin)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
                AddToInformation("Sign In Successful.");
                PlayerController controller = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;
                controller.UserID = task.Result.UserId;

                if (!isLogin)
                {
                    

                    DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>();

                    dbManager.CreateUser(controller.DbReference, usernameIF.text, task.Result.UserId);
                }

                SceneManager.LoadScene("Menu");
            }
        });
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        signType = 2;

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToInformation("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void AddToInformation(string str) { infoText.text += "\n" + str; }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
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

public class AuthenticationManager : MonoBehaviour
{
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

    [SerializeField]
    GameObject buttonLogin;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();

        usernameInput.SetActive(false);

        buttonLogin.SetActive(true);
    }

    public void StartGame()
    {

        PlayerController controller = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        DatabaseManager manager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
        manager.UsernameExist(usernameIF.text).ContinueWithOnMainThread(task =>
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
                    Debug.Log("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                Debug.Log("Dependency check was not completed. Error : " + task.Exception.Message);
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
                {
                    buttonLogin.SetActive(false);
                    usernameInput.SetActive(true);
                }
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
        Debug.Log("Calling SignIn");

        signType = 1;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        Debug.Log("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);

                    if (signType == 2)
                        OnSignOut();
                }
                else
                {
                    Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Task Cancelled");
        }
        else
        {
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
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0)) { }
            }
            else
            {
                PlayerController controller = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;
                controller.UserID = task.Result.UserId;
                
                DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

                if (!isLogin)
                {
                    dbManager.CreateUser(usernameIF.text, task.Result.UserId);
                    SceneManager.LoadScene("Menu");
                    return;
                }

                dbManager.UserDataExist(task.Result.UserId).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        if (task.Result)
                        {
                            SceneManager.LoadScene("Menu");
                        }
                        else
                        {
                            usernameInput.SetActive(true);
                            buttonLogin.SetActive(false);
                        }
                    }
                });
               
            }
        });
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        signType = 2;

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
}
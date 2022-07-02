using System;
using System.Collections;
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
using System.Text.RegularExpressions;
using System.IO;


public class AuthenticationManager : MonoBehaviour
{
    public string webClientId = "<your client id here>";

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;

    private string currUserId;
    private int id;
    private string tokenId;

    bool isValidDepend;

    [SerializeField]
    InputField usernameIF;

    [SerializeField]
    GameObject usernameInput;

    [SerializeField]
    GameObject buttonLogin;

    private bool isBindAccount;

    AutentikasiPage page;

    private void Awake()
    {
        page = GameObject.Find("AutentikasiPage").GetComponent<AutentikasiPage>();

        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();

        usernameInput.SetActive(false);

        buttonLogin.SetActive(true);

        isBindAccount = false;

        page.ShowScreen("loadingScreen");

        /*#if UNITY_EDITOR
        startScreen.SetActive(true);
        #endif*/
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("Menu");
    }

    public void GuestUser()
    {
        PlayerController pController = PlayerController.GetInstance();

        if (!pController.CheckGuestPlayer())
            pController.CreateGuestPlayerInfo();

        pController.CurrentUserId = "guestAccount";

        page.ShowScreen("startScreen");
    }

    public void StartGame()
    {
        DatabaseManager manager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
        /*     if (usernameIF.text.Contains("@")|| usernameIF.text.Contains("#") || usernameIF.text.Contains("$") || usernameIF.text.Contains("%") || usernameIF.text.Contains("^") || usernameIF.text.Contains("&") || usernameIF.text.Contains("*") || usernameIF.text.Contains("(") || usernameIF.text.Contains(")") || usernameIF.text.Contains("-") || usernameIF.text.Contains("_") || usernameIF.text.Contains("+") || usernameIF.text.Contains("=") || usernameIF.text.Contains("~") || usernameIF.text.Contains("`") || usernameIF.text.Contains("<") || usernameIF.text.Contains(",") || usernameIF.text.Contains(">") || usernameIF.text.Contains(".") || usernameIF.text.Contains("?") || usernameIF.text.Contains("/")) 
                 Debug.Log("bangsat");*/

        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        Debug.Log("Current User : " + currUserId);

        if (regexItem.IsMatch(usernameIF.text) && usernameIF.text.Length <= 8 && usernameIF.text.Length > 0)
        {
            manager.UsernameExist(usernameIF.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    if (!task.Result)
                    {
                        PlayerController pController = PlayerController.GetInstance();

                        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

                        if (!String.IsNullOrEmpty(PlayerPrefs.GetString("BarenAccount")))
                        {
                            StartCoroutine(ApiHelper.Update(id, usernameIF.text, currUserId));

                            pController.CurrentUserId = currUserId;
                        }

                        if (pController.CheckGuestPlayer() && isBindAccount)
                        {
                            Debug.Log("Bind Account ");
                            PlayerInfo pInfo = pController.LoadGuestPlayerInfo();
                            pInfo.Username = usernameIF.text;
                          
                            dbManager.UpdatePlayerInfo(currUserId, pInfo).ContinueWithOnMainThread(task =>
                            {
                                Debug.Log("Player Info berhasil disimpan");
                            });

                            List<History> historyList = pController.LoadGuestHistory();

                            if(historyList != null && historyList.Count != 0)
                            {
                                for (int i = 0; i < historyList.Count; i++)
                                {
                                    dbManager.AddHistory(historyList[i], i, currUserId).ContinueWithOnMainThread(task1 =>
                                    {
                                        if (task1.IsCompleted)
                                        {
                                            //ShowErrorMessage("Akun Berhasil Bind");

                                        }
                                    });
                                }
                            }

                            Debug.Log("Bind Account Success");

                            pController.DeleteFileGuest();
                            dbManager.UpdateSignedIn(true, currUserId);
                            pController.CurrentUserId = currUserId;
                            isBindAccount = false;
                            page.ShowScreen("startScreen");
                        }
                        else
                        {
                            Debug.Log("Create Account");

                            PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();
                            pInfo.Username = usernameIF.text;
                            pInfo.BattlePoint = 0;
                            pInfo.WinRate = 1;

                            dbManager.UpdatePlayerInfo(currUserId, pInfo).ContinueWithOnMainThread(task =>
                            {
                                Debug.Log("Player Info berhasil disimpan");
                            });
                            dbManager.UpdateSignedIn(true, currUserId);
                            pController.CurrentUserId = currUserId;
                            pController.DeleteFileGuest();
                            //ShowErrorMessage("Akun Berhasil Dibuat");
                            page.ShowScreen("startScreen");
                        }
                    }

                    else
                    {
                        StartCoroutine(page.ShowErrorMessage("Username", "Username Sudah Digunakan"));
                    }

                }
            });
        }
        else
        {
            StartCoroutine(page.ShowErrorMessage("Username", "Username Tidak Valid"));
        }
 
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    isValidDepend = true;

                    if(auth == null)
                        auth = FirebaseAuth.DefaultInstance;

                    PlayerController pController = PlayerController.GetInstance();

                    if (pController.CheckGuestPlayer())
                        page.ShowScreen("authScreen");

                   else if (!String.IsNullOrEmpty(PlayerPrefs.GetString("BarenAccount")))
                    {
                        pController.CurrentUserId = PlayerPrefs.GetString("BarenAccount");
                        currUserId = pController.CurrentUserId;
                        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

                        dbManager.UserDataExist(pController.CurrentUserId).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                if (task.Result)
                                    page.ShowScreen("startScreen");
                                else
                                    page.ShowScreen("usernameForm");
                            }
                        });
                    }
                    else
                    {
                        OnSignInSilently();
                    }

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
                if (lenght == 0)
                {
                    SignInWithGoogleOnFirebase(tokenId);
                }
                else
                    page.ShowPopUp("confirmBind");
                    //changeAccountScreen.SetActive(true);
            }
            if (task.IsFaulted)
            {
                Debug.Log("Exception : " + task.Exception);
            }
        });
    }

    public void SignInWithGoogle(bool bindAccount) {
        if (!isValidDepend)
            return;

        isBindAccount = bindAccount;

        OnSignIn();

    }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        if(GoogleSignIn.Configuration == null)
            GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        try
        {
            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Singin width google Exception:" + ex.Message);
        }
    }

    private void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
        PlayerController pController = PlayerController.GetInstance();
        dbManager.UpdateSignedIn(false, pController.CurrentUserId);
        PlayerPrefs.DeleteKey("BarenAccount");
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
                    page.ShowScreen("authScreen");
                    page.ShowPopUp("closeAll");
                    //if (signType == 2)
                        //OnSignOut();
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

            if (isBindAccount)
            {
                UserExist(task.Result.Email, task.Result.IdToken);
            }
            else
                SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    public void BindAccount()
    {
        SignInWithGoogleOnFirebase(tokenId);
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Debug.Log("Sign In Run");

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

                currUserId = task.Result.UserId;

                dbManager.UserDataExist(task.Result.UserId).ContinueWithOnMainThread(taskData =>
                {
                    if (taskData.IsCompleted)
                    {
                        bool result = (bool)taskData.Result;
                        Debug.Log("Run Login2 : " + result);

                        if (taskData.Result)
                        {
                            dbManager.GetSignedIn(task.Result.UserId).ContinueWithOnMainThread(taskSigned =>
                            {
                                if (taskSigned.IsCompleted)
                                {
                                    if (!taskSigned.Result || taskSigned.Result)
                                    {
                                        dbManager.UpdateSignedIn(true, task.Result.UserId);

                                        PlayerController pController = PlayerController.GetInstance();

                                        pController.CurrentUserId = task.Result.UserId;

                                        if (pController.CheckGuestPlayer())
                                            pController.DeleteFileGuest();

                                        page.ShowScreen("startScreen");

                                        return;
                                    }
                                    else
                                    {
                                        SignOutFromGoogle();
                                        StartCoroutine(page.ShowFailedScreen("Akun sedang digunakan"));
                                    }
                                }
                                if (taskSigned.IsCanceled)
                                {
                                    Debug.Log("Canceled Signed Status : " + taskSigned.Result);
                                }
                                if (taskSigned.IsFaulted)
                                {
                                    Debug.Log("Faulted Signed Status : " + taskSigned.Result);
                                }
                            });
                        }
                        else
                        {
                            page.ShowScreen("usernameForm");
                            return;
                        }
                    }

                    if (task.IsCanceled)
                    {
                        Debug.Log("Canceled");
                    }

                    if (task.IsFaulted)
                    {
                        Debug.Log("Exception : " + task.Exception);
                    }
                });

                

                
            }

            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                {
                    //ShowErrorMessage("Autentikasi Gagal");
                    DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
                    dbManager.UpdateSignedIn(false, task.Result.UserId);
                    OnSignOut();
                }
            }
        });
    }

    public void OnSignInSilently()
    {
        if(GoogleSignIn.Configuration == null)
            GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        try
        {
            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
        }
        catch (System.Exception ex)
        {
            page.ShowScreen("authScreen");
            Debug.LogError("Singin width google Exception:" + ex.Message);
        }
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    public IEnumerator Login(string username, string password)
    {
        page.ShowPopUp("loadingScreen");

        string result = "";

        yield return StartCoroutine(ApiHelper.Login(username, password, s => result = s));

        if (!string.IsNullOrEmpty(result))
        {
            DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

            dbManager.UserDataExist(result).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User Data : " + task.Result);
                    if (task.Result)
                    {
                        dbManager.GetSignedIn(result).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                if (!task.Result)
                                {
                                    PlayerController pController = PlayerController.GetInstance();

                                    pController.DeleteFileGuest();

                                    pController.CurrentUserId = result;
                                    PlayerPrefs.SetString("BarenAccount", result);
                                    PlayerPrefs.Save();
                                    dbManager.UpdateSignedIn(true, result);
                                    page.ShowPopUp("close");
                                    page.ShowScreen("startScreen");
                                }
                                else
                                {
                                    StartCoroutine(page.ShowFailedScreen("Akun sedang digunakan"));
                                    page.ShowPopUp("close");
                                }
                            }
                        });
                    }
                    else
                    {
                        page.ShowPopUp("close");
                        page.ShowScreen("usernameForm");
                        currUserId = result;
                        PlayerPrefs.SetString("BarenAccount", result);
                        PlayerPrefs.Save();
                    }
                }
            });

            
        }
        else
        {
            StartCoroutine(page.ShowFailedScreen("Login Gagal"));
        }
    }

    public IEnumerator SignUp(string email, string password)
    {
        page.ShowPopUp("loadingScreen");

        int result = -1;
        yield return StartCoroutine(ApiHelper.Register(email, password, s => result = s));

        if (result != -1)
        {
            id = result;
            page.ShowPopUp("close");
            DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
            

            page.ShowScreen("usernameForm");

            currUserId = dbManager.GetUserKey();
            PlayerPrefs.SetString("BarenAccount", currUserId);
            PlayerPrefs.Save();

            PlayerController pController = PlayerController.GetInstance();

            pController.DeleteFileGuest();
        }
        else
        {
            StartCoroutine(page.ShowFailedScreen("Registrasi akun gagal"));
        }
    }

    public void Logout()
    {
        PlayerController pController = PlayerController.GetInstance();

        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();
        

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("BarenAccount")))
        {
            dbManager.UpdateSignedIn(false, pController.CurrentUserId);
            PlayerPrefs.DeleteKey("BarenAccount");
        }
        else if (!pController.CheckGuestPlayer())
        {
            dbManager.UpdateSignedIn(false, pController.CurrentUserId);
            GoogleSignIn.DefaultInstance.SignOut();
        }
        page.ShowScreen("authScreen");
    }
}
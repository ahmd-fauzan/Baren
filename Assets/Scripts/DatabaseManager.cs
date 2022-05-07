using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.SceneManagement;


public class DatabaseManager : MonoBehaviour
{
    public InputField Name;
    public InputField Gold;
    public Text totalWinsTXT, totalLosesTXT, totalDrawsTXT, text,battlePointTXT,userNameTXT, userName2TXT, userIDTXT;


    private string userID;
    private int lastHistoryIndex = -1;

    private DatabaseReference dbReference;

    public SceneSwitcher SceneSwitcherScript;


    // Start is called before the first frame update
    void Start()
    {

        SignOut();      
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        /* CreateUser();*/
        //Register();
       /* SignIn();*/
        //GetUserInfo();

        /*       StartCoroutine(LoadPlayers());
               CreateUser();
               StartCoroutine(LoadPlayers());*/


    }


    public void CreateUser()
    {
        User newUser = new User("Shanks", 2);
        string json = JsonUtility.ToJson(newUser);
        Debug.Log("json create user: " + json);

        dbReference.Child("Players").Child(/*userID*/"DD11").SetRawJsonValueAsync(json);
    }

    public void Register()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.CreateUserWithEmailAndPasswordAsync("12345678@gmail.com", "12345678").ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            userID = newUser.UserId;
            Debug.Log("UserID" + userID);

        });
    }

    public void SignIn()
    {

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync("12345678@gmail.com", "12345678").ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }


            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            userID = newUser.UserId;
            Debug.Log("UserID" + userID);
            SceneManager.LoadScene("Profile");

            Debug.Log("sdf");
    

        });    
    }


    private void SignOut()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();
    }

    public IEnumerator GetName(Action<string> onCallback)
    {
        var userNameData = dbReference.Child("Players").Child(userID).Child("name").GetValueAsync();

        yield return new WaitUntil(predicate: () => userNameData.IsCompleted);

        if (userNameData != null)
        {
            DataSnapshot snapshot = userNameData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }

    }


    public IEnumerator GetGold(Action<int> onCallback)
    {
        var userGoldData = dbReference.Child("Players").Child(userID).Child("gold").GetValueAsync();

        yield return new WaitUntil(predicate: () => userGoldData.IsCompleted);

        if (userGoldData != null)
        {
            DataSnapshot snapshot = userGoldData.Result;
            onCallback.Invoke(int.Parse(snapshot.Value.ToString()));
        }
    }

     
    public void GetUserInfo()
    {
        StartCoroutine(GetName((string name) =>
        {
            Debug.Log("nameinfo: "+name);
        }));

        StartCoroutine(GetGold((int gold) =>
        {
            Debug.Log("goldinfo: " + gold);
        }));


    }

    public void getListPlayers()
    {
        StartCoroutine(LoadPlayers());
    }

    public IEnumerator LoadPlayers()
    {
        Debug.Log("IEstart get statistci");

        var historyReference = dbReference.Child("Players").Child(userID).Child("History");

        var DBTask = historyReference.GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                /*     Debug.Log("else load player foreach " + childSnapshot);
                     Debug.Log("else load player foreach2 " + childSnapshot.Child("name"));*/

                String HistoryID = childSnapshot.Child("HistoryID").Value.ToString();
                int battlePoint = int.Parse(childSnapshot.Child("battlePoint").Value.ToString());
                int matchResult = int.Parse(childSnapshot.Child("matchResult").Value.ToString());
                int matchStatus = int.Parse(childSnapshot.Child("matchStatus").Value.ToString());

                Debug.Log("History ID: " + HistoryID + "---------------------");
                Debug.Log("battlePoint: " + battlePoint);
                Debug.Log("matchResult: " + matchResult);
                Debug.Log("matchStatus: " + matchStatus);

                /*                Debug.Log("end else load player foreach");
                */
            }
        }

    }

    public IEnumerator IgetStatistic()
    {
        Debug.Log("IEstart get statistci");

        var historyReference = dbReference.Child("Players").Child(userID).Child("History");

        var DBTask = historyReference.GetValueAsync();

        int win = 0;
        int lose = 0;
        int draw = 0;


        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {        
                int matchResult = int.Parse(childSnapshot.Child("matchResult").Value.ToString());
                Debug.Log("matchResult: " + matchResult);

                if (matchResult == 1)     
                    win++;
                else if(matchResult==0)
                    draw++;
                else if (matchResult == -1)
                    lose++;           
            }

            Debug.Log("win lose draw:" + win + lose + draw);
            totalWinsTXT.text = win.ToString();
            totalLosesTXT.text = lose.ToString();
            totalDrawsTXT.text = draw.ToString();

        }


    }

    public void getStatistic()
    {
        Debug.Log("start get statistci");
        StartCoroutine(IgetStatistic());
    }




    public IEnumerator LoadLastIndex()
    {
        var historyReference = dbReference.Child("Players").Child(userID).Child("History");

        var DBTask = historyReference.GetValueAsync();

        int lastindex = 0;

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            int Historylength = snapshot.Children.Reverse<DataSnapshot>().Count();
        
            lastindex = Historylength - 1;
            this.lastHistoryIndex = lastindex;
            Debug.Log("last index in load last index: " + lastindex );
            createHistory();

        }
    }



    public void getLastIndexHistory()
    {
        Debug.Log("start get last index history");
        StartCoroutine(LoadLastIndex()) ;

    }

    public void createHistory()
    {
        //getLastIndexHistory();

        History newHistory = new History("HS19", 1,1,2);
        string json = JsonUtility.ToJson(newHistory);
        Debug.Log("Json: " + json + "hIndex"+this.lastHistoryIndex);
        Debug.Log(this.lastHistoryIndex);
        dbReference.Child("Players").Child(userID).Child("History").Child((this.lastHistoryIndex+1).ToString()).SetRawJsonValueAsync(json);
        Debug.Log("Create History");
    }


    public void setBattlePointAndUsernameTXT()
    {
        StartCoroutine(GetBattlePoint((int battlePoint) =>
        {
            Debug.Log("battlePointINfo: " + battlePoint);
            battlePointTXT.text = battlePoint.ToString();

            //settext batltepoint
        }));


        StartCoroutine(GetUsername((string userName) =>
        {
            Debug.Log("userName Infor: " + userName);
            userNameTXT.text = userName.ToString();

            //settext batltepoint
        }));

    }

    public IEnumerator GetBattlePoint(Action<int> onCallback)
    {
        var battlePointData = dbReference.Child("Players").Child(userID).Child("PlayerInfo").Child("battlePoint").GetValueAsync();

        yield return new WaitUntil(predicate: () => battlePointData.IsCompleted);

        if (battlePointData != null)
        {
            DataSnapshot snapshot = battlePointData.Result;
            onCallback.Invoke(int.Parse(snapshot.Value.ToString()));
        }

    }
    public IEnumerator GetUsername(Action<string> onCallback)
    {
        var userNameData = dbReference.Child("Players").Child(userID).Child("PlayerInfo").Child("username").GetValueAsync();

        yield return new WaitUntil(predicate: () => userNameData.IsCompleted);

        if (userNameData != null)
        {
            DataSnapshot snapshot = userNameData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }

    }

    public void setUserIDTXT()
    {
        userIDTXT.text = userID;
    }

    // Update is called once per frame
    void Update()
    {

    }

   
}

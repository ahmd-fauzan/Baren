using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

public static class ApiHelper
{
    private static string baseUrl = "https://api-baren.000webhostapp.com/api/";

    [System.Serializable]
    class Account
    {
        
        public string playerId;
        public string username;
        public string email;
        public string updated_at;
        public string created_at;
        public int id;
    }

    [System.Serializable]
    class Data
    {
        public Account data;
    }

    public static IEnumerator Register(string email, string password, System.Action<int> aOnData)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        //var registerTask = UnityWebRequest.Post(baseUrl + "register", form);

        //yield return new WaitUntil(predicate: () => registerTask.result == UnityWebRequest.Result.Success || registerTask.result == UnityWebRequest.Result.ConnectionError);
        //UnityWebRequest.Post(baseUrl + "register", form)
        using (UnityWebRequest request = UnityWebRequest.Post(baseUrl + "register", form))
        {
            yield return request.SendWebRequest();

            Data data = new Data();

            JsonUtility.FromJsonOverwrite(request.downloadHandler.text, data);

            if(aOnData != null)
            {
                if (request.result == UnityWebRequest.Result.Success)
                    aOnData(data.data.id);
                else
                    aOnData(-1);
            }

        }
    }

    public static IEnumerator Login(string username, string password, System.Action<string> aOnData)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using(UnityWebRequest request = UnityWebRequest.Post(baseUrl + "login", form))
        {
            yield return request.SendWebRequest();

            Data data = new Data();

            JsonUtility.FromJsonOverwrite(request.downloadHandler.text, data);

            if (request.result == UnityWebRequest.Result.Success)
                aOnData(data.data.playerId);
            else
                aOnData("");
        }
    }

    public static IEnumerator Update(int id, string username, string playerId)
    {
        Debug.Log("Updating");
        WWWForm form = new WWWForm();
        Debug.Log("ID : " + id + " Username : " + username + " : " + playerId);
        form.AddField("id", id);
        form.AddField("username", username);
        form.AddField("playerId", playerId);

        string json = "{\"id\": " + id + ", \"username\": \"" + username + "\", \"playerId\": \"" + playerId + "\"}";

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        Debug.Log("Form : " + form.data);
        Debug.Log("JSON UPDATE : " + json);

        using(UnityWebRequest request = UnityWebRequest.Post(baseUrl + "update", form))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);

            if(request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Update Success");
            }
        }
    }
}

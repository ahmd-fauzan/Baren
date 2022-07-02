using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AutentikasiPage : MonoBehaviour
{
    [SerializeField]
    AuthenticationManager manager;

    [SerializeField]
    private InputField usernameEmailIF;

    [SerializeField]
    private InputField passwordIF;

    [SerializeField]
    private Sprite emailIcon;

    [SerializeField]
    private Sprite usernameIcon;

    [SerializeField]
    private Text signSignUpText;

    [SerializeField]
    private Text askText;

    [SerializeField]
    private Image currIcon;

    [SerializeField]
    private Text buttonText;

    [SerializeField]
    private GameObject authScreen;

    [SerializeField]
    private GameObject usernameForm;

    [SerializeField]
    private GameObject startScree;

    [SerializeField]
    private Text usernameEmailErrText;

    [SerializeField]
    private Text passwordErrText;

    [SerializeField]
    private Text usernameErrText;

    [SerializeField]
    private GameObject confirmBind;

    [SerializeField]
    private GameObject loadingScreen;

    [SerializeField]
    private GameObject circleLoadingScreen;

    [SerializeField]
    private Image circleLoading;

    [SerializeField]
    private GameObject bindButton;

    [SerializeField]
    private GameObject confirmChangeAccount;

    [SerializeField]
    private GameObject failedScreen;

    [SerializeField]
    private Text failedText;

    private void Start()
    {
        //ShowScreen("authScreen");
    }

    public void ChangeAutentikasi()
    {
        if (signSignUpText.text == "Daftar")
            ShowSignUp();
        else
            ShowLogin();
    }

    private void ShowSignUp()
    {
        signSignUpText.text = "Masuk";
        askText.text = "Sudah punya akun?";
        currIcon.sprite = emailIcon;
        buttonText.text = "Daftar";
        usernameEmailIF.text = "";
        passwordIF.text = "";
        usernameEmailIF.placeholder.GetComponent<Text>().text = "Masukan Email...";
    }

    private void ShowLogin()
    {
        signSignUpText.text = "Daftar";
        askText.text = "Belum punya akun?";
        currIcon.sprite = usernameIcon;
        buttonText.text = "Masuk";
        usernameEmailIF.text = "";
        passwordIF.text = "";
        usernameEmailIF.placeholder.GetComponent<Text>().text = "Masukan Nama...";
    }

    public void LoginSignUp()
    {
        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");

        if (buttonText.text == "Masuk")
        {

            if (!regexItem.IsMatch(usernameEmailIF.text) || usernameEmailIF.text.Length > 8)
            {
                StartCoroutine(ShowErrorMessage("UsernameEmail", "Nama Tidak Valid"));
                return;
            }

            if (!regexItem.IsMatch(passwordIF.text) || passwordIF.text.Length != 8)
            {
                StartCoroutine(ShowErrorMessage("Password", "Password Tidak Valid"));
                return;
            }

            StartCoroutine(manager.Login(usernameEmailIF.text, passwordIF.text));
        }
        else
        {
            var regexEmail = new Regex(@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$");

            if (!regexEmail.IsMatch(usernameEmailIF.text))
            {
                StartCoroutine(ShowErrorMessage("UsernameEmail", "Email Tidak Valid"));
                return;
            }

            if (!regexItem.IsMatch(passwordIF.text) || passwordIF.text.Length != 8)
            {
                StartCoroutine(ShowErrorMessage("Password", "Password Tidak Valid"));
                return;
            }

            Debug.Log("Call SignUp" + usernameEmailIF.text + " Password : " + passwordIF.text);
            StartCoroutine(manager.SignUp(usernameEmailIF.text, passwordIF.text));
        }
    }

    public void ShowScreen(string screen)
    {
        usernameForm.SetActive(screen == "usernameForm");
        authScreen.SetActive(screen == "authScreen");
        startScree.SetActive(screen == "startScreen");
        loadingScreen.SetActive(screen == "loadingScreen");
        
        if(screen == "startScreen")
        {
            PlayerController pController = PlayerController.GetInstance();
            bindButton.SetActive(pController.CheckGuestPlayer());
        }
    }

    public void ShowPopUp(string popUp)
    {
        confirmBind.SetActive(popUp == "confirmBind");
        confirmChangeAccount.SetActive(popUp == "confirmChangeAccount");
        failedScreen.SetActive(popUp == "failedScreen");
        circleLoadingScreen.SetActive(popUp == "loadingScreen");
        if (popUp == "loadingScreen")
            StartCoroutine(StartLoading());
    }

    public IEnumerator ShowErrorMessage(string type, string message)
    {
        float counter = 4f;

        switch (type)
        {
            case "UsernameEmail":
                usernameEmailErrText.text = message;
                usernameEmailErrText.gameObject.SetActive(true);
                passwordErrText.gameObject.SetActive(false);
                usernameErrText.gameObject.SetActive(false);
                break;
            case "Password":
                passwordErrText.text = message;
                usernameEmailErrText.gameObject.SetActive(false);
                passwordErrText.gameObject.SetActive(true);
                usernameErrText.gameObject.SetActive(false);
                break;
            case "Username":
                usernameErrText.text = message;
                usernameEmailErrText.gameObject.SetActive(false);
                passwordErrText.gameObject.SetActive(false);
                usernameErrText.gameObject.SetActive(true);
                break;
        }

        while(counter > 0)
        {
            yield return new WaitForSeconds(1f);
            counter--;
        }

        usernameEmailErrText.gameObject.SetActive(false);
        passwordErrText.gameObject.SetActive(false);
        usernameErrText.gameObject.SetActive(false);
    }

    IEnumerator StartLoading()
    {
        float counter = 1f;
        while (counter >= 0)
        {
            yield return new WaitForSeconds(0.02f);
            circleLoading.fillAmount = counter;
            counter -= 0.02f;
        }

        StartCoroutine(StartLoading());
    }

    public IEnumerator ShowFailedScreen(string message)
    {
        ShowPopUp("failedScreen");
        failedText.text = message;

        float counter = 3f;
        while(counter >= 0)
        {
            yield return new WaitForSeconds(1f);
            counter--;
        }

        ShowPopUp("close");
    }
}

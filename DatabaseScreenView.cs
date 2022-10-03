using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/*
Tasks:

Create a Scene to Test Firebase Modules.

Main Flow Test Development:
- Login/Create/GoogleLogin Account
- Download/Create userdatabase
- Put App in Background
- Make some changes to Data
- Access some public data
- Test Rules for denying write access to data
- View Leaderboard
- Add account to current account
- Logout
*/

/* 
* Firebase Database Tester/Administration Tool
* Create a Scene to Test Firebase Auth & DataBase.
* 
* Author: Sean Z Eozmon Studios
* Version: 1.0 | 23rd August 2017
* */

public class DatabaseScreenView : MonoBehaviour, FirebaseAuth_Manager_Callback, FirebaseDB_Manager_Callback
{

    public bool DEBUG = true;
    public const DatabaseScreens DEBUG_SCREEN = DatabaseScreens.leaderboards;

    [Header("UI Panels")]
    public GameObject mainUI;
    public GameObject signUpUI;
    public GameObject databaseUI;
    public GameObject leaderboardsUI;
    public GameObject loginUI;
    public GameObject errorUI;
    public GameObject successUI;
    public GameObject consoleBackground;

    [Header("Main UI")]
    public Text textBuildNumber;
    public Text textStatus;
    public GameObject mainUISignupLogin;
    public GameObject mainUIDatabase;

    [Header("Signup UI")]
    public Text textSignupError;
    public InputField inputSignupEmail;
    public InputField inputSignupPassword;
    public InputField inputSignupDisplayName;
    public Button objbuttonSignup;

    [Header("Login UI")]
    public Text textLoginError;
    public InputField inputLoginEmail;
    public InputField inputLoginPassword;
    public Button objbuttonLogin;
    public Button objbuttonLoginGoogle;

    [Header("Database UI")]

    [Header("Leaderboards UI")]
    public Text textLeaderboardTitle;
    public GameObject leaderboardVerticalGrid;
    public GameObject leaderboardEntryPrefab;
    public GameObject leaderboardEntryShowmore;
    public LeaderboardViewController LeaderboardController;

    [Header("Buttons")]
    public GameObject objbuttonBack;
    public GameObject objbuttonLogout;
    public GameObject objbuttonDevTest;

    ScreenBackButtonStack ScreensStack; // Back button functionality needs to record "screens viewed"

    public enum DatabaseScreens
    {
        NA,
        initial,
        login,
        signup,
        database,
        leaderboards
    }

    // Checks to see if firebase is active
    void Start () {
        textBuildNumber.text = "Build: " + Application.version; // Build version
        if (FirebaseAuth_Manager.instance == null || FirebaseDB_Manager.instance == null) {
            ShowError("Firebase has not started");
        } else
        {
            Initialize();
        }
	}

    void Initialize()
    {
        if (true) //Application.genuineCheckAvailable && Application.genuine)
        {
            FirebaseAuth_Manager.instance.Callback = this;
            FirebaseDB_Manager.instance.Callback = this;

            // UI Changes
            ScreensStack = new ScreenBackButtonStack(objbuttonBack);
            SetScreen(DatabaseScreens.initial);
            errorUI.SetActive(false);
            successUI.SetActive(false);

            //Leaderboard Controller Instantiate
            LeaderboardController = new LeaderboardViewController(leaderboardVerticalGrid, leaderboardEntryPrefab, leaderboardEntryShowmore);

            // Authentication check
            UpdateMainUIState();

            // Debug autofill forums
            if (DEBUG)
            {
                setDefaultCredentials();
                if (DEBUG_SCREEN != DatabaseScreens.NA)
                {
                    SetScreen(DEBUG_SCREEN);
                }
            }
            Debug.Log("Started DB ScreenView");
        } else
        {
            //ShowError("App Not Genuine");
        }

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            buttonBack();
        }
    }

    // Sets Screens for the UI and starts appropriate update tasks to the selected screen
    public void SetScreen(DatabaseScreens screen, bool addToStack = true)
    {
        if (FirebaseAuth_Manager.instance == null)
        {
            return;
        }
        mainUI.SetActive(false);
        signUpUI.SetActive(false);
        databaseUI.SetActive(false);
        leaderboardsUI.SetActive(false);
        loginUI.SetActive(false);
        switch (screen)
        {
            case DatabaseScreens.NA:
                ShowError("Unable to load Screen: 'NA'");
                break; ;
            case DatabaseScreens.initial:
                if(DEBUG)
                {
                    setDefaultCredentials();
                }
                mainUI.SetActive(true);
                UpdateMainUIState();
                break;
            case DatabaseScreens.signup:
                textSignupError.text = "";
                signUpUI.SetActive(true);
                break;
            case DatabaseScreens.login:
                textLoginError.text = "";
                loginUI.SetActive(true);
                break;
            case DatabaseScreens.database:
                databaseUI.SetActive(true);
                break;
            case DatabaseScreens.leaderboards:
                textLeaderboardTitle.text = "Leaderboard Global";
                //FirebaseDB_Manager.instance.LeaderboardDB.GetLeaderBoard();
                var leaderboardList = FirebaseDB_Manager.instance.LeaderboardDB.GetLeaderBoard();
                LeaderboardController.UpdateLeaderboard(leaderboardList);
                leaderboardsUI.SetActive(true);
                break;
            default:
                ShowError("Unable to load Screen: " + screen.ToString());
                addToStack = false;
                break;
        }
        if (addToStack)
        {
            ScreensStack.AddCurrentScene(screen);
        }
    }

    private void CreateUser()
    {
        objbuttonSignup.interactable = false;
        if (RegExCheckEmail.CheckEmail.IsEmail(inputSignupEmail.text)) {
            FirebaseAuth_Manager.instance.email = inputSignupEmail.text;
            FirebaseAuth_Manager.instance.password = inputSignupPassword.text;
            FirebaseAuth_Manager.instance.displayName = inputSignupDisplayName.text;
            FirebaseAuth_Manager.instance.CreateUser();
        } else
        {
            textSignupError.text = ("Email not accepted");
            objbuttonSignup.interactable = true;
        }
    }

    // Callback that runs if FirebaseAuth_Manager was able to auth fine
    public void CreateUserFinished(bool value, string username)
    {
        objbuttonSignup.interactable = true;
        if (value == true)
        {
            ShowSuccess("Created user\n\n" + username);
            inputSignupEmail.text = "";
            inputSignupPassword.text = "";
            inputSignupDisplayName.text = "";
            buttonBack();
        }
    }

    private void SignIn()
    {
        objbuttonLogin.interactable = false;
        objbuttonLoginGoogle.interactable = false;
        if (RegExCheckEmail.CheckEmail.IsEmail(inputLoginEmail.text))
        {
            FirebaseAuth_Manager.instance.email = inputLoginEmail.text;
            FirebaseAuth_Manager.instance.password = inputLoginPassword.text;
            inputLoginPassword.text = "";
            FirebaseAuth_Manager.instance.SigninWithCredential();
        }
        else
        {
            textLoginError.text = "Email not accepted";
            objbuttonLogin.interactable = true;
            objbuttonLoginGoogle.interactable = true;
        }
    }

    public void SignInFinished(bool value, string username)
    {
        if (value == true)
        {
            ShowSuccess("Signedin with\n\n" + username);
            inputLoginEmail.text = "";
            buttonBack();
        } else
        {
            textLoginError.text = "Incorrect Username or Password";
            objbuttonLogin.interactable = true;
            objbuttonLoginGoogle.interactable = true;
        }
    }

    public void UpdateMainUIState()
    {
        bool authenticated = false;
        if (FirebaseAuth_Manager.instance.IsAutheticated())
        {
            Firebase.Auth.FirebaseUser user = FirebaseAuth_Manager.instance.GetUserInfo();
            if (user != null)
            {
                mainUISignupLogin.SetActive(false);
                mainUIDatabase.SetActive(true);
                if (user.DisplayName != null)
                {
                    textStatus.text = (user.DisplayName == "") ? user.Email : user.DisplayName;
                    authenticated = true;
                }
                else if (user.Email != null)
                {
                    textStatus.text = user.Email;
                    authenticated = true;
                    ShowError("No Username");
                } else
                {
                    ShowError("Main UI Update Authentication Error");
                }
            }
        }
        if (!authenticated)
        {
            textStatus.text = "Not Logged in";
            mainUISignupLogin.SetActive(true);
            mainUIDatabase.SetActive(false);
            objbuttonLogin.interactable = true;
            objbuttonLoginGoogle.interactable = true;
        }
    }

    public void ShowAuthStatus(string status)
    {
        textStatus.text = status;
    }
    #region DIALOGBOXES
    public void ShowSuccess(string text)
    {
        TextUIShow(successUI, text);
    }

    public void ShowError(string text)
    {
        TextUIShow(errorUI, text, true);
    }

    private void TextUIShow(GameObject UI, string text, bool vibrate = false)
    {
        Text uiText = UI.GetComponentInChildren<Text>();
        if (text == "c") // Hide UI if text is "c" for close
        {
            UI.SetActive(false);
            uiText.text = "";
        }
        else
        {
            if (vibrate)
                Handheld.Vibrate();
            if (UI == errorUI)
                text = "ERROR:\n\n" + text;

            uiText.text = text;
            UI.SetActive(true);
        }
    }

    public void ToggleConsole()
    {
        FirebaseAuth_Manager.instance.showGUI = !FirebaseAuth_Manager.instance.showGUI;
        consoleBackground.SetActive(!consoleBackground.activeSelf);
    }
    #endregion

    #region BUTTONS
    public void buttonBack()
    {
        SetScreen(ScreensStack.GetPreviousScene(), false);
    }
    public void buttonShowSignUp()
    {
        SetScreen(DatabaseScreens.signup);
    }
    public void buttonShowDatabase()
    {
        SetScreen(DatabaseScreens.database);
    }
    public void buttonShowLogin()
    {
        SetScreen(DatabaseScreens.login);
    }
    public void buttonLogOut()
    {
        FirebaseAuth_Manager.instance.SignOut();
        UpdateMainUIState();
    }
    public void buttonSignIn()
    {
        SignIn();
    }
    public void buttonCreateUser()
    {
        CreateUser();
    }
    public void buttonGoogleSignIn()
    {
        SignIn();
    }
    public void buttonShowLeaderboards()
    {
        SetScreen(DatabaseScreens.leaderboards);
    }
    public void buttonShowGlobalLeaderboards()
    {
        textLeaderboardTitle.text = "Leaderboard Global";
        var leaderboardList = FirebaseDB_Manager.instance.LeaderboardDB.GetLeaderBoard();
        LeaderboardController.UpdateLeaderboard(leaderboardList);
    }
    public void buttonShowRegionalLeaderboards()
    {
        textLeaderboardTitle.text = "Leaderboard Regional";
        var leaderboardList = FirebaseDB_Manager.instance.LeaderboardDB.GetLeaderBoard();
        LeaderboardController.UpdateLeaderboard(leaderboardList);
    }
    public void buttonShowClanLeaderboards()
    {
        textLeaderboardTitle.text = "Leaderboard Clan";
        var leaderboardList = FirebaseDB_Manager.instance.LeaderboardDB.GetLeaderBoard();
        LeaderboardController.UpdateLeaderboard(leaderboardList);
    }
    #endregion

    #region DEBUG
    private void setDefaultCredentials()
    {
        inputSignupEmail.text = "text@test.com";
        inputSignupPassword.text = "12345678";
        inputSignupDisplayName.text = "textApp1";
        inputLoginEmail.text = "text@test.com";
        inputLoginPassword.text = "12345678";
    }
    #endregion 
}

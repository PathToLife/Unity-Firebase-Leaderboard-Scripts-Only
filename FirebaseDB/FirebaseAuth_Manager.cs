using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface FirebaseAuth_Manager_Callback
{
    void CreateUserFinished(bool value, string output);
    void SignInFinished(bool value, string output);
    void ShowAuthStatus(string status);
    void ShowError(string error);
    void ShowSuccess(string success);
}

public class FirebaseAuth_Manager : MonoBehaviour
{
    public FirebaseAuth_Manager_Callback Callback; // Remeber to assign "FirebaseAuth_Manager.instance.Callback = this;" in the GUI class Initialize()!
    public static FirebaseAuth_Manager instance;

    [Header("Settings")]
    public bool useAdminAccount = true;
    public bool showGUI = false;

    protected Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth = new Dictionary<string, Firebase.Auth.FirebaseUser>();

    private string logText = "";
    public string email = "";
    public string password = "";
    public string displayName = "";

    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;
    // Enable / disable password input box.
    // NOTE: In some versions of Unity the password input box does not work in
    // iOS simulators.
    public bool showPasswordInput = false;
    private Vector2 controlsScrollViewVector = Vector2.zero;
    private Vector2 scrollViewVector = Vector2.zero;
    bool UIEnabled = true;

    // Options used to setup secondary authentication object.
    // Created in InitializeFirebase, because of a bug where creating an
    // AppOptions at declaration is causing a crash.
    private Firebase.AppOptions otherAuthOptions;

    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start()
    {
        dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
        if (dependencyStatus != Firebase.DependencyStatus.Available)
        {
            Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    instance = this;
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                        "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
        else
        {
            instance = this;
            InitializeFirebase();
        }
    }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase()
    {
        DebugLog("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        // Specify valid options to construct a secondary authentication object.
        otherAuthOptions = new Firebase.AppOptions
        {
            ApiKey = "",
            AppId = "",
            ProjectId = ""
        };
        if (otherAuthOptions != null &&
            !(String.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
              String.IsNullOrEmpty(otherAuthOptions.AppId) ||
              String.IsNullOrEmpty(otherAuthOptions.ProjectId)))
        {
            try
            {
                otherAuth = Firebase.Auth.FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
                  otherAuthOptions, "Secondary"));
                otherAuth.StateChanged += AuthStateChanged;
                otherAuth.IdTokenChanged += IdTokenChanged;
            }
            catch (Exception)
            {
                DebugLog("ERROR: Failed to initialize secondary authentication object.");
            }
        }
        AuthStateChanged(this, null);
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth.IdTokenChanged -= IdTokenChanged;
        auth = null;
        if (otherAuth != null)
        {
            otherAuth.StateChanged -= AuthStateChanged;
            otherAuth.IdTokenChanged -= IdTokenChanged;
            otherAuth = null;
        }
    }

    void DisableUI()
    {
        UIEnabled = false;
    }

    void EnableUI()
    {
        UIEnabled = true;
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        Debug.Log(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }
        scrollViewVector.y = int.MaxValue;
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                DebugLog("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                DebugLog("Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                DisplayDetailedUserInfo(user, 1);
            }
        }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => DebugLog(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            DebugLog(task.Exception.ToString());
            Callback.ShowError(task.Exception.ToString());
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    public void CreateUser()
    {
        DebugLog(String.Format("Attempting to create user {0}...", email));
        DisableUI();

        // This passes the current displayName through to HandleCreateResult
        // so that it can be passed to UpdateUserProfile().  displayName will be
        // reset by AuthStateChanged() when the new user is created and signed in.
        string newDisplayName = displayName;
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWith((task) => HandleCreateResult(task, newDisplayName: newDisplayName));
    }

    void HandleCreateResult(Task<Firebase.Auth.FirebaseUser> authTask,
                            string newDisplayName = null)
    {
        EnableUI();
        // Pop the success/fail result back to the UI management system, on fail will return empty string
        // Note we also chack if username is empty, in which case email string is sent instead
        Callback.CreateUserFinished(auth.CurrentUser != null, auth.CurrentUser != null ? (auth.CurrentUser.DisplayName == "") ? auth.CurrentUser.Email : auth.CurrentUser.DisplayName : "");

        if (LogTaskCompletion(authTask, "User Creation"))
        {
            if (auth.CurrentUser != null)
            {
                DebugLog(String.Format("User Info: {0}  {1}", auth.CurrentUser.Email,
                                       auth.CurrentUser.ProviderId));
                UpdateUserProfile(newDisplayName: newDisplayName);
            }
        }
    }

    // Update the user's display name with the currently selected display name.
    public void UpdateUserProfile(string newDisplayName = null)
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to update user profile");
            return;
        }
        displayName = newDisplayName ?? displayName;
        DebugLog("Updating user profile");
        DisableUI();
        auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
        {
            DisplayName = displayName,
            PhotoUrl = auth.CurrentUser.PhotoUrl,
        }).ContinueWith(HandleUpdateUserProfile);
    }

    void HandleUpdateUserProfile(Task authTask)
    {
        EnableUI();
        if (LogTaskCompletion(authTask, "User profile"))
        {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void Signin()
    {
        DebugLog(String.Format("Attempting to sign in as {0}...", email));
        DisableUI();
        auth.SignInWithEmailAndPasswordAsync(email, password)
          .ContinueWith(HandleSigninResult);
    }

    // This is functionally equivalent to the Signin() function.  However, it
    // illustrates the use of Credentials, which can be aquired from many
    // different sources of authentication.
    public void SigninWithCredential()
    {
        DebugLog(String.Format("Attempting to sign in as {0}...", email));
        DisableUI();
        Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
    }

    // Attempt to sign in anonymously.
    public void SigninAnonymously()
    {
        DebugLog("Attempting to sign anonymously...");
        DisableUI();
        auth.SignInAnonymouslyAsync().ContinueWith(HandleSigninResult);
    }

    void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask)
    {
        EnableUI();
        Callback.SignInFinished(authTask.IsCompleted, (authTask.Result.DisplayName == "") ? authTask.Result.Email : authTask.Result.DisplayName);
        LogTaskCompletion(authTask, "Sign-in");
    }

    void LinkWithCredential()
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to link credential to user.");
            return;
        }
        DebugLog("Attempting to link credential to user...");
        Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        auth.CurrentUser.LinkWithCredentialAsync(cred).ContinueWith(HandleLinkCredential);
    }

    void HandleLinkCredential(Task authTask)
    {
        if (LogTaskCompletion(authTask, "Link Credential"))
        {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void ReloadUser()
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to reload user.");
            return;
        }
        DebugLog("Reload User Data");
        auth.CurrentUser.ReloadAsync().ContinueWith(HandleReloadUser);
    }

    void HandleReloadUser(Task authTask)
    {
        if (LogTaskCompletion(authTask, "Reload"))
        {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void GetUserToken()
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to get token.");
            return;
        }
        DebugLog("Fetching user token");
        fetchingToken = true;
        auth.CurrentUser.TokenAsync(false).ContinueWith(HandleGetUserToken);
    }

    void HandleGetUserToken(Task<string> authTask)
    {
        fetchingToken = false;
        if (LogTaskCompletion(authTask, "User token fetch"))
        {
            DebugLog("Token = " + authTask.Result);
        }
    }

    public Firebase.Auth.FirebaseUser GetUserInfo()
    {
        if (auth.CurrentUser == null)
        {
            displayName = "not logged in";
            DebugLog("Not signed in, unable to get info.");
            return null;
        }
        else
        {
            DebugLog("Current user info:");
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
            return auth.CurrentUser;
        }
    }

    public bool IsAutheticated()
    {
        return auth.CurrentUser != null;
    }

    // Display user information.
    void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
        foreach (var property in userProperties)
        {
            if (!String.IsNullOrEmpty(property.Value))
            {
                DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    // Display a more detailed view of a FirebaseUser.
    void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
    {
        DisplayUserInfo(user, indentLevel);
        DebugLog("  Anonymous: " + user.IsAnonymous);
        DebugLog("  Email Verified: " + user.IsEmailVerified);
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        if (providerDataList.Count > 0)
        {
            DebugLog("  Provider Data:");
            foreach (var providerData in user.ProviderData)
            {
                DisplayUserInfo(providerData, indentLevel + 1);
            }
        }
    }

    public void SignOut()
    {
        DebugLog("Signing out.");
        Callback.ShowSuccess("Signed out");
        auth.SignOut();
    }

    public void DeleteUser()
    {
        if (auth.CurrentUser != null)
        {
            DebugLog(String.Format("Attempting to delete user {0}...", auth.CurrentUser.UserId));
            DisableUI();
            auth.CurrentUser.DeleteAsync().ContinueWith(HandleDeleteResult);
        }
        else
        {
            DebugLog("Sign-in before deleting user.");
        }
    }

    void HandleDeleteResult(Task authTask)
    {
        EnableUI();
        LogTaskCompletion(authTask, "Delete user");
    }

    // Show the providers for the current email address.
    public void DisplayProvidersForEmail()
    {
        auth.FetchProvidersForEmailAsync(email).ContinueWith((authTask) =>
        {
            if (LogTaskCompletion(authTask, "Fetch Providers"))
            {
                DebugLog(String.Format("Email Providers for '{0}':", email));
                foreach (string provider in authTask.Result)
                {
                    DebugLog(provider);
                }
            }
        });
    }

    // Send a password reset email to the current email address.
    public void SendPasswordResetEmail()
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWith((authTask) =>
        {
            if (LogTaskCompletion(authTask, "Send Password Reset Email"))
            {
                DebugLog("Password reset email sent to " + email);
            }
        });
    }

    // Determines whether another authentication object is available to focus.
    public bool HasOtherAuth { get { return auth != otherAuth && otherAuth != null; } }

    // Swap the authentication object currently being controlled by the application.
    public void SwapAuthFocus()
    {
        if (!HasOtherAuth) return;
        var swapAuth = otherAuth;
        otherAuth = auth;
        auth = swapAuth;
        DebugLog(String.Format("Changed auth from {0} to {1}",
                               otherAuth.App.Name, auth.App.Name));
    }

    // Render the log output in a scroll view.
    void GUIDisplayLog()
    {
        scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
        GUILayout.Label(logText);
        GUILayout.EndScrollView();
    }

    // Render the buttons and other controls.
    void GUIDisplayControls()
    {
        if (UIEnabled)
        {
            controlsScrollViewVector = GUILayout.BeginScrollView(controlsScrollViewVector);
            GUILayout.BeginVertical();
            GUILayout.Space(150);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Email:", GUILayout.Width(Screen.width * 0.20f));
            email = GUILayout.TextField(email);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", GUILayout.Width(Screen.width * 0.20f));
            password = showPasswordInput ?  GUILayout.TextField(password) : GUILayout.PasswordField(password, '*');
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("DisplayName:", GUILayout.Width(Screen.width * 0.20f));
            displayName = GUILayout.TextField(displayName);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (GUILayout.Button("Create User", GUILayout.Height(50)))
            {
                CreateUser();
            }
            if (GUILayout.Button("Sign In Anonymously", GUILayout.Height(50)))
            {
                SigninAnonymously();
            }
            if (GUILayout.Button("Sign In With Email", GUILayout.Height(50)))
            {
                Signin();
            }
            if (GUILayout.Button("Sign In With Credentials", GUILayout.Height(50)))
            {
                SigninWithCredential();
            }
            if (GUILayout.Button("Link With Credential", GUILayout.Height(50)))
            {
                LinkWithCredential();
            }
            if (GUILayout.Button("Reload User", GUILayout.Height(50)))
            {
                ReloadUser();
            }
            if (GUILayout.Button("Get User Token", GUILayout.Height(50)))
            {
                GetUserToken();
            }
            if (GUILayout.Button("Get User Info", GUILayout.Height(50)))
            {
                GetUserInfo();
            }
            if (GUILayout.Button("Sign Out", GUILayout.Height(50)))
            {
                SignOut();
            }
            if (GUILayout.Button("Delete User", GUILayout.Height(50)))
            {
                DeleteUser();
            }
            if (GUILayout.Button("Show Providers For Email", GUILayout.Height(50)))
            {
                DisplayProvidersForEmail();
            }
            if (GUILayout.Button("Password Reset Email", GUILayout.Height(50)))
            {
                SendPasswordResetEmail();
            }
            if (HasOtherAuth && GUILayout.Button(String.Format("Switch to auth object {0}",
                                                               otherAuth.App.Name)))
            {
                SwapAuthFocus();
            }

            if (GUILayout.Button("Vibrate!", GUILayout.Height(50)))
            {
                Handheld.Vibrate();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }

    // Render the GUI:
    void OnGUI()
    {
        if (showGUI)
        {
            if (dependencyStatus != Firebase.DependencyStatus.Available)
            {
                GUILayout.Label("One or more Firebase dependencies are not present.");
                GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
                return;
            }
            Rect logArea, controlArea;

            // Landscape mode
            logArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
            controlArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f, Screen.height);

            GUILayout.BeginArea(logArea);
            GUIDisplayLog();
            GUILayout.EndArea();

            GUILayout.BeginArea(controlArea);
            GUIDisplayControls();
            GUILayout.EndArea();
        }
    }
}
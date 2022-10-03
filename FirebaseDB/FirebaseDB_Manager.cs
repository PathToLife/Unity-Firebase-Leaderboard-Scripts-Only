using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;

/* 
* Firebase Database Tester/Administration Tool
* Create a Scene to Test Firebase Auth & DataBase.
* 
* Author: Sean Z Eozmon Studios
* Version: 0.1 | 28th August 2017
* */

public interface FirebaseDB_Manager_Callback
{
    void CreateUserFinished(bool value, string output);
    void SignInFinished(bool value, string output);
    void ShowAuthStatus(string status);
    void ShowError(string error);
    void ShowSuccess(string success);
}

public class FirebaseDB_Manager : MonoBehaviour
{

    public FirebaseDB_Manager_Callback Callback;
    public static FirebaseDB_Manager instance;

    public const string DatabaseURL = "https://fir-db-test-f633c.firebaseio.com";
    public FirebaseDB_Leaderboards LeaderboardDB;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    void Start()
    {
        instance = this;
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DatabaseURL); // Apply database URL
        LeaderboardDB = new FirebaseDB_Leaderboards(); // This is the leaderboard class that gets and sets leaderboard data
    }
}

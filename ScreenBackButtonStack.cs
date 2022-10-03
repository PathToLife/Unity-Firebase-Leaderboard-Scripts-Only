using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
* Storage for the trail of screens visited by the user
* 
* Initial Source: https://forum.unity3d.com/threads/how-to-program-the-back-button-of-android-phone-through-unity.82311/
* Authors: Sean Z Eozmon Studios, JoeriVDE
* Version: 1.0 | 24th August 2017
* */

public class ScreenBackButtonStack {

    //A list to keep track of all the scenes you've loaded so far
    private List<DatabaseScreenView.DatabaseScreens> previousScenes = new List<DatabaseScreenView.DatabaseScreens>();

    public GameObject backButton;

    public ScreenBackButtonStack(GameObject backButton)
    {
        backButton.SetActive(false); //OPTIONAL: deactivate the button in your first scene, logically because there are no previous scenes  
        this.backButton = backButton;
    }

    public void AddCurrentScene(DatabaseScreenView.DatabaseScreens currentScreen)
    {
        previousScenes.Add(currentScreen);
        if (previousScenes.Count > 1)
        {
            backButton.SetActive(true);
        }
    }

    // Peek at current screen without editing the list
    public DatabaseScreenView.DatabaseScreens PeekCurrentScene()
    {
        if (previousScenes != null && previousScenes.Count != 0)
        {
            return previousScenes[previousScenes.Count - 1];
        } else
        {
            return DatabaseScreenView.DatabaseScreens.NA;
        }
    }

    // Every time back is pressed, Remove screen from list and return it so it can be activated
    public DatabaseScreenView.DatabaseScreens GetPreviousScene()
    {
        DatabaseScreenView.DatabaseScreens previousScene = DatabaseScreenView.DatabaseScreens.NA;

        //Check wether you're not back at your original scene (index 0)
        if (previousScenes.Count > 1)
        {
            previousScene = previousScenes[previousScenes.Count - 2]; //Get the last previously loaded scene name from the list
            previousScenes.RemoveAt(previousScenes.Count - 1); //Remove the last previously loaded scene name from the list
            if (previousScenes.Count == 1)
            {
                backButton.SetActive(false);
            }
            return previousScene;
        }
        else
        {
            previousScene = previousScenes[0]; //0 will always be your first scene
            backButton.SetActive(false); //The else is optional if you want the button to be deactivated when returning to the first scene
            return previousScene;
        }
    }
}

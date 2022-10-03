using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardViewController {

    private GameObject scrollRectParent;
    private GameObject leaderboardEntryPrefab;
    private GameObject leaderboardShowMorePrefab;
    public int maxNumberToShow = 5; // Number of entries to load into the UI in one go

    public LeaderboardViewController(GameObject scrollRectParent, GameObject leaderboardEntryPrefab) {
        this.scrollRectParent = scrollRectParent;
        this.leaderboardEntryPrefab = leaderboardEntryPrefab;
    }

    public LeaderboardViewController(GameObject scrollRectParent, GameObject leaderboardEntryPrefab, GameObject leaderboardShowMorePrefab)
    {
        this.scrollRectParent = scrollRectParent;
        this.leaderboardEntryPrefab = leaderboardEntryPrefab;
        this.leaderboardShowMorePrefab = leaderboardShowMorePrefab;
    }

    // Instantiate Leaderboard entry Gameobject/prefab and apply the correct data to it.
    // WARNING, PREFAB COMPONENTS MUST BE IN THE RIGHT ORDER!!!
    public void UpdateLeaderboard(List<DataStructs.LeaderboardEntry> leaderboardList) {
        DestroyChildren(scrollRectParent);

        leaderboardList.Reverse();

        int numberToShow = leaderboardList.Count;
        if(leaderboardList.Count > maxNumberToShow) numberToShow = maxNumberToShow;

        for (int i = 0; i < numberToShow; i++)
        {
            DataStructs.LeaderboardEntry entry = leaderboardList[i];
            GameObject newEntryGameObject = MonoBehaviour.Instantiate(leaderboardEntryPrefab, scrollRectParent.transform);

            // Apply the text
            Text[] textArray = newEntryGameObject.GetComponentsInChildren<Text>();
            FormatPrefabText(textArray, entry);

            // Apply Top 3 Entries Special Formats
            if (i < 3)
            {
                // Remove Text Rank Gameobject but store positional data for our new Gameobject that is about to be created
                Vector3 postionalData = textArray[0].gameObject.transform.localPosition;
                MonoBehaviour.Destroy(textArray[0].gameObject);

                // Create new game object and set image to be from the resources folder according to "i" value
                GameObject placingGameObject = new GameObject("Image Rank");
                Image image = placingGameObject.AddComponent<Image>() as Image;
                Sprite leaderboardPlacingImage = Resources.Load<Sprite>("Leaderboard/placing" + (i+1).ToString()); // Get leaderboard image
                image.sprite = leaderboardPlacingImage;
                image.preserveAspect = true;

                // Position the new gameobject correctly and use the "RectTransform" Cast to modify the size of the image
                placingGameObject.transform.SetParent(newEntryGameObject.transform.transform);
                placingGameObject.transform.localScale = new Vector3(1, 1, 1);
                placingGameObject.transform.localPosition = postionalData;
                RectTransform positionRect = (RectTransform)placingGameObject.transform;
                positionRect.sizeDelta = new Vector2(127, 63.5f);

                // Set colours of the scores =)
                switch (i)
                {
                    case 0:
                        textArray[3].color = new Color32(255, 192, 35, 255);
                        break;
                    case 1:
                        textArray[3].color = new Color32(240, 240, 240, 255);
                        break;
                    case 2:
                        textArray[3].color = new Color32(216, 75, 54, 255);
                        break;
                    default:
                        textArray[3].color = new Color32(137, 137, 137, 255);
                        break;
                }
            }

            // Apply Region Images
            Image[] imageArray = newEntryGameObject.GetComponentsInChildren<Image>();
            imageArray[1].sprite = Resources.Load<Sprite>("Avatars/" + entry.profilePicture.ToString());
            imageArray[3].sprite = Resources.Load<Sprite>("Countryflags/" + entry.region);

            newEntryGameObject.transform.SetSiblingIndex(i);
        }

        AddMyLeaderBoardEntry(FirebaseDB_Leaderboards.GenerateRandomEntry(560412));
        AddShowMoreEntry();
    }

    public void AddMyLeaderBoardEntry(DataStructs.LeaderboardEntry entry)
    {
        if (entry.leaderboardPlacing > 4)
        {
            GameObject newEntryGameObject = MonoBehaviour.Instantiate(leaderboardEntryPrefab, scrollRectParent.transform);

            // Apply the text
            Text[] textArray = newEntryGameObject.GetComponentsInChildren<Text>();
            FormatPrefabText(textArray, entry);

            // Create a background with a color
            Image image = newEntryGameObject.AddComponent<Image>() as Image;
            image.color = new Color32(248, 120, 0, 136);

            RectTransform rectTransfrom = (RectTransform)newEntryGameObject.transform;
            rectTransfrom.SetSiblingIndex(4);
        }
    }

    public void AddShowMoreEntry() {
        GameObject newEntryGameObject = MonoBehaviour.Instantiate(leaderboardShowMorePrefab, scrollRectParent.transform);
        newEntryGameObject.transform.SetAsLastSibling();
    }

    private void FormatPrefabText(Text[] textArray, DataStructs.LeaderboardEntry entry)
    {
        // Apply the text
        textArray[0].text = entry.leaderboardPlacing.ToString();
        textArray[1].text = entry.username;
        textArray[2].text = entry.tagLine.ToString();
        textArray[3].text = entry.score.ToString();
        textArray[3].color = new Color32(203, 203, 203, 255);
    }

    private void DestroyChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }
}

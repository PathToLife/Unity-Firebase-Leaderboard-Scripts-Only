using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase.Database;

public class FirebaseDB_Leaderboards
{
    public const int maxEntries = 100000;
    private string username;
    private long score;

    // Currently get all Data from a leaderboard, can change to get 20 increments at a time....
    public List<DataStructs.LeaderboardEntry> GetLeaderBoard()
    {
        return GenerateRandomLeaderboard();
    }

    private ArrayList GetLeaderboardFromServer()
    {
        ArrayList leaderBoard = new ArrayList();

        FirebaseDatabase.DefaultInstance
          .GetReference("entries").OrderByChild("score")
          .ValueChanged += (object sender2, ValueChangedEventArgs e2) =>
          {
              if (e2.DatabaseError != null)
              {
                  Debug.LogError(e2.DatabaseError.Message);
                  return;
              }
              string title = leaderBoard[0].ToString();
              leaderBoard.Clear();
              leaderBoard.Add(title);
              if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0)
              {
                  foreach (var childSnapshot in e2.Snapshot.Children)
                  {
                      if (childSnapshot.Child("score") == null
                    || childSnapshot.Child("score").Value == null)
                      {
                          Debug.LogError("Bad data in sample.  Did you forget to call SetEditorDatabaseUrl with your project id?");
                          break;
                      }
                      else
                      {
                          leaderBoard.Insert(1, childSnapshot.Child("score").Value.ToString()
                        + "  " + childSnapshot.Child("email").Value.ToString());
                      }
                  }
              }
          };

        return leaderBoard;
    }

    public void AddScore(long score, string username)
    {
        this.score = score; // We do this so that the Asychronous "Transaction" we do is able to access values
        this.username = username;

        if (score == 0 || string.IsNullOrEmpty(username))
        {
            Debug.LogError("invalid score or email.");
            return;
        }

        Debug.Log(String.Format("Attempting to add score {0} {1}", username, score.ToString()));

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("entries");

        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(AddScoreTransaction).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception.ToString());
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Transaction complete.");
            }
        });
    }

    // A realtime database transaction receives MutableData which can be modified
    // and returns a TransactionResult which is either TransactionResult.Success(data) with
    // modified data or TransactionResult.Abort() which stops the transaction with no changes.
    private TransactionResult AddScoreTransaction(MutableData mutableData)
    {
        List<object> entries = mutableData.Value as List<object>;

        if (entries == null)
        {
            entries = new List<object>();
        }
        else if (mutableData.ChildrenCount >= maxEntries)
        {
            // If the current list of scores is greater or equal to our maximum allowed number,
            // we see if the new score should be added and remove the lowest existing score.
            long minScore = long.MaxValue;
            object minVal = null;
            foreach (var child in entries)
            {
                if (!(child is Dictionary<string, object>))
                    continue;
                long childScore = (long)((Dictionary<string, object>)child)["score"];
                if (childScore < minScore)
                {
                    minScore = childScore;
                    minVal = child;
                }
            }
            // If the new score is lower than the current minimum, we abort.
            if (minScore > score)
            {
                return TransactionResult.Abort();
            }
            // Otherwise, we remove the current lowest to be replaced with the new score.
            entries.Remove(minVal);
        }

        // Now we add the new score as a new entry that contains the email address and score.
        Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
        newScoreMap["score"] = score;
        newScoreMap["username"] = username;
        entries.Add(newScoreMap);

        // You must set the Value to indicate data at that location has changed.
        mutableData.Value = entries;
        return TransactionResult.Success(mutableData);
    }

    public static unsafe List<DataStructs.LeaderboardEntry> GenerateRandomLeaderboard()
    {
        // Generate mockup leaderbaord for now
        List<DataStructs.LeaderboardEntry> leaderboard = new List<DataStructs.LeaderboardEntry>();
        long currentScore = 156314;
        for (int i = 100; i > 0; i--)
        {
            leaderboard.Add(GenerateRandomEntry(i, &currentScore));
        }
        return leaderboard;
    }

    // Generate with respect to what the last score was
    public static unsafe DataStructs.LeaderboardEntry GenerateRandomEntry(int leaderboardPlacing, long* currentScore)
    {
        DataStructs.PlayerRank rank = (DataStructs.PlayerRank)UnityEngine.Random.Range(0, 7);
        string username = "Tester" + leaderboardPlacing.ToString() + UnityEngine.Random.Range(10, 99).ToString();
        string region = "NZ";

        int tagLine = 0;
        int profilePicture = UnityEngine.Random.Range(0, 6);

        *currentScore += UnityEngine.Random.Range(1000, 50000);
        var leaderboardEntry = new DataStructs.LeaderboardEntry(leaderboardPlacing, username, region, rank, *currentScore, tagLine, profilePicture);

        return leaderboardEntry;
    }
    // Generate anything between ranks 100k and 999k without respect to score =P
    public static DataStructs.LeaderboardEntry GenerateRandomEntry(int leaderboardPlacing)
    {
        DataStructs.PlayerRank rank = (DataStructs.PlayerRank)UnityEngine.Random.Range(0, 7);
        string username = "Tester" + leaderboardPlacing.ToString() + UnityEngine.Random.Range(10, 99).ToString();
        string region = "NZ";

        int tagLine = 0;
        int profilePicture = UnityEngine.Random.Range(0, 6);

        long score = UnityEngine.Random.Range(100000, 900000);
        var leaderboardEntry = new DataStructs.LeaderboardEntry(leaderboardPlacing, username, region, rank, score, tagLine, profilePicture);

        return leaderboardEntry;
    }
}

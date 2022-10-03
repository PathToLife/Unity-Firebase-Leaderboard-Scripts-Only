using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructs
{
    public enum PlayerRank
    {
        XS,
        S,
        APlus,
        A,
        BPlus,
        B,
        CPlus,
        C,
        Unranked
    }

    [Serializable]
    public struct PlayerEntity
    {
        public string UID;
        public string username;
        public string stringTagline;
        public string region;

        public PlayerRank rank;

        public int tagLine;
        public int profilePicture;
        public long battlePoints;
        public long playerPoints;
        public int[] winLossDraws;

        PlayerEntity(string UID, string username, string region, PlayerRank rank, int tagLine, int profilePicture, long battlePoints, long playerPoints, int[] winLossDraws)
        {
            this.UID = UID;
            this.username = username;
            this.stringTagline = "";
            this.region = region;

            this.rank = rank;

            this.tagLine = tagLine;
            this.profilePicture = profilePicture;
            this.battlePoints = battlePoints;
            this.playerPoints = playerPoints;
            this.winLossDraws = winLossDraws;
        }
    }

    public class LeaderboardEntry
    {
        public int leaderboardPlacing;

        public PlayerRank rank;

        public string username;
        public string region;

        public int tagLine;
        public int profilePicture;
        public long score;

        public LeaderboardEntry(PlayerEntity playerEntity)
        {
            this.leaderboardPlacing = 0;

            this.rank = playerEntity.rank;

            this.username = playerEntity.username;
            this.region = playerEntity.region;

            this.tagLine = playerEntity.tagLine;
            this.profilePicture = playerEntity.profilePicture;
            this.score = playerEntity.battlePoints;
        }

        public LeaderboardEntry(int leaderboardPlacing, string username, string region, PlayerRank rank, long score, int tagLine, int profilePicture)
        {
            this.leaderboardPlacing = leaderboardPlacing;

            this.rank = rank;

            this.username = username;
            this.region = region;

            this.score = score;
            this.tagLine = tagLine;
            this.profilePicture = profilePicture;
        }
    }
}

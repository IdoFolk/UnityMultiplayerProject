using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Game
{
    public class ScoreHandler : MonoBehaviourPunCallbacks
    {
        #region Singleton

        public static ScoreHandler Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        #endregion
        
        [SerializeField] private PlayerScoreUIBlock[] playerScoreUIBlocks;
        
        private Dictionary<int,int> playerScores = new Dictionary<int, int>();
        private Stack<int> currentRoundPlacments = new Stack<int>();

        private const string ShowScoresRPC = nameof(ShowScores);
        private const string PlayerDeathRPC = nameof(PlayerDeath);
        private const string GameOverRPC = nameof(GameOver);
        public void SendShowScoresRPC()
        {
            photonView.RPC(GameOverRPC, RpcTarget.All);
        }

        public void SendPlayerDeathRPC(int userID)
        {
            photonView.RPC(PlayerDeathRPC, RpcTarget.All, userID);
        }
        public void SendGameOverRPC()
        {
            photonView.RPC(GameOverRPC, RpcTarget.All);
        }

        [PunRPC]
        public void ShowScores()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 8)
            {
                Debug.LogError("Player Count Above 8 Not Implemented in scoreboard");
                return;
            }
            var i = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                // playerScoreUIBlocks[i].gameObject.SetActive(true);
                // var score = playerScores.GetValueOrDefault(player.Key, 0); 
                // var color = player.Value.
                // playerScoreUIBlocks[i].Init(score);
                i++;
            }

        }
        [PunRPC]
        public void PlayerDeath(int userID)
        {
            currentRoundPlacments.Push(userID);

        }
        [PunRPC]
        public void GameOver()
        {
            for (int i = 0; i < currentRoundPlacments.Count; i++)
            {
                playerScores[currentRoundPlacments.Pop()] += i;
            }
        }
    }
}
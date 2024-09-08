using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private int scoreToWin;
        [SerializeField] private TMP_Text aboveTheStartButtonText;
        [SerializeField] private Button startRoundButton;
        
        private Dictionary<int,int> playerScores = new Dictionary<int, int>();
        private Queue<int> currentRoundPlacments = new Queue<int>();

        private const string RoundBeginsRPC = nameof(RoundBegin);
        private const string ShowScoresRPC = nameof(ShowScores);
        private const string PlayerDeathRPC = nameof(PlayerDeath);
        private const string GameOverRPC = nameof(GameOver);
        
        public static event Action MatchEnded;

        public void SetScoreGoalForTheMatch(int score)
        {
            if(!PhotonNetwork.IsMasterClient) return;
            scoreToWin = score;
        }
        public void SendShowScoresRPC()
        {
            photonView.RPC(ShowScoresRPC, RpcTarget.All);
        }

        public void SendPlayerDeathRPC(int userID)
        {
            photonView.RPC(PlayerDeathRPC, RpcTarget.MasterClient, userID);
        }
        public void SendRoundBeginsRPC()
        {
            photonView.RPC(RoundBeginsRPC, RpcTarget.MasterClient);
        }
        private void SendGameOverRPC()
        {
            photonView.RPC(GameOverRPC, RpcTarget.MasterClient);
        }
        

        #region RPC
        
        [PunRPC]
        public void ShowScores()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 8)
            {
                Debug.LogError("Player Count Above 8 Not Implemented in scoreboard");
                return;
            }
            foreach (var playerScoreUIBlock in playerScoreUIBlocks)
            {
                playerScoreUIBlock.Hide();
            }
            int i = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                var score = player.Value.CustomProperties["Score"];
                var color = (string)player.Value.CustomProperties["Color"];
                if(score != null) playerScoreUIBlocks[i].Show(player.Value.NickName,color.FromHexToColor(),(int)score);
                else playerScoreUIBlocks[i].Show(player.Value.NickName,color.FromHexToColor(),0);
                i++;
            }

        }

        [PunRPC]
        public void RoundBegin()
        {
            currentRoundPlacments.Clear();
            SendShowScoresRPC();
        }
        [PunRPC]
        public void PlayerDeath(int userID)
        {
            if(!currentRoundPlacments.Contains(userID)) currentRoundPlacments.Enqueue(userID);
            if(currentRoundPlacments.Count == PhotonNetwork.CurrentRoom.PlayerCount - 1) SendGameOverRPC();
        }
        [PunRPC]
        public void GameOver()
        {
            bool gameEnded = false;
            int winnerID = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                if(!currentRoundPlacments.Contains(player.Value.ActorNumber)) currentRoundPlacments.Enqueue(player.Value.ActorNumber);
            }

            var playerCount = currentRoundPlacments.Count;
            //Debug.Log("Current Players: " + playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                var userID = currentRoundPlacments.Dequeue();
                if(!playerScores.TryAdd(userID, i)) playerScores[userID] += i;
                if (playerScores[userID] >= scoreToWin)
                {
                    winnerID = userID;
                    gameEnded = true;
                }
                    
            }
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                player.Value.SetCustomProperties(new Hashtable(){{"Score", playerScores[player.Value.ActorNumber]}});
                //Debug.Log($"player {player.Value.ActorNumber} score: {playerScores[player.Value.ActorNumber]}");
            }
            Invoke(nameof(SendShowScoresRPC),0.5f);

            if (gameEnded)
            {
                photonView.RPC(nameof(EndGame), RpcTarget.All, winnerID);
            }
            
        }

        [PunRPC]
        private void EndGame(int userID)
        {
            Debug.Log("Game Over");
            startRoundButton.gameObject.SetActive(false);
            if (PhotonNetwork.LocalPlayer.ActorNumber == userID)
            {
                aboveTheStartButtonText.text = "You Win!";
                Debug.Log("you win");
            }
            else
            {
                aboveTheStartButtonText.text = "You Lose!";
                Debug.Log("you lose");
            }
            
            MatchEnded?.Invoke();
            
        }
        #endregion

      
    }
    
    
}
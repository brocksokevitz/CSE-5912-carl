using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Prototype.NetworkLobby
{
    public class LobbyManager : NetworkLobbyManager 
    {
        static short MsgKicked = MsgType.Highest + 1;

        static public LobbyManager s_Singleton;


        [Header("Unity UI Lobby")]
        [Tooltip("Time in second between all players ready & match start")]
        public float prematchCountdown = 5.0f;

        [Space]
        [Header("UI Reference")]
        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public LobbyInfoPanel infoPanel;
        public LobbyCountdownPanel countdownPanel;
        public GameObject addPlayerButton;

        protected RectTransform currentPanel;

        public Button backButton;
        public Button exitButton;

        public Text statusInfo;
        public Text hostInfo;
        
        public int redPlayers = 0;
        public int bluePlayers = 0;

        List<Vector3> redSpawns;
        int redSpawnCounter = 0;
        List<Vector3> blueSpawns;
        int blueSpawnCounter = 0;
        short playerCounter = 0;
        bool SpawnLoading = false;

        //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
        //of players, so that even client know how many player there is.
        [HideInInspector]
        public int _playerNumber = 0;

        //used to disconnect a client properly when exiting the matchmaker
        [HideInInspector]
        public bool _isMatchmaking = false;

        protected bool _disconnectServer = false;
        
        protected ulong _currentMatchID;

        protected LobbyHook _lobbyHooks;

        // Variables used for custom matchmaking
        public static DateTime matchSuccessConnectTime = DateTime.MinValue;
        public static DateTime matchDisconnectTime = DateTime.MinValue;
        [HideInInspector]
        public string CurrentNetworkID { get; set; }
        public GameObject mainMenuObj;

        void Start()
        {
            s_Singleton = this;
            _lobbyHooks = GetComponent<Prototype.NetworkLobby.LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);

            redSpawns = new List<Vector3>();
            blueSpawns = new List<Vector3>();
            var config = new ConnectionConfig();
            config.NetworkDropThreshold = 45;
            config.OverflowDropThreshold = 45;
            config.MaxSentMessageQueueSize = 200;
            config.AddChannel(QosType.ReliableSequenced);
            config.AddChannel(QosType.Unreliable);
            NetworkServer.Configure(config, 12);
            //SetServerInfo("Offline", "None");
        }
        

        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
            if (SceneManager.GetSceneAt(0).name == lobbyScene)
            {
                if (topPanel.isInGame)
                {
                    ChangeTo(lobbyPanel);
                    if (_isMatchmaking)
                    {
                        if (conn.playerControllers[0].unetView.isServer)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                    else
                    {
                        if (conn.playerControllers[0].unetView.isClient)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                }
                else
                {
                    ChangeTo(mainMenuPanel);
                }

                topPanel.ToggleVisibility(true);
                topPanel.isInGame = false;
            }
            else
            {
                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));
                //backDelegate = StopGameClbk;
                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);
            }
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel != mainMenuPanel)
            {
                backButton.gameObject.SetActive(true);
                exitButton.gameObject.SetActive(false);
            }
            else
            {
                backButton.gameObject.SetActive(false);
                exitButton.gameObject.SetActive(true);
                //SetServerInfo("Offline", "None");
                _isMatchmaking = false;
            }
        }

        public void DisplayIsConnecting()
        {
            var _this = this;
            infoPanel.Display("Connecting...", "Cancel", () => { _this.backDelegate(); });
        }

        public void SetServerInfo(string status, string host)
        {
            statusInfo.text = status;
            hostInfo.text = host;
        }


        public delegate void BackButtonDelegate();
        public BackButtonDelegate backDelegate;
        public void GoBackButton()
        {
            backDelegate();
			topPanel.isInGame = false;
            GameObject tempMusic = GameObject.Find("MusicAudioSource");
            MusicScript ms = tempMusic.GetComponent<MusicScript>();
            ms.SwitchState(MusicScript.SongStates.Menu);
        }

        public void ExitButton()
        {
            if (backDelegate != null)
                backDelegate();
            ChangeTo(null);
            //backDelegate = StopGameClbk;
            topPanel.isInGame = true;
            topPanel.ToggleVisibility(false);

            Destroy(GameObject.Find("MainMenuUI(Clone)"));
            Destroy(GameObject.Find("LobbyManager"));

            // Unlock the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Change the scene
            SceneManager.LoadScene("MenuScene");
        }

        // ----------------- Server management

        public void AddLocalPlayer()
        {
            TryToAddPlayer();
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            player.RemovePlayer();
        }

        public void SimpleBackClbk()
        {
            ChangeTo(mainMenuPanel);
        }
                 
        public void StopHostClbk()
        {
            if (_isMatchmaking)
            {
				matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
				_disconnectServer = true;
            }
            else
            {
                StopHost();
            }

            
            ChangeTo(mainMenuPanel);
        }

        public void StopClientClbk()
        {
            StopClient();

            if (_isMatchmaking)
            {
                StopMatchMaker();
            }

            ChangeTo(mainMenuPanel);
        }

        public void StopServerClbk()
        {
            StopServer();
            ChangeTo(mainMenuPanel);
        }

        class KickMsg : MessageBase { }
        public void KickPlayer(NetworkConnection conn)
        {
            conn.Send(MsgKicked, new KickMsg());
        }




        public void KickedMessageHandler(NetworkMessage netMsg)
        {
            infoPanel.Display("Kicked by Server", "Close", null);
            netMsg.conn.Disconnect();
        }

        //===================

        public override void OnStartHost()
        {
            base.OnStartHost();

            ChangeTo(lobbyPanel);
            backDelegate = StopHostClbk;
            SetServerInfo("Hosting", networkAddress);
        }

		public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			base.OnMatchCreate(success, extendedInfo, matchInfo);
            _currentMatchID = (System.UInt64)matchInfo.networkId;
		}

        public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            base.OnMatchJoined(success, extendedInfo, matchInfo);
            if (!success)
            {
                Debug.Log("Failed to join match. Possibly because it is already started.");
            }
            else
            {
                Debug.Log("Match joined successfully!");
                matchSuccessConnectTime = DateTime.Now;
            }
        }

        public override void OnDestroyMatch(bool success, string extendedInfo)
		{
			base.OnDestroyMatch(success, extendedInfo);
			if (_disconnectServer)
            {
                StopMatchMaker();
                StopHost();
            }
        }

        //allow to handle the (+) button to add/remove player
        public void OnPlayersNumberModified(int count)
        {
            _playerNumber += count;

            int localPlayerCount = 0;
            foreach (UnityEngine.Networking.PlayerController p in ClientScene.localPlayers)
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            addPlayerButton.SetActive(localPlayerCount < maxPlayersPerConnection && _playerNumber < maxPlayers);
        }

        // ----------------- Server callbacks ------------------

        //we want to disable the button JOIN if we don't have enough player
        //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation
        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

            LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
            newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);


            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
                }
            }

            return obj;
        }

        public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
                }
            }
        }

        public override void OnLobbyServerDisconnect(NetworkConnection conn)
        {
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers >= minPlayers);
                }
            }

        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            //This hook allows you to apply state data from the lobby-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            if (_lobbyHooks)
                _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

            return true;
        }

        // --- Countdown management

        public override void OnLobbyServerPlayersReady()
        {
			bool allready = true;
			for(int i = 0; i < lobbySlots.Length; ++i)
			{
				if(lobbySlots[i] != null)
					allready &= lobbySlots[i].readyToBegin;
			}

			if(allready)
            {
                StartCoroutine(ServerCountdownCoroutine());
            }
        }

        public IEnumerator ServerCountdownCoroutine()
        {
            float remainingTime = prematchCountdown;
            int floorTime = Mathf.FloorToInt(remainingTime);

            while (remainingTime > 0)
            {
                yield return null;

                remainingTime -= Time.deltaTime;
                int newFloorTime = Mathf.FloorToInt(remainingTime);

                if (newFloorTime != floorTime)
                {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                    floorTime = newFloorTime;

                    for (int i = 0; i < lobbySlots.Length; ++i)
                    {
                        if (lobbySlots[i] != null)
                        {//there is maxPlayer slots, so some could be == null, need to test it before accessing!
                            (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
                        }
                    }
                }
            }

            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                if (lobbySlots[i] != null)
                {
                    (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
                }
            }

            ServerChangeScene(playScene);


            // Lock the cursor to the window
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        

        // ----------------- Client callbacks ------------------

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            infoPanel.gameObject.SetActive(false);

            conn.RegisterHandler(MsgKicked, KickedMessageHandler);

            if (!NetworkServer.active)
            {//only to do on pure client (not self hosting client)
                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
                SetServerInfo("Client", networkAddress);
            }
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            //CheckIfSpawnsCreated();
            Debug.Log("Player Adding");
            playerCounter++;
            GameObject player = GameObject.Instantiate(gamePlayerPrefab, new Vector3(0f, 100f, 0f), Quaternion.identity);


            player.SetActive(true);
            //ClientScene.AddPlayer(conn, playerControllerId);
            //NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            return player;
        }



        public Vector3 GetSpawnLocation(int team)
        {
            if (team == 0)
            {
                int currentCounter = redSpawnCounter;
                redSpawnCounter++;
                if (redSpawnCounter >= redSpawns.Count)
                {
                    redSpawnCounter = 0;
                }
                return redSpawns[redSpawnCounter];
            }
            else
            {
                int currentCounter = blueSpawnCounter;
                blueSpawnCounter++;
                if (blueSpawnCounter >= blueSpawns.Count)
                {
                    blueSpawnCounter = 0;
                }
                return blueSpawns[blueSpawnCounter];
            }
        } 

        public void CheckIfSpawnsCreated()
        {
            if (!SpawnLoading)
            {
                SpawnLoading = true;
                if (redSpawns.Count < 6)
                {
                    GameObject[] tempRedSpawns = GameObject.FindGameObjectsWithTag("RedSpawn");
                    foreach (GameObject tempSpawn in tempRedSpawns)
                    {
                        redSpawns.Add(tempSpawn.transform.position);

                    }
                }
                if (blueSpawns.Count < 6)
                {
                    GameObject[] tempBlueSpawns = GameObject.FindGameObjectsWithTag("BlueSpawn");
                    foreach (GameObject tempSpawn in tempBlueSpawns)
                    {
                        blueSpawns.Add(tempSpawn.transform.position);

                    }
                }
                SpawnLoading = false;
            }
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            ChangeTo(mainMenuPanel);
            matchDisconnectTime = DateTime.Now;
            if (matchDisconnectTime.Subtract(matchSuccessConnectTime).TotalSeconds < 2)
            {
                Debug.Log("Should continue matchmaking! --> " + CurrentNetworkID.ToString());
                LobbyMainMenu LMM = mainMenuObj.GetComponent<LobbyMainMenu>();
                LMM.matchesNotToJoin.Add(CurrentNetworkID);
                LMM.OnClickStartMatchmaking(false);
            }
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            ChangeTo(mainMenuPanel);
            infoPanel.Display("Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()), "Close", null);
        }
    }
}

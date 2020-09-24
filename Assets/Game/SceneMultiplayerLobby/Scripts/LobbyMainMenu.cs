using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour 
    {
        public LobbyManager lobbyManager;

        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;

        public InputField ipInput;
        public InputField matchNameInput;

        private bool _isOnline;

        // Matchmaking variables
        private int _gamesPerPage = 6;
        private int _previousPage = 0;
        private int _currentPage = 0;
        private bool _hasHitBlankPage = false;
        public List<string> matchesNotToJoin = new List<string>();

        public void OnEnable()
        {
            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);
            
            if (NetworkHelper.IsConnectedToInternet())
            {
                Debug.Log("INTERNET CONNECTED!");
                lobbyManager.SetServerInfo("ONLINE", "None");
                _isOnline = true;
            }
            else
            {
                Debug.Log("INTERNET ISSUES...");
                lobbyManager.SetServerInfo("OFFLINE", "None");
                _isOnline = false;
            }
        }
        
        public void OnClickStartMatchmaking(bool isButtonClick=true)
        {
            if (_isOnline)
            {
                Debug.Log("MATCHMAKING!");
                lobbyManager.StartMatchMaker();
                lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
                _previousPage = 0;
                _currentPage = 0;
                _hasHitBlankPage = false;
                if (isButtonClick)
                    matchesNotToJoin = new List<string>();
                FindGames(0);
            }
            else
            {
                Debug.Log("Currently offline...");
            }
        }

        public void OnGUIMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
        {
            if (matches.Count == 0)
            {
                if (_currentPage == 0)
                {
                    // No games found. Create one.
                    Debug.Log("No games found on page 0. Creating one...");
                    OnClickCreateMatchmakingGame(false);
                }
                else
                {
                    Debug.Log("No games found. Getting previous page...");
                    _hasHitBlankPage = true;
                    FindGames(_currentPage - 1);
                }
                return;
            }
            else
            {
                // Looks like there are some games here. Loop through to see if there is an open spot to join
                for (int i = 0; i < matches.Count; i++)
                {
                    // If theres any room left in this game to join...
                    if (matches[i].currentSize < matches[i].maxSize && !matchesNotToJoin.Contains(matches[i].networkId.ToString()))
                    {
                        Debug.Log("Joining match: " + matches[i].networkId.ToString());
                        JoinMatch(matches[i].networkId);
                        break;
                    }
                }

                if (!lobbyManager._isMatchmaking)
                {
                    if (_gamesPerPage > matches.Count)
                    {
                        Debug.Log("All games are full. Create a new game.");
                        OnClickCreateMatchmakingGame(false);
                    }
                    else if (!_hasHitBlankPage)
                    {
                        Debug.Log("All games are full and no higher pages are available. Create a new game.");
                        OnClickCreateMatchmakingGame(false);
                    }
                    else
                    {
                        Debug.Log("All games are full. Getting next page...");
                        FindGames(_currentPage + 1);
                    }
                }
                return;
            }
        }

        public void FindGames(int page)
        {
            _previousPage = _currentPage;
            _currentPage = page;
            lobbyManager.matchMaker.ListMatches(page, _gamesPerPage, "", true, 0, 0, OnGUIMatchList);
        }

        void JoinMatch(NetworkID networkID)
        {
            lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager._isMatchmaking = true;
            lobbyManager.CurrentNetworkID = networkID.ToString();
            lobbyManager.DisplayIsConnecting();
        }

        #region Other Stuff
        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.networkAddress = ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickDedicated()
        {
            lobbyManager.ChangeTo(null);
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
        }

        public void OnClickCreateMatchmakingGame(bool isButtonClick=true)
        {
            lobbyManager.StartMatchMaker();
            if (isButtonClick)
            {
                lobbyManager.matchMaker.CreateMatch(
                matchNameInput.text,
                (uint)lobbyManager.maxPlayers,
                true,
                "", "", "", 0, 0,
                lobbyManager.OnMatchCreate);
            }
            else
            {
                lobbyManager.matchMaker.CreateMatch(
                System.Guid.NewGuid().ToString(),
                (uint)lobbyManager.maxPlayers,
                true,
                "", "", "", 0, 0,
                lobbyManager.OnMatchCreate);
            }

            lobbyManager.backDelegate = lobbyManager.StopHost;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
        }

        public void OnClickOpenServerList()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.ChangeTo(lobbyServerList);
        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        void onEndEditGameName(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickCreateMatchmakingGame();
            }
        }
        #endregion
    }
}

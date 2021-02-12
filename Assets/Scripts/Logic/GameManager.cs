﻿using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;
using Characters;
using UnityEngine.Events;
using UserInterface;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Logics
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        
        public enum Roles
        {
            Serb, Croat, Murderer,Survivor
        }

        // TODO need classes
        public enum Classes
        {
            Hunter, // reads the tracks, starts with rifle
            Gypsy,  // 6 inventory slots, starts with coins and slipping baby
            Medic,  // reads wounds, starts with first aid kit
            
        }

        [SerializeField] private PlayerCharacter playerPrefab;

        [SerializeField] private Transform homePoint;

        [SerializeField] private UI userInterface;

        [SerializeField] private int requiredPlayers = 2;

        [SerializeField] private UnityEvent<PlayerCharacter> OnLocalPlayerCreated;

        [SerializeField] private UnityEvent OnGameStarted;

        public Dictionary<Player, PlayerCharacter> Characters
        {
            get;
            private set;
        }

        private bool _canStartGame = false;
        private bool CanStartGame
        {
            get => _canStartGame;
            set
            {
                _canStartGame = value;
                
                // TODO add events or smth
                userInterface.StartGameButton.SetActive(value && PhotonNetwork.IsMasterClient);
            }
        }

        public static GameManager Instance { get; private set; }
        
        private bool _isGameStarted = false;


        private Voting _voting;
        
        
        private void Awake()
        {
            Characters = new Dictionary<Player, PlayerCharacter>();
            _voting = GetComponent<Voting>();

            Instance = this;
        }

        // TODO add remove player
        public void AddPlayer(PlayerCharacter character)
        {
            character.DeathEvent.AddListener(CheckGameState);
            Characters.Add(character.photonView.Owner,character);

            if (Characters.Count >= requiredPlayers)
                CanStartGame = true;

            if (character.photonView.IsMine)
            {
                OnLocalPlayerCreated?.Invoke(character);
            }
        }
        
        public void StartGame()
        {
            if(!PhotonNetwork.IsMasterClient || !CanStartGame) return;
            SingRoles();
            SingClasses();
            MovePlayersToSpawnPositions();
        }

        private void SingClasses()
        {
            var classes = Enum.GetNames(typeof(Classes));
            foreach (var player in Characters.Keys)
            {
                var hash = new Hashtable{{"Class",classes[Random.Range(0,classes.Length)]}};
                player.SetCustomProperties(hash);
            }
        }
        
        private void SingRoles()
        {
            var players = Characters.Keys.ToList();

            var serbian = players[Random.Range(0, players.Count)];
            var sHash = new Hashtable{{"Role", Roles.Serb.ToString()}};
            serbian.SetCustomProperties(sHash);
            players.Remove(serbian);
            
            var croat = players[Random.Range(0, players.Count)];
            var cHash = new Hashtable{{"Role", Roles.Croat.ToString()}};
            croat.SetCustomProperties(cHash);
            players.Remove(croat);

            var hash = new Hashtable{{"Role",Roles.Survivor.ToString()}};
            foreach (var player in players)
            {
                player.SetCustomProperties(hash);
            }
        }

        public void MovePlayersToSpawnPositions()
        {
            if(!PhotonNetwork.IsMasterClient)
                return;

            var delta = 360 / Characters.Count;
            var angle = 0;
            foreach (var character in Characters.Values)
            {
                if (character.IsAlive)
                { 
                    var pos = homePoint.position + Quaternion.Euler(0, 0, angle) * Vector3.right;
                    character.photonView.RPC("RelocateTo", character.photonView.Owner, pos);
                }
                
                angle += delta;
            }
        }
        
        public void KickPlayer(Player player)
        {
            if(player is null)
                return;
            
            var character = Characters[player];
            character.Kill();
        }

        public void CheckGameState()
        {
            throw new NotImplementedException();
        }

        private void EndGame()
        {
            throw new NotImplementedException();

        }

        public void FreezePlayers()
        {
            Debug.Log("Players are frozen");
            foreach (var character in Characters.Values)
            {
                character.Freeze();
            }
        }

        public void UnfreezePlayers()
        {
            Debug.Log("Players are unfrozen");
            foreach (var character in Characters.Values)
            {
                character.Unfreeze();
            }
        }
    }
}
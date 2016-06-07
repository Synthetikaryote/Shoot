using UnityEngine;
using System.Collections.Generic;

public class Uber : MonoBehaviour {
    public GameObject OtherPlayerPrefab;

    public ServerCommunication server;
    public bool isConnected = false;
    Player player = null;
    Dictionary<uint, OtherPlayer> otherPlayers;

	// Use this for initialization
	void Start () {
        player = GameObject.FindObjectOfType<Player>();

        otherPlayers = new Dictionary<uint, OtherPlayer>();
        server = GetComponent<ServerCommunication>();
        server.onGameInfoReceived += (id, others) => {
            foreach (KeyValuePair<uint, ServerCommunication.Player> pair in others) {
                var other = pair.Value;
                AddPlayer(pair.Value);
            }
            server.EnterGame(server.player.id.ToString(), player.gameObject.transform.position, player.hitPoints);
            isConnected = true;
        };
        server.onPlayerMoved += other => {
            otherPlayers[other.id].gameObject.transform.position = other.pos;
        };
        server.onPlayerConnected += AddPlayer;
        server.onPlayerDisconnected += other => {
            Destroy(otherPlayers[other.id].gameObject);
        };
        server.onPlayerUpdateHealth += (other, health) => {
            if (other.id == server.player.id)
                player.hitPoints = health;
            else
                otherPlayers[other.id].health = health;
        };
        server.onPlayerUpdateState += (other, state) => {
            if (other.id != server.player.id)
                otherPlayers[other.id].state = state;
        };
    }

    void AddPlayer(ServerCommunication.Player other) {
        var otherGO = Instantiate(OtherPlayerPrefab, other.pos, Quaternion.identity) as GameObject;
        var otherPlayer = otherGO.GetComponent<OtherPlayer>();
        otherPlayer.playerName = other.name;
        otherPlayers[other.id] = otherPlayer;
    }

    // Update is called once per frame
    void Update () {
	
	}
}

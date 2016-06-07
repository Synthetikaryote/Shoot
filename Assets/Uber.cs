using UnityEngine;
using System.Collections.Generic;

public class Uber : MonoBehaviour {
    public GameObject OtherPlayerPrefab;

    ServerCommunication server;
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

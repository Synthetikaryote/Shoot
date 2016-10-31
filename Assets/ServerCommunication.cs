using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

public class ServerCommunication : MonoBehaviour {
    const uint
        // from server
        specInitialize = 1,
        specDisconnect = 2,
        specAnnounceConnect = 3,
        specUpdatePosition = 4,
        specHeartbeat = 5,
        specHeartbeatResponse = 6,
        specUpdateHealth = 7,
        specUpdateState = 8,
        // client only
        specMessage = 502;

    public class Player
    {
        public uint id = uint.MaxValue;
        public string name = null;
        public Vector3 pos = Vector3.zero;
        public float health = 0.0f;
        public byte[] state = null;
    }

    public const int nameLength = 16;
    public Player player = new Player();
    public Dictionary<uint, Player> otherPlayers = new Dictionary<uint, Player>();
    WebSocket w;
    public bool isConnected { get { return w != null && w.isConnected; } }
    public delegate void GameInfoReceived(uint id, Dictionary<uint, Player> otherPlayers);
    public GameInfoReceived onGameInfoReceived;
    public delegate void PlayerConnected(Player player);
    public PlayerConnected onPlayerConnected;
    public delegate void PlayerDisconnected(Player player);
    public PlayerDisconnected onPlayerDisconnected;
    public delegate void PlayerMoved(Player player);
    public PlayerMoved onPlayerMoved;
    public delegate void PlayerMessage(Player player, string message);
    public PlayerMessage onPlayerMessage;
    public delegate void PlayerUpdateHealth(Player player, float health);
    public PlayerUpdateHealth onPlayerUpdateHealth;
    public delegate void PlayerUpdateState(Player player, byte[] state);
    public PlayerUpdateState onPlayerUpdateState;


    // Use this for initialization
    IEnumerator Start () {
		w = new WebSocket(new Uri("ws://ec2-54-213-60-9.us-west-2.compute.amazonaws.com:16248"));
        w.OnError += OnError;
        w.OnLogMessage += OnLogMessage;
        Debug.Log("Trying to connect");
        yield return StartCoroutine(w.Connect());
        Debug.Log(w.isConnected ? "Connected!" : "Couldn't connect");
		while (true) {
            if (w.lastError != null)
                break;
            var reply = w.Recv();
            if (reply != null) {
                //var stringData = "[";
                //for (var i = 0; i < reply.Length; ++i)
                //{
                //    if (i % 4 == 0)
                //        stringData += "\n";
                //    var c = reply[i];
                //    stringData += " " + c.ToString("000");
                //}
                //stringData += " ]";
                //Debug.Log("Received from the server: " + stringData);
                uint spec = BitConverter.ToUInt32(reply, 0);
                switch (spec) {
                    case specInitialize:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            player.id = id;
                            uint otherPlayerCount = BitConverter.ToUInt32(reply, 8);
                            int byteIndex = 12;
                            for (int i = 0; i < otherPlayerCount; ++i)
                            {
                                var other = new Player();
                                other.id = BitConverter.ToUInt32(reply, byteIndex);
                                other.name = Encoding.Unicode.GetString(reply, byteIndex + 4, nameLength);
                                float x = BitConverter.ToSingle(reply, byteIndex + nameLength + 4);
                                float y = BitConverter.ToSingle(reply, byteIndex + nameLength + 8);
                                float z = BitConverter.ToSingle(reply, byteIndex + nameLength + 12);
                                other.pos = new Vector3(x, y, z);
                                other.health = BitConverter.ToSingle(reply, byteIndex + nameLength + 16);
                                otherPlayers[other.id] = other;
                                byteIndex += 4 + nameLength + 16;
                            }
                            Debug.Log("Received ID: " + id + " and game start info.  There are " + otherPlayerCount + " other players.");
                            if (onGameInfoReceived != null)
                                onGameInfoReceived(id, otherPlayers);
                            break;
                        }
                    case specDisconnect:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            Player other = null;
                            if (otherPlayers.TryGetValue(id, out other))
                            {
                                Debug.Log(other.name + " disconnected.");
                                if (onPlayerDisconnected != null)
                                    onPlayerDisconnected(other);
                            } else
                                Debug.Log("player" + id + " disconnected.");
                            otherPlayers.Remove(id);
                            break;
                        }
                    case specAnnounceConnect:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            if (id == player.id)
                                break;
                            Player other = new Player();
                            other.id = id;
                            other.name = Encoding.Unicode.GetString(reply, 8, nameLength);
                            float x = BitConverter.ToSingle(reply, 8 + nameLength);
                            float y = BitConverter.ToSingle(reply, 12 + nameLength);
                            float z = BitConverter.ToSingle(reply, 16 + nameLength);
                            other.pos = new Vector3(x, y, z);
                            otherPlayers[id] = other;
                            Debug.Log(other.name + " has connected (id " + other.id + ", pos " + other.pos + ")");
                            if (onPlayerConnected != null)
                                onPlayerConnected(other);
                            break;
                        }
                    case specUpdatePosition:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            float x = BitConverter.ToSingle(reply, 8);
                            float y = BitConverter.ToSingle(reply, 12);
                            float z = BitConverter.ToSingle(reply, 16);
                            var pos = new Vector3(x, y, z);
                            Player other = null;
                            if (player.id == id)
                                break;
                            if (otherPlayers.TryGetValue(id, out other)) {
                                //Debug.Log(other.name + " is now at position " + pos);
                                other.pos = pos;
                                if (onPlayerMoved != null)
                                    onPlayerMoved(other);
                            }
                            break;
                        }
                    case specMessage:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            string message = Encoding.Unicode.GetString(reply, 8, reply.Length - 8);
                            string name = null;
                            if (id == player.id)
                                name = player.name;
                            if (name == null) {
                                Player other = null;
                                if (otherPlayers.TryGetValue(id, out other)) {
                                    name = other.name;
                                    if (onPlayerMessage != null)
                                        onPlayerMessage(other, message);
                                }
                            }
                            if (name != null)
                                Debug.Log(name + ": " + message);
                            else
                                Debug.Log("player" + id + ": " + message);
                            break;
                        }
                    case specHeartbeat:
                        {
                            SendHeartbeatResponse();
                            break;
                        }
                    case specUpdateHealth:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            float health = BitConverter.ToSingle(reply, 8);
                            Player updatedPlayer = null;
                            if (id == player.id)
                            {
                                updatedPlayer = player;
                            } else
                            {
                                Player other = null;
                                if (otherPlayers.TryGetValue(id, out other))
                                {
                                    updatedPlayer = other;
                                }
                            }
                            if (updatedPlayer != null)
                            {
                                updatedPlayer.health = health;
                                if (onPlayerUpdateHealth != null)
                                    onPlayerUpdateHealth(updatedPlayer, health);
                            }
                            break;
                        }
                    case specUpdateState:
                        {
                            uint id = BitConverter.ToUInt32(reply, 4);
                            int stateSize = reply.Length - 8;
                            byte[] state = new byte[stateSize];
                            Buffer.BlockCopy(reply, 8, state, 0, stateSize);
                            Player updatedPlayer = null;
                            if (id == player.id) {
                                updatedPlayer = player;
                            } else {
                                Player other = null;
                                if (otherPlayers.TryGetValue(id, out other)) {
                                    updatedPlayer = other;
                                }
                            }
                            if (updatedPlayer != null) {
                                updatedPlayer.state = state;
                                if (onPlayerUpdateState != null)
                                    onPlayerUpdateState(updatedPlayer, state);
                            }
                            break;
                        }
                }
            } else {
                yield return 0;
            }
		}
		w.Close();
	}

    void OnLogMessage(WebSocketSharp.LogData logData, string message) {
        Debug.LogWarning(logData.Message);
    }

    void OnError(string message) {
        Debug.LogWarning("WebSocket error: " + message);
    }

    bool Validate(string prefix)
    {
        if (!isConnected)
        {
            Debug.LogWarning(prefix + ": Need to connect to the server first");
            return false;
        }
        if (player.id == uint.MaxValue)
        {
            Debug.LogWarning(prefix + ": Need an id from the server first");
            return false;
        }
        return true;
    }

    public void EnterGame(string name, Vector3 position, float health) {
        player.name = name;
        player.pos = position;
        player.health = health;
        if (!Validate("EnterGame")) return;
        var stream = new MemoryStream(16 + nameLength);
        stream.Write(BitConverter.GetBytes(specAnnounceConnect), 0, 4);
        stream.Write(BitConverter.GetBytes(player.id), 0, 4);
        var paddedName = new byte[nameLength];
        var nameBytes = Encoding.Unicode.GetBytes(player.name);
        Buffer.BlockCopy(nameBytes, 0, paddedName, 0, nameBytes.Length);
        stream.Write(paddedName, 0, nameLength);
        stream.Write(BitConverter.GetBytes(player.pos.x), 0, 4);
        stream.Write(BitConverter.GetBytes(player.pos.y), 0, 4);
        stream.Write(BitConverter.GetBytes(player.pos.z), 0, 4);
        stream.Write(BitConverter.GetBytes(player.health), 0, 4);
        w.Send(stream.ToArray());
    }

    public void UpdatePosition(Vector3 pos) {
        if (!Validate("UpdatePosition")) return;
        var stream = new MemoryStream(16);
        stream.Write(BitConverter.GetBytes(specUpdatePosition), 0, 4);
        stream.Write(BitConverter.GetBytes(player.id), 0, 4);
        stream.Write(BitConverter.GetBytes(pos.x), 0, 4);
        stream.Write(BitConverter.GetBytes(pos.y), 0, 4);
        stream.Write(BitConverter.GetBytes(pos.z), 0, 4);
        w.Send(stream.ToArray());
    }

    public void UpdateHealth(uint playerID, float health) {
        if (!Validate("UpdateHealth")) return;
        var stream = new MemoryStream(16);
        stream.Write(BitConverter.GetBytes(specUpdateHealth), 0, 4);
        stream.Write(BitConverter.GetBytes(playerID), 0, 4);
        stream.Write(BitConverter.GetBytes(health), 0, 4);
        Player updatedPlayer = null;
        if (playerID == player.id)
            updatedPlayer = player;
        else {
            Player other = null;
            if (otherPlayers.TryGetValue(playerID, out other))
                updatedPlayer = other;
        }
        if (updatedPlayer != null)
            updatedPlayer.health = health;
        w.Send(stream.ToArray());
    }

    public void SendChat(string message) {
        if (!Validate("SendChat")) return;
        var stream = new MemoryStream();
        stream.Write(BitConverter.GetBytes(specMessage), 0, 4);
        stream.Write(BitConverter.GetBytes(player.id), 0, 4);
        var messageBytes = Encoding.Unicode.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
        w.Send(stream.ToArray());
    }

    public void SendHeartbeatResponse() {
        if (!Validate("SendHeartbeatResponse")) return;
        var stream = new MemoryStream();
        stream.Write(BitConverter.GetBytes(specHeartbeatResponse), 0, 4);
        w.Send(stream.ToArray());
    }

    public void UpdateState(byte[] state) {
        if (!Validate("UpdateState")) return;
        var stream = new MemoryStream();
        stream.Write(BitConverter.GetBytes(specUpdateState), 0, 4);
        stream.Write(BitConverter.GetBytes(player.id), 0, 4);
        stream.Write(state, 0, state.Length);
        w.Send(stream.ToArray());
    }
}

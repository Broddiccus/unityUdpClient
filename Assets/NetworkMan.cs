using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public GameObject spawnPlayer;
    public UdpClient udp;
    private List<Player> cPlayers = new List<Player>();
    private PlayerScript myPlayer;
    private List<Player> unmadePlayers = new List<Player>();
    private List<PlayerScript> sPlayers = new List<PlayerScript>();
    private List<string> discPlayers = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        udp = new UdpClient();
        
        udp.Connect("18.223.162.138", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy(){
        udp.Dispose();
    }


    public enum commands{
        NEW_CLIENT,
        UPDATE,
        DISCONNECT,
        LIST
    };
    [Serializable]
    public struct receivedColor
    {
        public float R;
        public float G;
        public float B;
    }
    [Serializable]
    public struct receivedPos
    {
        public float X;
        public float Y;
        public float Z;
    }

    [Serializable]
    public class Message{
        public commands cmd;
        public Player player;
        public Player[] players;
        public string discID;
    }
    
    [Serializable]
    public class Player{
        public string id;
        
        public receivedColor color;
        public receivedPos pos;
    }

    [Serializable]
    public class NewPlayer{
        
    }

    [Serializable]
    public class GameState{
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){
                case commands.NEW_CLIENT:
                    unmadePlayers.Add(latestMessage.player);
                    break;
                case commands.UPDATE:
                    for (int i = 0; i < latestMessage.players.Length; i++)
                    {
                        for (int j = 0; j < cPlayers.Count; j++)
                        {
                            if (cPlayers[j].id == latestMessage.players[i].id)
                            {
                                cPlayers[j] = latestMessage.players[i];
                            }
                        }
                    }
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.DISCONNECT:
                    discPlayers.Add(latestMessage.discID);
                    break;
                case commands.LIST:
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(){
        foreach (Player p in unmadePlayers)
        {
            GameObject temp = Instantiate(spawnPlayer);
            PlayerScript s = temp.GetComponent<PlayerScript>();
            
            if (cPlayers.Count == 0)
            {
                myPlayer = s;
                s.PlayerStart(p.id, p.color.R, p.color.G, p.color.B, true);
            }
            else
            {
                s.PlayerStart(p.id, p.color.R, p.color.G, p.color.B, false);
            }
            cPlayers.Add(p);
            sPlayers.Add(s);
        }
        unmadePlayers.Clear();
        
    }

    void UpdatePlayers(){
        for (int i = 0; i < cPlayers.Count; i++)
        {
            sPlayers[i].PlayerUpdate(cPlayers[i].id, cPlayers[i].color.R, cPlayers[i].color.G, cPlayers[i].color.B, cPlayers[i].pos.X, cPlayers[i].pos.Y, cPlayers[i].pos.Z);
        }
    }

    void DestroyPlayers(){
        for (int i = 0; i < discPlayers.Count; i++)
        {
            for (int j = 0; j < cPlayers.Count; j++)
            {
                if (discPlayers[i] == cPlayers[j].id)
                {
                    Destroy(sPlayers[j].gameObject);
                    cPlayers.Remove(cPlayers[j]);
                    sPlayers.Remove(sPlayers[j]);
                }
            }
        }
        discPlayers.Clear();
    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
        Byte[] sendBytes2 = Encoding.ASCII.GetBytes(myPlayer.positVec.x.ToString() + "," + myPlayer.positVec.y.ToString() + "," + myPlayer.positVec.z.ToString());
        Debug.Log(sendBytes2);
        udp.Send(sendBytes2, sendBytes2.Length);
    }

    void Update(){
        if (unmadePlayers.Count > 0)
            SpawnPlayers();
        
        
        UpdatePlayers();
        if (discPlayers.Count > 0)
            DestroyPlayers();
    }
}
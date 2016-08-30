using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;


/// <summary>
/// Setup for connecting a unity client to a twisted server.
/// Client will connect to the server and send a message with its identification. 
/// </summary>
public class UnityClient : MonoBehaviour { 
    public StateMachine stateMachine;
    public WebSocket ws;
    public bool identified;  // bool flag to specify if unity client is identified by server

    private void Connect() {
        //When connect method is called from the client, assign the statemachine running state : Wait and connect to websocket
    	stateMachine.AddHandler(State.Running, () => {
            //Wait and then connect to websocket
            new Wait(this, 0.1f, () => {
				ws.ConnectAsync();
            });
        });
    }
    
    private void Identify(){
        //When in Connected state, transition to Ping, 
    	stateMachine.AddHandler(State.Connected, () => {
            stateMachine.Transition(State.Ping);
			new Wait(this, 0.05f, () => {
				string uniqueID = SystemInfo.deviceUniqueIdentifier;  //generate a uniqueID for this Unity client
                ws.SendAsync("[{\"proto\":{\"identity\":\""+uniqueID+"\",\"type\":\"unity\"}}]", OnIdentComplete);  //send the specific JSON message the twisted client protocol will identify
			});
        });
        //Add no handler method for Pong State 
        stateMachine.AddHandler(State.Pong, () => {});
    }

    /// <summary>
    /// Public method that can be accessed by any Unity GameObject
    /// allowing a message the be sent to the conencted server, usually 
    /// as a JSON formatted string
    /// </summary>
    /// <param name="msg">JSON Formatted String sent to the connected server</param>
    public void sendMessage(string msg) {
        if (ws.IsConnected & identified) {
            ws.SendAsync(msg, (bool success) => { });
        }
        else {
            Debug.Log("server not connected or identified");
        }
    }

    #region Websocket Event Handlers

    //Handler for when websocket is connected
    private void OnOpenHandler(object sender, System.EventArgs e) {
        Debug.Log("WebSocket connected!");
        //if websocket is connected, transition the stateMachine's state to Connected
        stateMachine.Transition(State.Connected);
    }

    //Handler for when message is received from websocket	
    private void OnMessageHandler(object sender, MessageEventArgs e) {
        Debug.Log (e.Data);
    }
	
    //Handler for when the websocket is closed	
    private void OnCloseHandler(object sender, CloseEventArgs e) {
        Debug.Log("WebSocket closed with reason: " + e.Reason);
        //if websocket is closed, change the stateMachine's state to Done
        stateMachine.Transition(State.Done);
    }

    //Handler for when the websocket message sent
    private void OnSendComplete(bool success) {
        //Debug.Log("Message sent successfully? " + success);
    }
    
    //Handler for when websocket connection is Identified
    private void OnIdentComplete(bool success) {
    	identified = success;
    }
    #endregion

    void Start() {
        //Instantiate a websocket
        ws = new WebSocket("ws://www.mywebsite.com:9000");

        // Subscribe our websocket event handlers to the websocket events
        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;
        
        //Attempt to connect and identify the Unity client with the connected server
        Connect();
        Identify();
        stateMachine.Run();
    }
    
}

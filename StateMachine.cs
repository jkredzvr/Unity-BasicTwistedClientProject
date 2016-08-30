using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State Machine Script that handles server states asynchronously for Unity
/// </summary>

//States
public enum State { NotRunning, Running, Connected, Ping, Pong, Done }

// Delegates to pass methods for each State i.e. call this method for this State
public delegate void Handler();

public class StateMachine : MonoBehaviour {

    #region StateMachine Properties
    private readonly object _syncLock = new object();
    
    //Set currentState as not running
    [SerializeField]
    private State _currentState = State.NotRunning;
        
    //Queue of states for pending transitions
    private readonly Queue<State> _pendingTransitions = new Queue<State>();
    
    //Dictionary of States to assign handlers for each state
    private readonly Dictionary<State, Handler> _handlers = new Dictionary<State, Handler>();
    #endregion

    #region StateMachine Methods 

    //public accessible method to transition to Run State
    public void Run() {
        Transition(State.Running);
    }

    /// <summary>
    /// Helper function that appends to the handlers dictionary  methods to states
    /// </summary>
    /// <param name="state">State machine enum state</param>
    /// <param name="handler">generic method that matches Handler's signature</param>
    public void AddHandler(State state, Handler handler) {
        _handlers.Add(state, handler);
    }

    public void Transition(State state) {
        State cur;
        //Lock statement to ensure transition method's state gets enqueued.
        lock(_syncLock) {
            cur = _currentState;
            _pendingTransitions.Enqueue(state);
        }
        Debug.Log("Queued transition from " + cur + " to " + state);
    }
    #endregion

    public void Update() {

        //if there are queued transition states, dequeue and then call all methods assigned in the _handlers dictionary
        while (_pendingTransitions.Count > 0) { 
            currentState = _pendingTransitions.Dequeue();
            Debug.Log("Transitioned to state " + _currentState);

            Handler handler;
            if (_handlers.TryGetValue(_currentState, out handler)) {
                handler();
            }
        }
    }
}

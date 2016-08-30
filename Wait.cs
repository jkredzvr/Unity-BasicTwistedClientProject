using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Simple Wait class that delays a monobehaviour for seconds 
/// In this case delay sending a message so that the server has time to connect 
/// before we start asking it to do things. 
/// </summary>
/// 
public class Wait {
    public Wait(MonoBehaviour mb, float seconds, Action a){
        mb.StartCoroutine(RunAndWait(seconds, a));
    }
    
    IEnumerator RunAndWait(float seconds, Action a){
        yield return new WaitForSeconds(seconds);
        a();
    }
}

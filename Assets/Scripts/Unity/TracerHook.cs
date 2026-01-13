using UnityEngine;
using PocketSquire.Arena.Core;

public class TracerHook : MonoBehaviour
{
    void Start()
    {
        // Calling your Pure C# logic from Phase 1
        var logic = new ArenaLogic(); 
        string message = logic.GetHelloMessage();
        
        // This is the "Telemetry" the browser will look for
        Debug.Log($"TELEMETRY_MESSAGE: {message}");
    }
}
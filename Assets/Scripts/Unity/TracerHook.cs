using UnityEngine;
using PocketSquire.Arena.Core;

public class TracerHook : MonoBehaviour
{
    void Start()
    {
        // Calling your Pure C# logic from Phase 1
        var logic = new ArenaLogic();
        string arenaName = logic.GetArenaName();

        // This is the "Telemetry" the browser will look for
        Application.ExternalEval($"console.log('TELEMETRY_MESSAGE: {arenaName}')");
    }
}
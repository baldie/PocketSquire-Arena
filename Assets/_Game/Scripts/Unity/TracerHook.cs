using UnityEngine;
using PocketSquire.Arena.Core;
using System.Runtime.InteropServices;

public class TracerHook : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void LogTelemetry(string str);
#else
    private static void LogTelemetry(string str)
    {
        Debug.Log(str);
    }
#endif

    void Start()
    {
        // Calling your Pure C# logic from Phase 1
        var logic = new ArenaLogic();
        string arenaName = logic.GetArenaName();

        // This is the "Telemetry" the browser will look for
        LogTelemetry($"TELEMETRY_MESSAGE: {arenaName}");
    }
}
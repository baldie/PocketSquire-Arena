using System;
using System.IO;
using System.Text;
using UnityEngine;

public class ConsoleLogRedirector : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    // Direct WriteLine calls to Unity's Debug.Log
    public override void WriteLine(string value)
    {
        Debug.Log(value);
    }

    // Optional: Use RuntimeInitializeOnLoadMethod to automate setup
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        Console.SetOut(new ConsoleLogRedirector());
    }
}

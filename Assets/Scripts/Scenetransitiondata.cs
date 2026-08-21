using UnityEngine;

// Statische Klasse, die eine Ziel-Spawn-ID über SceneManager.LoadScene hinweg
// im Speicher hält (überlebt Szenenwechsel, wird nur bei Domain-Reload
// zurückgesetzt, also normalerweise nicht während des Spielens).
public static class SceneTransitionData
{
    public static string targetSpawnPointId;
}
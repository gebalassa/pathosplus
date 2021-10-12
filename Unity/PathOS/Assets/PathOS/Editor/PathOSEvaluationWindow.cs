using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathOS;
using System.IO;

/*
PathOSEvaluationWindow.cs 
(Atiya Nova) 2021
 */

public class PathOSEvaluationWindow : EditorWindow
{
    private void OnEnable()
    {

    }

    public void OnWindowOpen()
    {

    }

    public bool LoadHeuristics(string filename)
    {
        if (!File.Exists(filename) || filename.Substring(filename.Length - 3) != "csv")
        {
            if (filename != "")
                NPDebug.LogError("ERROR: Unable to load heuristics");
            return false;
        }

        return true;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
PathOSWindow.cs 
(Atiya Nova) 2021
 */

public class PathOSWindow : EditorWindow
{

    string[] tabLabels = { "Agent", "Manager", "Visualization", "Batching", "Profiles"};
    int tabSelection = 0;

    private PathOSProfileWindow profileWindow;
    private PathOSAgentBatchingWindow batchingWindow;
    private PathOSAgentWindow agentWindow;
    private PathOSManagerWindow managerWindow;

    private GameObject proxyScreenshot;
    private ScreenshotManager screenshot;
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/PathOS")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PathOSWindow), false, "PathOS");
    }


    private void OnEnable()
    {
        //initializes the different windows
        profileWindow = new PathOSProfileWindow();
        batchingWindow = (PathOSAgentBatchingWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentBatchingWindow));
        agentWindow = new PathOSAgentWindow();
        managerWindow = new PathOSManagerWindow();
    }

    //gizmo stuff from here https://stackoverflow.com/questions/37267021/unity-editor-script-visible-hidden-gizmos
    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);

        EditorGUILayout.LabelField("Screenshot Options", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        //grab screenshot if it already exists
        screenshot = EditorGUILayout.ObjectField("Screenshot Reference: ", screenshot, typeof(ScreenshotManager), true)
        as ScreenshotManager;

        //or instantiate the screenshot camera if it does not exist
        if (screenshot == null)
        {
            if (GUILayout.Button("Instantiate Screenshot Camera"))
            {
                proxyScreenshot = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PathOS/Prefabs/ScreenshotCamera.prefab") as GameObject;
                Instantiate(proxyScreenshot);
                screenshot = proxyScreenshot.GetComponent<ScreenshotManager>();
            }
        }

        //only draws the screenshot elements if the variable is not null
        if (screenshot != null)
        {
            screenshot.SetFolderName(EditorGUILayout.TextField("Folder Name: ", screenshot.GetFolderName()));
            screenshot.SetFilename(EditorGUILayout.TextField("Filename: ", screenshot.GetFilename()));

            if (GUILayout.Button("Take Screenshot"))
            {
                screenshot.TakeScreenshot();
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("PathOS Features", EditorStyles.boldLabel);
        // The tabs to alternate between specific menus
        GUILayout.BeginHorizontal();
        tabSelection = GUILayout.Toolbar(tabSelection, tabLabels);
        GUILayout.EndHorizontal();

        //Switches between the tabs (temp solution, todo: clean this up when you get the time)
        switch (tabSelection)
        {
            case 0:
                agentWindow.OnWindowOpen();
                break;
            case 1:
                managerWindow.OnWindowOpen();
                break;
            case 2:
                managerWindow.OnVisualizationOpen();
                break;
            case 3:
                batchingWindow.OnWindowOpen();
                break;
            case 4:
                profileWindow.OnWindowOpen();
                break;
        }
        GUILayout.EndScrollView();

    }

    private void Update()
    {
        //Temporary solution
        batchingWindow.UpdateBatching();
    }
}

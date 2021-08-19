using System.Collections;
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
    private bool disableCamera = true;
    private Color bgColor, btnColor, btnColorLight, btnColorDark;

    [SerializeField]
    private bool hasScreenshot;

    [SerializeField]
    private int screenshotID;

    [MenuItem("Window/PathOS")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PathOSWindow), false, "PathOS");
    }


    private void OnEnable()
    {
        //Background color
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);
        btnColorLight = new Color32(229, 231, 241, 255);
        btnColorDark = new Color32(158, 164, 211, 255);

        //initializes the different windows
        profileWindow = new PathOSProfileWindow();
        batchingWindow = (PathOSAgentBatchingWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentBatchingWindow));
        agentWindow = new PathOSAgentWindow();
        managerWindow = new PathOSManagerWindow();

        //Re-establish agent reference, if it has been nullified.
        if (hasScreenshot)
        {

            if (screenshot != null)
                screenshotID = screenshot.GetInstanceID();
            else
                screenshot = EditorUtility.InstanceIDToObject(screenshotID) as ScreenshotManager;
        }

        hasScreenshot = screenshot != null;
    }

    
    
    //gizmo stuff from here https://stackoverflow.com/questions/37267021/unity-editor-script-visible-hidden-gizmos
    void OnGUI()
    {
        EditorGUILayout.Space();
        scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);
     
        // The tabs to alternate between specific menus
        GUI.backgroundColor = btnColorDark;
        GUILayout.BeginHorizontal();
        tabSelection = GUILayout.Toolbar(tabSelection, tabLabels);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = bgColor;

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

                EditorGUILayout.LabelField("Screenshot Options", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                GrabScreenshotReference();
                screenshot = EditorGUILayout.ObjectField("Screenshot Reference: ", screenshot, typeof(ScreenshotManager), true)
                    as ScreenshotManager;

                //Update agent ID if the user has selected a new object reference.
                if (EditorGUI.EndChangeCheck())
                {
                    hasScreenshot = screenshot != null;

                    if (hasScreenshot)
                    {
                        screenshotID = screenshot.GetInstanceID();
                    }
                }

                //or instantiate the screenshot camera if it does not exist
                if (screenshot == null)
                {
                    GUI.backgroundColor = btnColorLight;
                    if (GUILayout.Button("Instantiate Screenshot Camera"))
                    {
                        proxyScreenshot = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PathOS/Prefabs/ScreenshotCamera.prefab") as GameObject;
                        Instantiate(proxyScreenshot);
                        screenshot = proxyScreenshot.GetComponent<ScreenshotManager>();
                    }
                    GUI.backgroundColor = bgColor;
                }

                //only draws the screenshot elements if the variable is not null
                if (screenshot != null)
                {
                    screenshot.SetFolderName(EditorGUILayout.TextField("Folder Name: ", screenshot.GetFolderName()));
                    screenshot.SetFilename(EditorGUILayout.TextField("Filename: ", screenshot.GetFilename()));
                    disableCamera = EditorGUILayout.Toggle("Disable During Runtime", disableCamera);

                    if (GUILayout.Button("Take Screenshot"))
                    {
                        screenshot.TakeScreenshot();
                    }

                    if (EditorApplication.isPlaying && disableCamera)
                    {
                        screenshot.gameObject.SetActive(false);
                    }
                    else if (EditorApplication.isPlaying && !disableCamera)
                    {
                        screenshot.gameObject.SetActive(true);
                    }
                }

                EditorGUILayout.Space();
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


    private void GrabScreenshotReference()
    {
        if (hasScreenshot && null == screenshot)
            screenshot = EditorUtility.InstanceIDToObject(screenshotID) as ScreenshotManager;
    }
}

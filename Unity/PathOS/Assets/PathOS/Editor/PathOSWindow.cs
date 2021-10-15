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
   enum Tabs { 
        Agent = 0, 
        Resources = 1, 
        Batching = 2, 
        Manager = 3,
        Visualization = 4,
        Profiles = 5,
        ExpertEvaluation = 6
    };

    string[] tabLabels = { "Agent", "Resources", "Batching", "Manager", "Visualization",  "Profiles", "Expert Evaluation"};
    int tabSelection = 0;

    private PathOSProfileWindow profileWindow;
    private PathOSAgentBatchingWindow batchingWindow;
    private PathOSAgentWindow agentWindow;
    private static PathOSEvaluationWindow evaluationWindow;
    private static PathOSManagerWindow managerWindow;

    private GameObject proxyScreenshot;
    private ScreenshotManager screenshot;
    private Vector2 scrollPos = Vector2.zero;
    private bool disableCamera = true;
    private Color bgColor, btnColor, btnColorLight, btnColorDark;

    [SerializeField]
    private bool hasScreenshot;

    [SerializeField]
    private int screenshotID;

    //Screenshot display settings
    private string lblScreenshotFoldout = "Screenshot Options";
    private static bool screenshotFoldout = false;

    [MenuItem("Window/PathOS+")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PathOSWindow), false, "PathOS+");
    }
    private void OnEnable()
    {
        //Background color
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);
        btnColorLight = new Color32(229, 231, 241, 255);
        btnColorDark = new Color32(158, 164, 211, 255);

        //initializes the different windows
        profileWindow = (PathOSProfileWindow)ScriptableObject.CreateInstance(typeof(PathOSProfileWindow)); //new PathOSProfileWindow();
        batchingWindow = (PathOSAgentBatchingWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentBatchingWindow));
        agentWindow = (PathOSAgentWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentWindow)); //new PathOSAgentWindow();
        managerWindow = (PathOSManagerWindow)ScriptableObject.CreateInstance(typeof(PathOSManagerWindow)); //new PathOSManagerWindow();
        evaluationWindow = (PathOSEvaluationWindow)ScriptableObject.CreateInstance(typeof(PathOSEvaluationWindow)); //new PathOSEvaluationWindow();

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
        tabSelection = GUILayout.SelectionGrid(tabSelection, tabLabels, 3);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = bgColor;

        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        switch (tabSelection)
        {
            case (int)Tabs.Agent:
                agentWindow.OnWindowOpen();
                break;
            case (int)Tabs.Resources:
                OnResourcesOpen();
                break;
            case (int)Tabs.Batching:
                batchingWindow.OnWindowOpen();
                break;
            case (int)Tabs.Manager:
                managerWindow.OnWindowOpen();
                break;
            case (int)Tabs.Visualization:
                managerWindow.OnVisualizationOpen();
                UpdateScreenshots();
                break;
            case (int)Tabs.Profiles:
                profileWindow.OnWindowOpen();
                break;
            case (int)Tabs.ExpertEvaluation:
                    evaluationWindow.OnWindowOpen();
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

    private void UpdateScreenshots()
    {
        screenshotFoldout = EditorGUILayout.Foldout(screenshotFoldout, lblScreenshotFoldout);

        if (screenshotFoldout)
        {
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
        }
    }

    private void OnResourcesOpen()
    {
        agentWindow.OnResourceOpen();

        EditorGUILayout.Space(20);
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(20);

        managerWindow.OnResourceOpen();

    }
}

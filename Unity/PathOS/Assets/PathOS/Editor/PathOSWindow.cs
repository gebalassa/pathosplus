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
        ExpertEvaluation = 5
    };

    string[] tabLabels = { "Agent", "Resources", "Batching", "Manager", "Visualization", "Expert Evaluation"};
    int tabSelection = 0;

    private PathOSProfileWindow profileWindow;
    private PathOSAgentBatchingWindow batchingWindow;
    private PathOSAgentWindow agentWindow;
    private static PathOSEvaluationWindow evaluationWindow;
    private static PathOSManagerWindow managerWindow;

    private GameObject proxyScreenshot;
    private ScreenshotManager screenshot;
    private PathOSManager managerReference;
    private PathOSAgent agentReference;

    private Vector2 scrollPos = Vector2.zero;
    private bool disableCamera = true;
    private Color bgColor, btnColor, btnColorLight, btnColorDark, redColor, navigationColor;

    private GUIStyle foldoutStyle = GUIStyle.none;

    [SerializeField]
    private bool hasScreenshot, hasManager, hasAgent;

    [SerializeField]
    private int screenshotID, managerID, agentID;

    //Screenshot display settings
    private string lblScreenshotFoldout = "Screenshot Options", lblReferenceFoldout = "REFERENCES", lblNavigationFoldout = "NAVIGATION",
         lblBatchingFoldout = "Batching", lblProfilesFoldout = "Profiles";
    private static bool screenshotFoldout = false, showReferences = true, showNavigation = true, showBatching = true, showProfiles = true;

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
        redColor = new Color32(255, 60, 71, 240);
        navigationColor = new Color32(93, 112, 154, 255);

        //initializes the different windows
        profileWindow = (PathOSProfileWindow)ScriptableObject.CreateInstance(typeof(PathOSProfileWindow)); //new PathOSProfileWindow();
        batchingWindow = (PathOSAgentBatchingWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentBatchingWindow));
        agentWindow = (PathOSAgentWindow)ScriptableObject.CreateInstance(typeof(PathOSAgentWindow)); //new PathOSAgentWindow();
        managerWindow = (PathOSManagerWindow)ScriptableObject.CreateInstance(typeof(PathOSManagerWindow)); //new PathOSManagerWindow();
        evaluationWindow = (PathOSEvaluationWindow)ScriptableObject.CreateInstance(typeof(PathOSEvaluationWindow)); //new PathOSEvaluationWindow();

        //Re-establish references, if they have been nullified.
        if (hasScreenshot)
        {
            if (screenshot != null)
                screenshotID = screenshot.GetInstanceID();
            else
                screenshot = EditorUtility.InstanceIDToObject(screenshotID) as ScreenshotManager;
        }

        hasScreenshot = screenshot != null;

        //manager reference
        if (hasManager)
        {

            if (managerReference != null)
            {
                managerID = managerReference.GetInstanceID();
            }
            else
                managerReference = EditorUtility.InstanceIDToObject(managerID) as PathOSManager;
        }

        hasManager = managerReference != null;


        //Agent reference
        if (hasAgent)
        {
            if (agentReference != null)
                agentID = agentReference.GetInstanceID();
            else
                agentReference = EditorUtility.InstanceIDToObject(agentID) as PathOSAgent;
        }

        hasAgent = agentReference != null;

    }


    //gizmo stuff from here https://stackoverflow.com/questions/37267021/unity-editor-script-visible-hidden-gizmos
    void OnGUI()
    {
        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.Space();
        scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);

        GUI.backgroundColor = redColor;

        EditorGUILayout.BeginVertical("Box");
        GUI.backgroundColor = bgColor;

        showReferences = EditorGUILayout.Foldout(showReferences, lblReferenceFoldout, foldoutStyle);

        EditorGUI.BeginChangeCheck();

        if (showReferences) { 

            GrabManagerReference();
            managerReference = EditorGUILayout.ObjectField("Manager Reference: ", managerReference, typeof(PathOSManager), true)
            as PathOSManager;

            GrabAgentReference();
            agentReference = EditorGUILayout.ObjectField("Agent Reference: ", agentReference, typeof(PathOSAgent), true)
            as PathOSAgent;

        }

        //Update agent ID if the user has selected a new object reference.
        if (EditorGUI.EndChangeCheck())
        {
            hasManager = managerReference != null;

            if (hasManager)
            {
                managerID = managerReference.GetInstanceID();
            }

            hasAgent = agentReference != null;

            if (hasAgent)
            {
                agentID = agentReference.GetInstanceID();
            }
        }

        EditorGUILayout.EndVertical();

        GUI.backgroundColor = btnColorDark;
        EditorGUILayout.BeginVertical("Box");

        showNavigation = EditorGUILayout.Foldout(showNavigation, lblNavigationFoldout, foldoutStyle);
        GUI.backgroundColor = bgColor;

        if (showNavigation)
        {
            // The tabs to alternate between specific menus
            GUI.backgroundColor = btnColorDark;
            GUILayout.BeginHorizontal();
            tabSelection = GUILayout.SelectionGrid(tabSelection, tabLabels, 3);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = bgColor;

        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        ///
        switch (tabSelection)
        {
            case (int)Tabs.Agent:
                agentWindow.OnWindowOpen(agentReference);
                break;
            case (int)Tabs.Resources:
                OnResourcesOpen();
                break;
            case (int)Tabs.Batching:
                EditorGUILayout.BeginVertical("Box");
                showBatching = EditorGUILayout.Foldout(showBatching, lblBatchingFoldout, foldoutStyle);
                if (showBatching) batchingWindow.OnWindowOpen();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                showProfiles = EditorGUILayout.Foldout(showProfiles, lblProfilesFoldout, foldoutStyle);
                if (showProfiles) profileWindow.OnWindowOpen();
                EditorGUILayout.EndVertical();
                break;
            case (int)Tabs.Manager:
                managerWindow.OnWindowOpen(managerReference);
                break;
            case (int)Tabs.Visualization:
                managerWindow.OnVisualizationOpen(managerReference);
                UpdateScreenshots();
                break;
            case (int)Tabs.ExpertEvaluation:
                    evaluationWindow.OnWindowOpen(managerReference);
                break;
        }
        GUILayout.EndScrollView();
    }

    private void Update()
    {
        //Temporary solution
        batchingWindow.UpdateBatching();

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
        agentWindow.OnResourceOpen(agentReference, showReferences, showNavigation);

        EditorGUILayout.Space(20);
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(20);

        managerWindow.OnResourceOpen(managerReference);

    }
    private void GrabScreenshotReference()
    {
        if (hasScreenshot && null == screenshot)
            screenshot = EditorUtility.InstanceIDToObject(screenshotID) as ScreenshotManager;
    }
    private void GrabManagerReference()
    {
        if (hasManager && null == managerReference)
            managerReference = EditorUtility.InstanceIDToObject(managerID) as PathOSManager;
    }
    private void GrabAgentReference()
    {
        if (hasAgent && null == agentReference)
            agentReference = EditorUtility.InstanceIDToObject(agentID) as PathOSAgent;
    }

}

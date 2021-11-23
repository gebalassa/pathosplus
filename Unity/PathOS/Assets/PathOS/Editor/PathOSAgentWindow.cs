using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using PathOS;

/*
PathOSAgentWindow.cs 
Nine Penguins (Samantha Stahlke) 2018 (Atiya Nova) 2021
 */

public class PathOSAgentWindow : EditorWindow
{
    //Used to identify preferences string by Unity
    private const string editorPrefsID = "PathOSAgent";

    //Component variables
    [SerializeField]
    //private PathOSAgent agentReference;
    private PathOSAgentMemory memoryReference;
    private PathOSAgentEyes eyeReference;
    private PathOSAgentRenderer rendererReference;

    private Editor currentTransformEditor, currentAgentEditor, currentMemoryEditor,
        currentEyeEditor, currentRendererEditor;

    //Inspector variables
    private SerializedObject serial;

    private GUIStyle foldoutStyle = GUIStyle.none;
    private GUIStyle boldStyle = GUIStyle.none;

    private SerializedProperty experienceScale;
    private SerializedProperty timeScale;

    private bool showPlayerCharacteristics = true;

    private SerializedProperty freezeAgent;

    private bool showNavCharacteristics = false;

    private SerializedProperty exploreDegrees;
    private SerializedProperty invisibleExploreDegrees;
    private SerializedProperty lookDegrees;
    private SerializedProperty visitThreshold;
    private SerializedProperty exploreThreshold;
    private SerializedProperty exploreTargetMargin;

    //Properties for health
    private Texture2D enemy_hazard, enemy_low, enemy_med, enemy_high, enemy_boss, health_low, health_med, health_high;

    private Dictionary<Heuristic, string> heuristicLabels;

    private List<string> profileNames = new List<string>();
    private int profileIndex = 0;
    private bool agentInitialized = false;

    private string lblAgentValues = "Agent Values", lblMemory = "Agent Memory", lblEyes = "Agent Eyes", lblRenderer = "Agent Renderer";
    private static bool showAgentValues = true, showMemory = true, showEyes = true, showRenderer = true;
    private void OnEnable()
    {
        //Load saved settings.
        string prefsData = EditorPrefs.GetString(editorPrefsID, JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(prefsData, this);

        //Health variables
        enemy_low = Resources.Load<Texture2D>("hazard_enemy_low");
        enemy_med = Resources.Load<Texture2D>("hazard_enemy_medium");
        enemy_high = Resources.Load<Texture2D>("hazard_enemy_high");
        enemy_boss = Resources.Load<Texture2D>("hazard_enemy_boss");
        enemy_hazard = Resources.Load<Texture2D>("hazard_environment");
        health_low = Resources.Load<Texture2D>("resource_preservation_low");
        health_med = Resources.Load<Texture2D>("resource_preservation_med");
        health_high = Resources.Load<Texture2D>("resource_preservation_high");
    }

    private void OnDestroy()
    {
        agentInitialized = false;
        PlayerPrefs.SetInt(OGLogManager.overrideFlagId, 0);

        //Save settings to the editor.
        string prefsData = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString(editorPrefsID, prefsData);
    }
    private void OnDisable()
    {
        agentInitialized = false;

        //Save settings to the editor.
        string prefsData = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString(editorPrefsID, prefsData);
    }
    public void OnWindowOpen(PathOSAgent agentReference)
    {
        if (agentReference == null)
        {
            EditorGUILayout.HelpBox("AGENT REFERENCE REQUIRED", MessageType.Error);
            agentInitialized = false;
            return;
        }


        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.Space();

        //Todo: clean this up!
        memoryReference = agentReference.GetComponent<PathOSAgentMemory>();
        eyeReference = agentReference.GetComponent<PathOSAgentEyes>();
        rendererReference = agentReference.GetComponent<PathOSAgentRenderer>();

        if (!agentInitialized) InitializeAgent(agentReference);

        Selection.objects = new Object[] { agentReference.gameObject };

        Editor editor = Editor.CreateEditor(agentReference.gameObject);
        currentAgentEditor = Editor.CreateEditor(agentReference);
        currentMemoryEditor = Editor.CreateEditor(memoryReference);
        currentEyeEditor = Editor.CreateEditor(eyeReference);
        currentRendererEditor = Editor.CreateEditor(rendererReference);
        currentTransformEditor = Editor.CreateEditor(agentReference.gameObject.transform);

        //// Shows the created Editor beneath CustomEditor
        editor.DrawHeader();
        currentTransformEditor.DrawHeader();

        //EditorGUILayout.BeginVertical("Box");
        currentTransformEditor.OnInspectorGUI();
       // EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        currentAgentEditor.DrawHeader();
        EditorGUILayout.BeginVertical("Box");
        showAgentValues = EditorGUILayout.Foldout(showAgentValues, lblAgentValues, foldoutStyle);
        if (showAgentValues) AgentEditorGUI(agentReference);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3.0f);

        EditorGUILayout.BeginVertical("Box");
        showMemory = EditorGUILayout.Foldout(showMemory, lblMemory, foldoutStyle);
        if (showMemory) currentMemoryEditor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3.0f);

        EditorGUILayout.BeginVertical("Box");
        showEyes = EditorGUILayout.Foldout(showEyes, lblEyes, foldoutStyle);
        if (showEyes) currentEyeEditor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3.0f);

        EditorGUILayout.BeginVertical("Box");
        showRenderer = EditorGUILayout.Foldout(showRenderer, lblRenderer, foldoutStyle);
        if (showRenderer) currentRendererEditor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3.0f);
    }

    private void InitializeAgent(PathOSAgent agentReference)
    {
        serial = new SerializedObject(agentReference);
        experienceScale = serial.FindProperty("experienceScale");
        timeScale = serial.FindProperty("timeScale");
        freezeAgent = serial.FindProperty("freezeAgent");
        exploreDegrees = serial.FindProperty("exploreDegrees");
        invisibleExploreDegrees = serial.FindProperty("invisibleExploreDegrees");
        lookDegrees = serial.FindProperty("lookDegrees");
        visitThreshold = serial.FindProperty("visitThreshold");
        exploreThreshold = serial.FindProperty("exploreThreshold");
        exploreTargetMargin = serial.FindProperty("exploreTargetMargin");

        agentReference.RefreshHeuristicList();

        heuristicLabels = new Dictionary<Heuristic, string>();

        foreach (HeuristicScale curScale in agentReference.heuristicScales)
        {
            string label = curScale.heuristic.ToString();

            label = label.Substring(0, 1).ToUpper() + label.Substring(1).ToLower();
            heuristicLabels.Add(curScale.heuristic, label);
        }

        if (null == PathOSProfileWindow.profiles)
            PathOSProfileWindow.ReadPrefsData();

        agentInitialized = true;
    }

    private void AgentEditorGUI(PathOSAgent agentReference)
    {
        serial.Update();

        //Placed here since Unity seems to have issues with having these 
        //styles initialized on enable sometimes.
        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(timeScale);
        EditorGUILayout.PropertyField(freezeAgent);

        showPlayerCharacteristics = EditorGUILayout.Foldout(
            showPlayerCharacteristics, "Player Characteristics", foldoutStyle);

        if (showPlayerCharacteristics)
        {
            EditorGUILayout.PropertyField(experienceScale);

            for (int i = 0; i < agentReference.heuristicScales.Count; ++i)
            {
                agentReference.heuristicScales[i].scale = EditorGUILayout.Slider(
                     heuristicLabels[agentReference.heuristicScales[i].heuristic],
                     agentReference.heuristicScales[i].scale, 0.0f, 1.0f);
            }

            boldStyle = EditorStyles.boldLabel;
            EditorGUILayout.LabelField("Load Values from Profile", boldStyle);

            profileNames.Clear();

            if (null == PathOSProfileWindow.profiles)
                PathOSProfileWindow.ReadPrefsData();

            for (int i = 0; i < PathOSProfileWindow.profiles.Count; ++i)
            {
                profileNames.Add(PathOSProfileWindow.profiles[i].name);
            }

            if (profileNames.Count == 0)
                profileNames.Add("--");

            EditorGUILayout.BeginHorizontal();

            profileIndex = EditorGUILayout.Popup(profileIndex, profileNames.ToArray());

            if (GUILayout.Button("Apply Profile")
                && profileIndex < PathOSProfileWindow.profiles.Count)
            {
                AgentProfile profile = PathOSProfileWindow.profiles[profileIndex];

                Dictionary<Heuristic, HeuristicRange> ranges = new Dictionary<Heuristic, HeuristicRange>();

                for (int i = 0; i < profile.heuristicRanges.Count; ++i)
                {
                    ranges.Add(profile.heuristicRanges[i].heuristic,
                        profile.heuristicRanges[i]);
                }

                Undo.RecordObject(agentReference, "Apply Agent Profile");
                for (int i = 0; i < agentReference.heuristicScales.Count; ++i)
                {
                    if (ranges.ContainsKey(agentReference.heuristicScales[i].heuristic))
                    {
                        HeuristicRange hr = ranges[agentReference.heuristicScales[i].heuristic];
                        agentReference.heuristicScales[i].scale = Random.Range(hr.range.min, hr.range.max);
                    }
                }

                agentReference.experienceScale = Random.Range(profile.expRange.min, profile.expRange.max);
            }

            EditorGUILayout.EndHorizontal();
        }

        showNavCharacteristics = EditorGUILayout.Foldout(
            showNavCharacteristics, "Navigation", foldoutStyle);

        if (showNavCharacteristics)
        {
            EditorGUILayout.PropertyField(exploreDegrees);
            EditorGUILayout.PropertyField(invisibleExploreDegrees);
            EditorGUILayout.PropertyField(lookDegrees);
            EditorGUILayout.PropertyField(visitThreshold);
            EditorGUILayout.PropertyField(exploreThreshold);
            EditorGUILayout.PropertyField(exploreTargetMargin);
        }

        serial.ApplyModifiedProperties();

        if (GUI.changed && !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(agentReference);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    //Todo: get rid of these bools
    public void OnResourceOpen(PathOSAgent agentReference, bool isNavigationOpen)
    {
        if (agentReference == null)
        {
            EditorGUILayout.HelpBox("AGENT REFERENCE REQUIRED", MessageType.Error);
            agentInitialized = false;
            return;
        }

        int offset = GetOffset(isNavigationOpen);

        EditorGUILayout.Space();

        //Doing the initialization
        if (!agentInitialized) InitializeAgent(agentReference);

        Selection.objects = new Object[] { agentReference.gameObject };

        serial.Update();

        EditorGUIUtility.labelWidth = 200.0f;

        EditorGUILayout.LabelField("Enemy Damage Values", EditorStyles.boldLabel);
        EditorGUILayout.Space(15);

        DrawUIRow(new Rect(20, 140 + offset, 30, 30), enemy_low, "\t Low Enemy Damage", ref agentReference.lowEnemyDamage);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 183 + offset, 30, 30), enemy_med, "\t Medium Enemy Damage", ref agentReference.medEnemyDamage);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 225 + offset, 30, 30), enemy_high, "\t High Enemy Damage", ref agentReference.highEnemyDamage);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 265 + offset, 30, 30), enemy_boss, "\t Boss Enemy Damage", ref agentReference.bossEnemyDamage);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 307 + offset, 30, 30), enemy_hazard, "\t Hazard Damage", ref agentReference.hazardDamage);

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Resource Values", EditorStyles.boldLabel);
        EditorGUILayout.Space(15);

        DrawUIRow(new Rect(20, 383 + offset, 30, 30), health_low, "\t Low Health Gain", ref agentReference.lowHealthGain);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 425 + offset, 30, 30), health_med, "\t Medium Health Gain", ref agentReference.medHealthGain);

        EditorGUILayout.Space(20);
        DrawUIRow(new Rect(20, 467 + offset, 30, 30), health_high, "\t High Health Gain", ref agentReference.highHealthGain);

        serial.ApplyModifiedProperties();

        if (GUI.changed && !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(agentReference);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private int GetOffset(bool isNavigationOpen)
    {
        if (isNavigationOpen)
        {
            return 38;
        }
        else
        {
            return -8;
        }
    }
    private void DrawUIRow(Rect dimensions, Texture2D icon, string label, ref TimeRange range)
    {
        GUI.DrawTexture(dimensions, icon);
        PathOS.EditorUI.FullMinMaxSlider(label,
               ref range.min,
               ref range.max,
               0.0f,
               100.0f);
    }
}

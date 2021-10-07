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
    private PathOSAgent agentReference;
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
    private SerializedProperty heuristicList;

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
    private SerializedProperty enemyHealthLoss;
    private SerializedProperty hazardHealthLoss;
    private SerializedProperty resourceHealthGain;
    private Texture2D enemy_hazard, enemy_regular, resource_health;

    private Dictionary<Heuristic, string> heuristicLabels;

    private List<string> profileNames = new List<string>();
    private int profileIndex = 0;
    private bool agentInitialized = false;

    [SerializeField]
    private bool hasAgent;

    [SerializeField]
    private int agentID;
    private void OnEnable()
    {
        //Load saved settings.
        string prefsData = EditorPrefs.GetString(editorPrefsID, JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(prefsData, this);

        //Re-establish agent reference, if it has been nullified.
        if (hasAgent)
        {

            if (agentReference != null)
                agentID = agentReference.GetInstanceID();
            else
                agentReference = EditorUtility.InstanceIDToObject(agentID) as PathOSAgent;
        }

        agentInitialized = false;
        hasAgent = agentReference != null;

        //Health variables
        enemy_regular = Resources.Load<Texture2D>("hazard_enemy");
        enemy_hazard = Resources.Load<Texture2D>("hazard_environment");
        resource_health = Resources.Load<Texture2D>("resource_preservation");


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


    public void OnWindowOpen()
    {

        //Not sure if this will work or not
        EditorGUI.BeginChangeCheck();

        GrabAgentReference();
        agentReference = EditorGUILayout.ObjectField("Agent Reference: ", agentReference, typeof(PathOSAgent), true)
            as PathOSAgent;

        //Update agent ID if the user has selected a new object reference.
        if (EditorGUI.EndChangeCheck())
        {
            hasAgent = agentReference != null;
            agentInitialized = false;

            if (hasAgent)
            {
                agentID = agentReference.GetInstanceID();
            }
        }

        if (agentReference == null) return;

        EditorGUILayout.Space();

        //Todo: clean this up!
        memoryReference = agentReference.GetComponent<PathOSAgentMemory>();
        eyeReference = agentReference.GetComponent<PathOSAgentEyes>();
        rendererReference = agentReference.GetComponent<PathOSAgentRenderer>();

        if (!agentInitialized) InitializeAgent();

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
        currentTransformEditor.OnInspectorGUI();
        EditorGUILayout.Space();

        currentAgentEditor.DrawHeader();
        AgentEditorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        currentMemoryEditor.DrawHeader();
        currentMemoryEditor.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        currentEyeEditor.DrawHeader();
        currentEyeEditor.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        currentRendererEditor.DrawHeader();
        currentRendererEditor.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private void InitializeAgent()
    {
        serial = new SerializedObject(agentReference);
        experienceScale = serial.FindProperty("experienceScale");
        timeScale = serial.FindProperty("timeScale");
        heuristicList = serial.FindProperty("heuristicScales");

        freezeAgent = serial.FindProperty("freezeAgent");

        exploreDegrees = serial.FindProperty("exploreDegrees");
        invisibleExploreDegrees = serial.FindProperty("invisibleExploreDegrees");
        lookDegrees = serial.FindProperty("lookDegrees");
        visitThreshold = serial.FindProperty("visitThreshold");
        exploreThreshold = serial.FindProperty("exploreThreshold");
        exploreTargetMargin = serial.FindProperty("exploreTargetMargin");

        enemyHealthLoss = serial.FindProperty("enemyHealthLoss");
        hazardHealthLoss = serial.FindProperty("hazardHealthLoss");
        resourceHealthGain = serial.FindProperty("resourceHealthGain");

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

    private void AgentEditorGUI()
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
    private void GrabAgentReference()
    {
        if (hasAgent && null == agentReference)
            agentReference = EditorUtility.InstanceIDToObject(agentID) as PathOSAgent;
    }
    public void OnResourceOpen()
    {
        EditorGUI.BeginChangeCheck();

        GrabAgentReference();
        agentReference = EditorGUILayout.ObjectField("Agent Reference: ", agentReference, typeof(PathOSAgent), true)
            as PathOSAgent;

        //Update agent ID if the user has selected a new object reference.
        if (EditorGUI.EndChangeCheck())
        {
            hasAgent = agentReference != null;
            agentInitialized = false;

            if (hasAgent)
            {
                agentID = agentReference.GetInstanceID();
            }
        }

        if (agentReference == null) return;

        EditorGUILayout.Space();

        //Doing the initialization
        if (!agentInitialized) InitializeAgent();

        Selection.objects = new Object[] { agentReference.gameObject };

        serial.Update();

        EditorGUI.indentLevel += 4;
        EditorGUIUtility.labelWidth = 200.0f;
        EditorGUILayout.Space(25);

        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 130, 30, 30), enemy_regular);
        enemyHealthLoss.floatValue = EditorGUILayout.FloatField("Regular Enemy Damage: ", enemyHealthLoss.floatValue);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 170, 30, 30), enemy_hazard);
        hazardHealthLoss.floatValue = EditorGUILayout.FloatField("Hazard Enemy Damage: ", hazardHealthLoss.floatValue);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 215, 30, 30), resource_health);
        resourceHealthGain.floatValue = EditorGUILayout.FloatField("Resource Health Gain: ", resourceHealthGain.floatValue);
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel -= 4;

        serial.ApplyModifiedProperties();

        if (GUI.changed && !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(agentReference);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}

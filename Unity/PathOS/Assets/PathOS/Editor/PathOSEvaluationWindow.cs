using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using PathOS;
using Malee.Editor;

/*
PathOSEvaluationWindow.cs 
(Atiya Nova) 2021
 */

public enum Severity
{
    NONE = 0,
    LOW = 1,
    MED = 2,
    HIGH = 3,
}

public enum Category
{
    NONE = 0,
    POS = 1,
    NEG = 2,
}

//When you finally get time, please clean this up
[Serializable]
public class UserComment
{
    public string description;
    public bool categoryFoldout;
    public Severity severity;
    public Category category;
    public GameObject selection;
    public EntityType entityType;

    [SerializeField]
    public int selectionID;

    public UserComment()
    {
        description = "";
        categoryFoldout = false;
        severity = Severity.NONE;
        category = Category.NONE;
        selection = null;
        selectionID = 0;
        entityType = EntityType.ET_NONE;
    }

    public UserComment(string description, bool categoryFoldout, Severity severity, Category category, GameObject selection, EntityType entityType)
    {
        this.description = description;
        this.categoryFoldout = categoryFoldout;
        this.severity = severity;
        this.category = category;
        this.selection = selection;
        selectionID = selection.GetInstanceID();
        this.entityType = entityType;
    }
}

[Serializable]
class ExpertEvaluation 
{ 
    //TODO: Spread things out in here to clean it up
    public List<UserComment> userComments = new List<UserComment>();
    private GUIStyle foldoutStyle = GUIStyle.none, buttonStyle = GUIStyle.none, labelStyle = GUIStyle.none;

    private readonly string[] severityNames = new string[] { "NA", "LOW", "MED", "HIGH" };
    private readonly string[] entityNames = new string[] { "NONE", "OPTIONAL GOAL", "MANDATORY GOAL", "COMPLETION GOAL", "ACHIEVEMENT", "PRESERVATION LOW",
    "PRESERVATION MED", "PRESERVATION HIGH", "LOW ENEMY", "MED ENEMY", "HIGH ENEMY", "BOSS", "ENVIRONMENT HAZARD", "POI", "NPC POI"};
    private readonly string[] categoryNames = new string[] { "NA", "POS", "NEG" };
    private readonly string headerRow = "#";
    private Color[] severityColorsPos = new Color[] { Color.white, new Color32(175, 239, 169, 255), new Color32(86, 222, 74,255), new Color32(43, 172, 32,255) };
    private Color[] severityColorsNeg = new Color[] { Color.white, new Color32(232, 201, 100, 255), new Color32(232, 142, 100,255), new Color32(248, 114, 126, 255) };
    private Color[] categoryColors = new Color[] { Color.white, Color.green, new Color32(248, 114, 126, 255) };
    private Color entityColor = new Color32(60, 145, 255, 120);

    public void SaveData()
    {
        string saveName;
        Scene scene = SceneManager.GetActiveScene();

        saveName = scene.name + " heuristicAmount";

        int counter = userComments.Count;
        PlayerPrefs.SetInt(saveName, counter);

        for (int i = 0; i < userComments.Count; i++)
        {
            saveName = scene.name + " heuristicsInputs " + i;

            PlayerPrefs.SetString(saveName, userComments[i].description);

            saveName = scene.name + " heuristicsSeverities " + i;

            PlayerPrefs.SetInt(saveName, (int)userComments[i].severity);

            saveName = scene.name + " heuristicsCategories " + i;

            PlayerPrefs.SetInt(saveName, (int)userComments[i].category);

            saveName = scene.name + " selectionID " + i;

            PlayerPrefs.SetInt(saveName, userComments[i].selectionID);

            saveName = scene.name + " entityType " + i;

            PlayerPrefs.SetInt(saveName, (int)userComments[i].entityType);
        }
    }

    public void LoadData()
    {
        string saveName;
        Scene scene = SceneManager.GetActiveScene();
        int counter = 0;

        userComments.Clear();

        saveName = scene.name + " heuristicAmount";

        if (PlayerPrefs.HasKey(saveName))
            counter = PlayerPrefs.GetInt(saveName);

        for (int i = 0; i < counter; i++)
        {
            userComments.Add(new UserComment());

            saveName = scene.name + " heuristicsInputs " + i;
            if (PlayerPrefs.HasKey(saveName))
                userComments[i].description = PlayerPrefs.GetString(saveName);

            saveName = scene.name + " heuristicsSeverities " + i;
            if (PlayerPrefs.HasKey(saveName))
                userComments[i].severity = (Severity)PlayerPrefs.GetInt(saveName);

            saveName = scene.name + " heuristicsCategories " + i;

            if (PlayerPrefs.HasKey(saveName))
                userComments[i].category = (Category)PlayerPrefs.GetInt(saveName);

            saveName = scene.name + " selectionID " + i;

            userComments[i].selectionID = PlayerPrefs.GetInt(saveName);

            if (userComments[i].selectionID != 0)
                userComments[i].selection = EditorUtility.InstanceIDToObject(userComments[i].selectionID) as GameObject;

            saveName = scene.name + " entityType " + i;

            userComments[i].entityType = (EntityType)PlayerPrefs.GetInt(saveName);
        }

    }

    public void DrawComments()
    {
        EditorGUILayout.Space();

        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontSize = 14;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 15;

        labelStyle.fontSize = 15;
        labelStyle.fontStyle = FontStyle.Italic;

        EditorGUILayout.BeginVertical("Box");

        if (userComments.Count <= 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   There are currently no comments.", labelStyle);
            EditorGUILayout.EndHorizontal();
        }

        for (int i = 0; i < userComments.Count; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Button");
            foldoutStyle.fontStyle = FontStyle.Italic;

            EditorGUILayout.BeginHorizontal();

            userComments[i].categoryFoldout = EditorGUILayout.Foldout(userComments[i].categoryFoldout, "Comment #" + (i+1), foldoutStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("X", GUILayout.Width(17), GUILayout.Height(15)))
            {
                userComments.RemoveAt(i);
                i--;
                SaveData();
                continue;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (!userComments[i].categoryFoldout)
            {
                EditorGUILayout.EndVertical();
                continue;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorStyles.label.wordWrap = true;
            userComments[i].description = EditorGUILayout.TextArea(userComments[i].description, GUILayout.Width(Screen.width * 0.6f));

            GUI.backgroundColor = categoryColors[((int)userComments[i].category)];
            userComments[i].category = (Category)EditorGUILayout.Popup((int)userComments[i].category, categoryNames);

            if (userComments[i].category != Category.POS) GUI.backgroundColor = severityColorsNeg[((int)userComments[i].severity)];
            else GUI.backgroundColor = severityColorsPos[((int)userComments[i].severity)];

            userComments[i].severity = (Severity)EditorGUILayout.Popup((int)userComments[i].severity, severityNames);
            GUI.backgroundColor = severityColorsPos[0];

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            userComments[i].selection = EditorGUILayout.ObjectField("", userComments[i].selection, typeof(GameObject), true, GUILayout.Width(Screen.width * 0.6f))
                as GameObject;


            if (userComments[i].entityType != EntityType.ET_NONE) GUI.backgroundColor = entityColor;
            userComments[i].entityType = IndexToEntity(EditorGUILayout.Popup(EntityToIndex(userComments[i].entityType), entityNames));
            GUI.backgroundColor = severityColorsPos[0];

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (userComments[i].selection != null) userComments[i].selectionID = userComments[i].selection.GetInstanceID(); 
                SaveData();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+", buttonStyle, GUILayout.Width(100)))
        {
            userComments.Add(new UserComment());
            SaveData();
        }
        if (GUILayout.Button("-", buttonStyle, GUILayout.Width(100)))
        {
            if (userComments.Count > 0) 
            {
                userComments.RemoveAt(userComments.Count - 1);
                SaveData();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.EndVertical();

        foldoutStyle.fontSize = 12;

    }

    public void DeleteAll()
    {
        userComments.Clear();

        SaveData();
    }

    public void ImportInputs(string filename)
    {
        StreamReader reader = new StreamReader(filename);

        string line = "";
        string[] lineContents;

        int inputCounter = 0;
        int lineNumber = 0;

        userComments.Clear();

        while ((line = reader.ReadLine()) != null)
        {
            lineContents = line.Split(',');

            if (lineContents.Length < 1)
            {
                Debug.Log("Error! Unable to read line");
                continue;
            }

            lineNumber++;

            if (lineNumber <= 7)
            {
                continue;
            }

            userComments.Add(new UserComment());

            string newDescription = lineContents[1].Replace("  ", "\n").Replace("/", ",");
            userComments[inputCounter].description = newDescription;

            userComments[inputCounter].severity = StringToSeverity(lineContents[2]);

            userComments[inputCounter].category = StringToCategory(lineContents[3]);

            if (lineContents[4] == "No GameObject")
            {
                userComments[inputCounter].selection = null;
                userComments[inputCounter].selectionID = 0;
            }
            else
            {
                userComments[inputCounter].selectionID = int.Parse(lineContents[5]);
                userComments[inputCounter].selection = EditorUtility.InstanceIDToObject(userComments[inputCounter].selectionID) as GameObject; 
            }

            userComments[inputCounter].entityType = StringToEntityType(lineContents[6]);

            inputCounter++;
        }

        reader.Close();

        SaveData();
    }

    //Exports the heuristics
    public void ExportHeuristics(string filename)
    {
        StreamWriter writer = new StreamWriter(filename);

        List<string> headerComponents = new List<string>();
        List<string> noneComponents = new List<string>();
        List<string> lowComponents = new List<string>();
        List<string> medComponents = new List<string>();
        List<string> highComponents = new List<string>();

        headerComponents.Add("Severity,");
        noneComponents.Add("None,");
        lowComponents.Add("Low,");
        medComponents.Add("Med,");
        highComponents.Add("High,");

        for (int i = 0; i < userComments.Count; i++)
        {
            string entityString = EntityTypeToString(userComments[i].entityType) + ",";

            if (headerComponents.Contains(entityString))
            {
                int index = headerComponents.IndexOf(EntityTypeToString(userComments[i].entityType) + ",");
                
                if (userComments[i].severity == Severity.LOW)
                {
                    lowComponents[index-1] += " #" + (i + 1);
                }
                else if (userComments[i].severity == Severity.MED)
                {
                    medComponents[index-1] += " #" + (i + 1);
                }
                else if (userComments[i].severity == Severity.HIGH)
                {
                    highComponents[index-1] += " #" + (i + 1);
                }
                else
                {
                    noneComponents[index] += " #" + (i + 1);
                }
            }    
            else
            {
                headerComponents.Add(entityString);

                lowComponents.Add(",");
                medComponents.Add(",");
                highComponents.Add(",");
                noneComponents.Add(",");

                if (userComments[i].severity == Severity.LOW)
                {
                    lowComponents[lowComponents.Count-2] += "#" + (i + 1);
                }
                else if (userComments[i].severity == Severity.MED)
                {
                    medComponents[medComponents.Count - 2] += "#" + (i + 1);
                }
                else if (userComments[i].severity == Severity.HIGH)
                {
                    highComponents[highComponents.Count - 2] += "#" + (i + 1);
                }
                else
                {
                    noneComponents[noneComponents.Count - 2] += "#" + (i + 1);
                }
            }
        }

        string header = "";
        string none = "";
        string low = "";
        string med = "";
        string high = "";

        for (int i = 0; i < headerComponents.Count; i++)
        {
            header += headerComponents[i];
            none += noneComponents[i];
            low += lowComponents[i];
            med += medComponents[i];
            high += highComponents[i];
        }

        writer.WriteLine(header);
        writer.WriteLine(none);
        writer.WriteLine(low);
        writer.WriteLine(med);
        writer.WriteLine(high);
        writer.WriteLine("");

        writer.WriteLine("#, Description, Severity, Category, GameObject, Object ID, Entity Type");
        string description, severity, category, number, gameObjectName, ID, entity;

        for (int i = 0; i < userComments.Count; i++)
        {
            number = (i + 1).ToString();
            description = userComments[i].description.Replace("\r", "").Replace("\n", "  ").Replace(",", "/");

            severity = SeverityToString(userComments[i].severity);

            category = CategoryToString(userComments[i].category);

            if (userComments[i].selection != null)
            {
                gameObjectName = userComments[i].selection.name;
                ID = userComments[i].selectionID.ToString();
            }
            else
            {
                gameObjectName = "No GameObject";
                ID = "NA";
            }

            entity = entityNames[EntityToIndex(userComments[i].entityType)];

            writer.WriteLine(number + ',' + description + ',' + severity + ',' + category + ',' + gameObjectName + ',' + ID + ',' + entity);
        }

        writer.Close();
        
        SaveData();
    }


    private string SeverityToString(Severity name)
    {
        switch (name)
        {
            case Severity.NONE:
                return "NA";
            case Severity.LOW:
                return "LOW";
            case Severity.MED:
                return "MED";
            case Severity.HIGH:
                return "HIGH";
            default:
                return "NA";
        }
    }

    private EntityType StringToEntityType(string name)
    {
        switch (name)
        {
            case "NONE":
                return EntityType.ET_NONE;
            case "OPTIONAL GOAL":
                return EntityType.ET_GOAL_OPTIONAL;
            case "MANDATORY GOAL":
                return EntityType.ET_GOAL_MANDATORY;
            case "COMPLETION GOAL":
                return EntityType.ET_GOAL_COMPLETION;
            case "ACHIEVEMENT":
                return EntityType.ET_RESOURCE_ACHIEVEMENT;
            case "PRESERVATION LOW":
                return EntityType.ET_RESOURCE_PRESERVATION_LOW;
            case "PRESERVATION MED":
                return EntityType.ET_RESOURCE_PRESERVATION_MED;
            case "PRESERVATION HIGH":
                return EntityType.ET_RESOURCE_PRESERVATION_HIGH;
            case "LOW ENEMY":
                return EntityType.ET_HAZARD_ENEMY_LOW;
            case "MED ENEMY":
                return EntityType.ET_HAZARD_ENEMY_MED;
            case "HIGH ENEMY":
                return EntityType.ET_HAZARD_ENEMY_HIGH;
            case "BOSS":
                return EntityType.ET_HAZARD_ENEMY_BOSS;
            case "ENVIRONMENT HAZARD":
                return EntityType.ET_HAZARD_ENVIRONMENT;
            case "POI":
                return EntityType.ET_POI;
            case "NPC POI":
                return EntityType.ET_POI_NPC;
            default:
                return EntityType.ET_NONE;
        }
    }


    private String EntityTypeToString(EntityType name)
    {
        switch (name)
        {
            case EntityType.ET_NONE:
                return "NONE";
            case EntityType.ET_GOAL_OPTIONAL:
                return "OPTIONAL GOAL";
            case EntityType.ET_GOAL_MANDATORY:
                return "MANDATORY GOAL";
            case EntityType.ET_GOAL_COMPLETION:
                return "COMPLETION GOAL";
            case EntityType.ET_RESOURCE_ACHIEVEMENT:
                return "ACHIEVEMENT";
            case EntityType.ET_RESOURCE_PRESERVATION_LOW:
                return "PRESERVATION LOW";
            case EntityType.ET_RESOURCE_PRESERVATION_MED:
                return "PRESERVATION MED";
            case EntityType.ET_RESOURCE_PRESERVATION_HIGH:
                return "PRESERVATION HIGH";
            case EntityType.ET_HAZARD_ENEMY_LOW:
                return "LOW ENEMY";
            case EntityType.ET_HAZARD_ENEMY_MED:
                return "MED ENEMY";
            case EntityType.ET_HAZARD_ENEMY_HIGH:
                return "HIGH ENEMY";
            case EntityType.ET_HAZARD_ENEMY_BOSS:
                return "BOSS";
            case EntityType.ET_HAZARD_ENVIRONMENT:
                return "ENVIRONMENT HAZARD";
            case EntityType.ET_POI:
                return "POI";
            case EntityType.ET_POI_NPC:
                return "NPC POI";
            default:
                return "NONE";
        }
    }

    private Severity StringToSeverity(string name)
    {
        switch (name)
        {
            case "NA":
                return Severity.NONE;
            case "LOW":
                return Severity.LOW;
            case "MED":
                return Severity.MED;
            case "HIGH":
                return Severity.HIGH;
            default:
                return Severity.NONE;
        }
    }
    private string CategoryToString(Category name)
    {
        switch (name)
        {
            case Category.NONE:
                return "NA";
            case Category.POS:
                return "POS";
            case Category.NEG:
                return "NEG";
            default:
                return "NA";
        }
    }
    private Category StringToCategory(string name)
    {
        switch (name)
        {
            case "NA":
                return Category.NONE;
            case "POS":
                return Category.POS;
            case "NEG":
                return Category.POS;
            default:
                return Category.NONE;
        }
    }

    public void AddNewComment(UserComment comment)
    {
        userComments.Add(comment);
        SaveData();
    }

    private int EntityToIndex(EntityType type)
    {
        switch (type)
        {
            case EntityType.ET_NONE:
                return 0;
            case EntityType.ET_GOAL_OPTIONAL:
                return 1;
            case EntityType.ET_GOAL_MANDATORY:
                return 2;
            case EntityType.ET_GOAL_COMPLETION:
                return 3;
            case EntityType.ET_RESOURCE_ACHIEVEMENT:
                return 4;
            case EntityType.ET_RESOURCE_PRESERVATION_LOW:
                return 5;
            case EntityType.ET_RESOURCE_PRESERVATION_MED:
                return 6;
            case EntityType.ET_RESOURCE_PRESERVATION_HIGH:
                return 7;
            case EntityType.ET_HAZARD_ENEMY_LOW:
                return 8;
            case EntityType.ET_HAZARD_ENEMY_MED:
                return 9;
            case EntityType.ET_HAZARD_ENEMY_HIGH:
                return 10;
            case EntityType.ET_HAZARD_ENEMY_BOSS:
                return 11;
            case EntityType.ET_HAZARD_ENVIRONMENT:
                return 12;
            case EntityType.ET_POI:
                return 13;
            case EntityType.ET_POI_NPC:
                return 14;
            default:
                return 0;
        }
    }

    private EntityType IndexToEntity(int index)
    {

        switch (index)
        {
            case 0:
                return EntityType.ET_NONE;
            case 1:
                return EntityType.ET_GOAL_OPTIONAL;
            case 2:
                return EntityType.ET_GOAL_MANDATORY;
            case 3:
                return EntityType.ET_GOAL_COMPLETION;
            case 4:
                return EntityType.ET_RESOURCE_ACHIEVEMENT;
            case 5:
                return EntityType.ET_RESOURCE_PRESERVATION_LOW;
            case 6:
                return EntityType.ET_RESOURCE_PRESERVATION_MED;
            case 7:
                return EntityType.ET_RESOURCE_PRESERVATION_HIGH;
            case 8:
                return EntityType.ET_HAZARD_ENEMY_LOW;
            case 9:
                return EntityType.ET_HAZARD_ENEMY_MED;
            case 10:
                return EntityType.ET_HAZARD_ENEMY_HIGH;
            case 11:
                return EntityType.ET_HAZARD_ENEMY_BOSS;
            case 12:
                return EntityType.ET_HAZARD_ENVIRONMENT;
            case 13:
                return EntityType.ET_POI;
            case 14:
                return EntityType.ET_POI_NPC;
            default:
                return EntityType.ET_NONE;
        }
    }
}

public class PathOSEvaluationWindow : EditorWindow
{
    private Color bgColor, btnColor;
    private PathOSManager managerReference;
    ExpertEvaluation comments = new ExpertEvaluation();
    private GUIStyle headerStyle = new GUIStyle();
    private GameObject selection = null;
    static bool popupAlreadyOpen = false;
    private string expertEvaluation = "Expert Evaluation", deleteAll = "DELETE ALL", import = "IMPORT", export = "EXPORT";
    Popup window;
    private const string editorPrefsID = "PathOSEvaluationWindow";
    public static PathOSEvaluationWindow instance { get; private set; }

    private void OnEnable()
    {
        //Sets instance
        instance = this;

        //Background color
        comments.LoadData();
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);

        SceneView.onSceneGUIDelegate += this.OnSceneGUI;

        //Load saved settings.
        string prefsData = EditorPrefs.GetString(editorPrefsID, JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(prefsData, this);

    }

    private void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

        string prefsData = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString(editorPrefsID, prefsData);
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

        string prefsData = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString(editorPrefsID, prefsData);
    }

    public void OnWindowOpen(PathOSManager reference)
    {
        managerReference = reference;

        if (managerReference == null)
        {
            EditorGUILayout.HelpBox("MANAGER REFERENCE REQUIRED FOR ENTITY TAGGING", MessageType.Error);
        }

        EditorGUILayout.Space(15);

        GUILayout.BeginHorizontal();

        GUI.backgroundColor = btnColor;
        headerStyle.fontSize = 20;

        EditorGUILayout.LabelField(expertEvaluation, headerStyle);

        if (GUILayout.Button(deleteAll))
        {
            comments.DeleteAll();
        }

        if (GUILayout.Button(import))
        {
            string importPath = EditorUtility.OpenFilePanel("Import Evaluation", "ASSETS\\EvaluationFiles", "csv");

            if (importPath.Length != 0)
            {
                comments.ImportInputs(importPath);
            }
        }

        if (GUILayout.Button(export))
        {
            string exportPath = EditorUtility.OpenFilePanel("Export Evaluation", "ASSETS\\EvaluationFiles", "csv");

            if (exportPath.Length != 0)
            {
                comments.ExportHeuristics(exportPath);
            }
        }

        GUI.backgroundColor = bgColor;
        GUILayout.EndHorizontal();
        comments.DrawComments();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (popupAlreadyOpen || sceneView == null) return;

        //Selection update.
        if (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.ToString() == " (UnityEditor.SceneView)")
        {
            Event e = Event.current;

            if (e != null && e.type == EventType.MouseUp && e.button == 1 && !popupAlreadyOpen)
            {
                selection = HandleUtility.PickGameObject(Event.current.mousePosition, true);

                if (selection != null)
                {
                   // Debug.Log(selection.GetInstanceID());
                    popupAlreadyOpen = true;
                    OpenPopup(selection, GetMarkup(selection));
                }
            }
        }
        else
        {
            selection = null;
        }
    }

    //Please clean this up
    public void AddComment(UserComment comment)
    {
        popupAlreadyOpen = false;

        comments.AddNewComment(comment);
    }

    public void ClosePopup()
    {
        popupAlreadyOpen = false;
    }

    private void OpenPopup(GameObject selection, EntityType entityType)
    {
        window = new Popup();//ScriptableObject.CreateInstance<CommentPopup>();
        window.selection = selection;
        window.entityType = entityType;
        window.position = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 400, 150);
        window.ShowUtility();
    }

    private EntityType GetMarkup(GameObject selection)
    {
        if (managerReference == null) return EntityType.ET_NONE;

        for (int i = 0; i < managerReference.levelEntities.Count; i++)
        {
            //Looks into the pathos manager to figure out if that object has a tag
            if (managerReference.levelEntities[i].objectRef == selection)
            {
                return managerReference.levelEntities[i].entityType;
            }
        }

        return EntityType.ET_NONE;
    }
}

//Really messy, rushed implementation. Please clean this up
public class Popup : EditorWindow
{
    private string description = "";
    Severity severity = Severity.NONE;
    Category category = Category.NONE;

    private readonly string[] severityNames = new string[] { "NA", "LOW", "MED", "HIGH" };
    private readonly string[] categoryNames = new string[] { "NA", "POS", "NEG" };
    private readonly string[] entityNames = new string[] { "NONE", "OPTIONAL GOAL", "MANDATORY GOAL", "COMPLETION GOAL", "ACHIEVEMENT", "PRESERVATION LOW",
    "PRESERVATION MED", "PRESERVATION HIGH", "LOW ENEMY", "MED ENEMY", "HIGH ENEMY", "BOSS", "ENVIRONMENT HAZARD", "POI", "NPC POI"};

    private Color[] severityColorsPos = new Color[] { Color.white, new Color32(175, 239, 169, 255), new Color32(86, 222, 74, 255), new Color32(43, 172, 32, 255) };
    private Color[] severityColorsNeg = new Color[] { Color.white, new Color32(232, 201, 100, 255), new Color32(232, 142, 100, 255), new Color32(248, 114, 126, 255) };
    private Color[] categoryColors = new Color[] { Color.white, Color.green, new Color32(248, 114, 126, 255) };
    private Color entityColor = new Color32(60, 145, 255, 120);

    private GUIStyle labelStyle = GUIStyle.none;
    public GameObject selection;
    public EntityType entityType; 

    private void OnDestroy()
    {
        PathOSEvaluationWindow.instance.ClosePopup();
    }

    private void OnDisable()
    {
        PathOSEvaluationWindow.instance.ClosePopup();
    }

    void OnGUI()
    {
        labelStyle.fontSize = 15;
        labelStyle.fontStyle = FontStyle.Italic;

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical("Box");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("New Comment", labelStyle);

        if (GUILayout.Button("X", GUILayout.Width(17), GUILayout.Height(15)))
        {
            PathOSEvaluationWindow.instance.ClosePopup();
            this.Close();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);


        EditorGUILayout.BeginHorizontal();
        EditorStyles.label.wordWrap = true;
        description = EditorGUILayout.TextArea(description, GUILayout.Width(Screen.width * 0.6f));

        GUI.backgroundColor = categoryColors[((int)category)];
        category = (Category)EditorGUILayout.Popup((int)category, categoryNames);

        if (category != Category.POS) GUI.backgroundColor = severityColorsNeg[((int)severity)];
        else GUI.backgroundColor = severityColorsPos[((int)severity)];
        
        severity = (Severity)EditorGUILayout.Popup((int)severity, severityNames);
        GUI.backgroundColor = severityColorsPos[0];
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();

        selection = EditorGUILayout.ObjectField("", selection, typeof(GameObject), true, GUILayout.Width(Screen.width * 0.6f))
            as GameObject;

        //entityType = (EntityType)EditorGUILayout.Popup(EntityToIndex(entityType), entityNames);
        if (entityType != EntityType.ET_NONE) GUI.backgroundColor = entityColor;
        entityType = IndexToEntity(EditorGUILayout.Popup(EntityToIndex(entityType), entityNames));
        GUI.backgroundColor = severityColorsPos[0];

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Add Comment"))
        {
            //evaluationWindow.AddComment();
            PathOSEvaluationWindow.instance.AddComment(new UserComment(description, false, severity, category, selection, entityType));
            this.Close();
        }
    }

    private int EntityToIndex(EntityType type)
    {
        switch (type)
        {
            case EntityType.ET_NONE:
                return 0;
            case EntityType.ET_GOAL_OPTIONAL:
                return 1;
            case EntityType.ET_GOAL_MANDATORY:
                return 2;
            case EntityType.ET_GOAL_COMPLETION:
                return 3;
            case EntityType.ET_RESOURCE_ACHIEVEMENT:
                return 4;
            case EntityType.ET_RESOURCE_PRESERVATION_LOW:
                return 5;
            case EntityType.ET_RESOURCE_PRESERVATION_MED:
                return 6;
            case EntityType.ET_RESOURCE_PRESERVATION_HIGH:
                return 7;
            case EntityType.ET_HAZARD_ENEMY_LOW:
                return 8;
            case EntityType.ET_HAZARD_ENEMY_MED:
                return 9;
            case EntityType.ET_HAZARD_ENEMY_HIGH:
                return 10;
            case EntityType.ET_HAZARD_ENEMY_BOSS:
                return 11;
            case EntityType.ET_HAZARD_ENVIRONMENT:
                return 12;
            case EntityType.ET_POI:
                return 13;
            case EntityType.ET_POI_NPC:
                return 14;
            default:
                return 0;
        }
    }

    private EntityType IndexToEntity(int index)
    {

        switch (index)
        {
            case 0:
                return EntityType.ET_NONE;
            case 1:
                return EntityType.ET_GOAL_OPTIONAL;
            case 2:
                return EntityType.ET_GOAL_MANDATORY;
            case 3:
                return EntityType.ET_GOAL_COMPLETION;
            case 4:
                return EntityType.ET_RESOURCE_ACHIEVEMENT;
            case 5:
                return EntityType.ET_RESOURCE_PRESERVATION_LOW;
            case 6:
                return EntityType.ET_RESOURCE_PRESERVATION_MED;
            case 7:
                return EntityType.ET_RESOURCE_PRESERVATION_HIGH;
            case 8:
                return EntityType.ET_HAZARD_ENEMY_LOW;
            case 9:
                return EntityType.ET_HAZARD_ENEMY_MED;
            case 10:
                return EntityType.ET_HAZARD_ENEMY_HIGH;
            case 11:
                return EntityType.ET_HAZARD_ENEMY_BOSS;
            case 12:
                return EntityType.ET_HAZARD_ENVIRONMENT;
            case 13:
                return EntityType.ET_POI;
            case 14:
                return EntityType.ET_POI_NPC;
            default:
                return EntityType.ET_NONE;
        }
    }
}
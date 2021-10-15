using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/*
PathOSEvaluationWindow.cs 
(Atiya Nova) 2021
 */

public enum HeuristicPriority
{
    NONE = 0,
    LOW = 1,
    MED = 2,
    HIGH = 3,
}

//for the love of god please clean this up
class HeuristicSubcategories : MonoBehaviour
{
    public string subcategoryName;
    public bool subcategoryFoldout = false;
    public List<string> heuristics = new List<string>();
    public List<string> heuristicInputs = new List<string>();
    public List<HeuristicPriority> priorities = new List<HeuristicPriority>();
}

[Serializable]
class HeuristicCategory
{
    public string categoryName;
    public bool categoryFoldout = false;
    public List<HeuristicSubcategories> subcategories = new List<HeuristicSubcategories>();
}

class HeuristicGuideline
{
    //TODO: Spread things out in here to clean it up
}

[Serializable]
public class PathOSEvaluationWindow : EditorWindow
{
    static List<HeuristicCategory> loadedCategories = new List<HeuristicCategory>();
    private GUIStyle foldoutStyle = GUIStyle.none;
    private Color bgColor, btnColor;

    //Dropdown enums
    enum DropdownOptions
    {
        NONE = 0,
        PLAY_HEURISTICS = 1
    }

    public string[] dropdownStrings = new string[] { "NONE", "PLAY HEURISTICS" };
    public string[] priorityStrings = new string[] { "NA", "LOW", "MED", "HIGH" };
    public Color[] priorityColors = new Color[] { Color.white, Color.green, Color.yellow, new Color32(248, 114, 126, 255) };
    static DropdownOptions dropdowns = DropdownOptions.NONE;
    static int selected = 0;
    bool initialized = false;

    private void OnEnable()
    {
        //Background color
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);

        //Loading saved data

        if (PlayerPrefs.HasKey("selected"))
        {
            selected = PlayerPrefs.GetInt("selected");
        }

        if (PlayerPrefs.HasKey("initialized"))
        {
            initialized = PlayerPrefs.GetInt("initialized") == 1 ? true : false;
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("selected", selected);
        SaveInputs();

    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("selected", selected);
        SaveInputs();
    }

    private void SaveInputs()
    {
        string filename;
        for (int t = 0; t < loadedCategories.Count; t++)
        {
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    filename = "heuristicsInputs " + t + " " + j + " " + i;
                    PlayerPrefs.SetString(filename, loadedCategories[t].subcategories[j].heuristicInputs[i]);

                    filename = "heuristicsPriorities " + t + " " + j + " " + i;
                    PlayerPrefs.SetInt(filename, (int)loadedCategories[t].subcategories[j].priorities[i]);
                }
            }
        }
    }
    public void OnWindowOpen()
    {
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 70.0f;
        GUILayout.BeginHorizontal();

        selected = EditorGUILayout.Popup("Heuristics:", selected, dropdownStrings);
        GUILayout.EndHorizontal();

        if (selected != 0)
        {
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button("CLEAR"))
            {
                ClearCurrentInputs();
            }

            if (GUILayout.Button("IMPORT"))
            {

            }

            if (GUILayout.Button("EXPORT"))
            {
                ExportHeuristics();
            }

            GUI.backgroundColor = bgColor;
        }

        GUILayout.EndHorizontal();

        if (dropdowns != (DropdownOptions)selected)
        {
            dropdowns = (DropdownOptions)selected;
            SaveInputs(); //Save when the player switches categories

            switch (dropdowns)
            {
                case DropdownOptions.NONE:
                    loadedCategories.Clear();
                    break;
                case DropdownOptions.PLAY_HEURISTICS:
                        LoadHeuristics("ASSETS\\EvaluationFiles\\playheuristics.txt");
                    break;
            }
        }

        switch (dropdowns)
        {
            case DropdownOptions.PLAY_HEURISTICS:
                DrawPlayHeuristics();
                break;
        }

    }

    private void DrawPlayHeuristics()
    {

        if (loadedCategories.Count <= 0) return;

        EditorGUILayout.Space();

        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontSize = 13;

        //girl what is this
        for (int t = 0; t < loadedCategories.Count; t++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Toolbar");
            foldoutStyle.fontStyle = FontStyle.Bold;
            loadedCategories[t].categoryFoldout = EditorGUILayout.Foldout(loadedCategories[t].categoryFoldout, loadedCategories[t].categoryName, foldoutStyle);
            GUI.backgroundColor = bgColor;
            EditorGUILayout.EndVertical();

            if (!loadedCategories[t].categoryFoldout) continue;

            EditorGUI.indentLevel++;
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                EditorGUILayout.BeginVertical("Box");
                foldoutStyle.fontStyle = FontStyle.Italic;
                loadedCategories[t].subcategories[j].subcategoryFoldout = EditorGUILayout.Foldout(loadedCategories[t].subcategories[j].subcategoryFoldout, loadedCategories[t].subcategories[j].subcategoryName, foldoutStyle);
                EditorGUILayout.EndVertical();

                if (!loadedCategories[t].subcategories[j].subcategoryFoldout)
                    continue;

                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    EditorStyles.label.wordWrap = true;
                    EditorGUILayout.LabelField(loadedCategories[t].subcategories[j].heuristics[i], GUILayout.MaxWidth(Screen.width * 0.7f));
                    EditorGUILayout.BeginHorizontal();
                    loadedCategories[t].subcategories[j].heuristicInputs[i] = EditorGUILayout.TextArea(loadedCategories[t].subcategories[j].heuristicInputs[i], GUILayout.Width(Screen.width * 0.7f));
                    GUI.backgroundColor = priorityColors[((int)loadedCategories[t].subcategories[j].priorities[i])];
                    loadedCategories[t].subcategories[j].priorities[i] = (HeuristicPriority)EditorGUILayout.Popup((int)loadedCategories[t].subcategories[j].priorities[i], priorityStrings);
                    GUI.backgroundColor = priorityColors[0];
                    EditorGUILayout.EndHorizontal();
                }

            }
            EditorGUI.indentLevel--;
        }
    }

    public void LoadHeuristics(string filename)
    {
        string line;
        StreamReader reader = new StreamReader(filename);

        int categoryCounter = -1;
        int subcategoryCounter = -1;

        //Clears list so that we can populate it with file info
        loadedCategories.Clear();

        //Print the text from the file
        while ((line = reader.ReadLine()) != null)
        {
            if (line[0] == '*')
            {
                subcategoryCounter = -1;
                categoryCounter++;
                loadedCategories.Add(new HeuristicCategory());
                loadedCategories[categoryCounter].categoryName = line.TrimStart('*');
                continue;
            }
            else if (line[0] == '%')
            {
                subcategoryCounter++;
                loadedCategories[categoryCounter].subcategories.Add(new HeuristicSubcategories());
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].subcategoryName = line.TrimStart('%');
                continue;
            }

            loadedCategories[categoryCounter].subcategories[subcategoryCounter].heuristics.Add(line);
            loadedCategories[categoryCounter].subcategories[subcategoryCounter].heuristicInputs.Add(" ");
            loadedCategories[categoryCounter].subcategories[subcategoryCounter].priorities.Add(HeuristicPriority.NONE);

        }

        reader.Close();

        for (int t = 0; t < loadedCategories.Count; t++)
        {
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    filename = "heuristicsInputs " + t + " " + j + " " + i;
                    if (PlayerPrefs.HasKey(filename))
                        loadedCategories[t].subcategories[j].heuristicInputs[i] = PlayerPrefs.GetString(filename);

                    filename = "heuristicsPriorities " + t + " " + j + " " + i;
                    if (PlayerPrefs.HasKey(filename))
                        loadedCategories[t].subcategories[j].priorities[i] = (HeuristicPriority) PlayerPrefs.GetInt(filename);
                }
            }
        }
    }

    private void ClearCurrentInputs()
    {
        for (int t = 0; t < loadedCategories.Count; t++)
        {
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    loadedCategories[t].subcategories[j].heuristicInputs[i] = " ";
                    loadedCategories[t].subcategories[j].priorities[i] = HeuristicPriority.NONE;
                }
            }
        }

        SaveInputs();
    }

    private void ExportHeuristics()
    {
        StreamWriter writer = new StreamWriter("ASSETS\\EvaluationFiles\\PLAY_HEURISTICS.csv");

        writer.WriteLine("Type, Description, Input, Priority");
        string type, description, input, priority;

        for (int t = 0; t < loadedCategories.Count; t++)
        {
            type = "*";
            description = loadedCategories[t].categoryName;
            writer.WriteLine(type + "," + description) ;

            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                type = "%";
                description = loadedCategories[t].subcategories[j].subcategoryName;
                writer.WriteLine(type + "," + description);

                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    type = "#";
                    description = loadedCategories[t].subcategories[j].heuristics[i];
                    input = loadedCategories[t].subcategories[j].heuristicInputs[i];
                    priority = ((int)loadedCategories[t].subcategories[j].priorities[i]).ToString();
                    writer.WriteLine(type + "," + description + "," + input + "," + priority); 
                }
            }
        }

        writer.Close();
    }


}

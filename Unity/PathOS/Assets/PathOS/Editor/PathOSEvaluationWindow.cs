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
class HeuristicSubcategories 
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
    public List<HeuristicCategory> loadedCategories = new List<HeuristicCategory>();
    private GUIStyle foldoutStyle = GUIStyle.none;

    private string[] priorityStrings = new string[] { "NA", "LOW", "MED", "HIGH" };
    private string asterisk = "*", percentage = "%", hashtag = "#", headerRow = "Type";
    public string heuristicName;
    private Color[] priorityColors = new Color[] { Color.white, Color.green, Color.yellow, new Color32(248, 114, 126, 255) };
    
    public void SaveData()
    {
        string filename;
        for (int t = 0; t < loadedCategories.Count; t++)
        {
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    filename = heuristicName + " heuristicsInputs " + t + " " + j + " " + i;
                    PlayerPrefs.SetString(filename, loadedCategories[t].subcategories[j].heuristicInputs[i]);

                    filename = heuristicName + " heuristicsPriorities " + t + " " + j + " " + i;
                    PlayerPrefs.SetInt(filename, (int)loadedCategories[t].subcategories[j].priorities[i]);
                }
            }
        }
    }


    public void DrawHeuristics()
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
            GUI.backgroundColor = Color.white;
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
                    EditorGUILayout.LabelField(loadedCategories[t].subcategories[j].heuristics[i], GUILayout.MaxWidth(Screen.width * 0.7f));
                    EditorGUILayout.BeginHorizontal();
                    EditorStyles.label.wordWrap = true;
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
        ImportHeuristics(filename);

        for (int t = 0; t < loadedCategories.Count; t++)
        {
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    filename = heuristicName + " heuristicsInputs " + t + " " + j + " " + i;
                    if (PlayerPrefs.HasKey(filename))
                        loadedCategories[t].subcategories[j].heuristicInputs[i] = PlayerPrefs.GetString(filename);

                    filename = heuristicName + " heuristicsPriorities " + t + " " + j + " " + i;
                    if (PlayerPrefs.HasKey(filename))
                        loadedCategories[t].subcategories[j].priorities[i] = (HeuristicPriority)PlayerPrefs.GetInt(filename);
                }
            }
        }
    }

    public void ClearCurrentInputs()
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

        SaveData();
    }

    public void ImportInputs(string filename)
    {
        StreamReader reader = new StreamReader(filename);

        string line = "";
        string[] lineContents;

        int categoryCounter = -1;
        int subcategoryCounter = -1;
        int inputCounter = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineContents = line.Split(',');

            if (lineContents.Length < 1)
            {
                Debug.Log("Error! Unable to read line");
                continue;
            }

            if (lineContents[0] == headerRow)
            {
                continue;
            }
            if (lineContents[0] == asterisk)
            {
                subcategoryCounter = -1;
                categoryCounter++;
                inputCounter = 0;
                continue;
            }
            else if (lineContents[0] == percentage)
            {
                subcategoryCounter++;
                inputCounter = 0;
                continue;
            }

            else if (lineContents[0] == hashtag)
            {
                if (categoryCounter > loadedCategories.Count || subcategoryCounter > loadedCategories[categoryCounter].subcategories.Count)
                {
                    Debug.Log("Incorrect file type inported");
                    return; 
                }

                loadedCategories[categoryCounter].subcategories[subcategoryCounter].heuristicInputs[inputCounter] = lineContents[2];
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].priorities[inputCounter] = (HeuristicPriority)int.Parse(lineContents[3]);
                inputCounter++;

            }
        }

    }

    public void ImportHeuristics(string filename)
    {
        StreamReader reader = new StreamReader(filename);

        string line = "";
        string[] lineContents;

        int categoryCounter = -1;
        int subcategoryCounter = -1;

        while ((line = reader.ReadLine()) != null)
        {
            lineContents = line.Split(',');

            if (lineContents.Length < 1)
            {
                Debug.Log("Error! Unable to read line");
                continue;
            }

            if (lineContents[0] == headerRow)
            {
                continue;
            }
            if (lineContents[0] == asterisk)
            {
                subcategoryCounter = -1;
                categoryCounter++;
                loadedCategories.Add(new HeuristicCategory());
                loadedCategories[categoryCounter].categoryName = lineContents[1];
                continue;
            }
            else if (lineContents[0] == percentage)
            {
                subcategoryCounter++;
                loadedCategories[categoryCounter].subcategories.Add(new HeuristicSubcategories());
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].subcategoryName = lineContents[1];
                continue;
            }

            else if (lineContents[0] == hashtag)
            {
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].heuristics.Add(lineContents[1]);
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].heuristicInputs.Add(lineContents[2]);
                loadedCategories[categoryCounter].subcategories[subcategoryCounter].priorities.Add((HeuristicPriority)int.Parse(lineContents[3]));
            }
        }

        reader.Close();

    }
    public void ExportHeuristics(string filename)
    {
        StreamWriter writer = new StreamWriter(filename);

        writer.WriteLine("Type, Description, Input, Priority");
        string type, description, input, priority;

        for (int t = 0; t < loadedCategories.Count; t++)
        {
            type = asterisk;
            description = loadedCategories[t].categoryName;
            writer.WriteLine(type + ',' + description);

            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                type = percentage;
                description = loadedCategories[t].subcategories[j].subcategoryName;
                writer.WriteLine(type + ',' + description);

                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    type = hashtag;
                    description = loadedCategories[t].subcategories[j].heuristics[i];
                    input = loadedCategories[t].subcategories[j].heuristicInputs[i];
                    priority = ((int)loadedCategories[t].subcategories[j].priorities[i]).ToString();
                    writer.WriteLine(type + ',' + description + ',' + input + ',' + priority);
                }
            }
        }

        writer.Close();
    }

    
}

[Serializable]
public class PathOSEvaluationWindow : EditorWindow
{
    private Color bgColor, btnColor;

    //Dropdown enums
    enum DropdownOptions
    {
        NONE = 0,
        PLAY_HEURISTICS = 1,
        NIELSEN = 2,
    }

    public string[] dropdownStrings = new string[] { "NONE", "PLAY HEURISTICS", "NIELSEN" };
    public string[] templateLocations = new string[] { "ASSETS\\EvaluationFiles\\PLAY_HEURISTICS_TEMPLATE.csv", "ASSETS\\EvaluationFiles\\NIELSEN_TEMPLATE.csv" };
    public string[] heuristicNames = new string[] { "PLAY", "NIELSEN" };

    static DropdownOptions dropdowns = DropdownOptions.NONE;
    HeuristicGuideline[] heuristics = new HeuristicGuideline[2];
    static int selected = 0;


    private void OnEnable()
    {
        //Background color
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);

        //Setting heuristic names
        for (int i = 0; i < heuristics.Length; i++)
        {
            heuristics[i] = new HeuristicGuideline();
            heuristics[i].heuristicName = heuristicNames[i];
        }

        //Loading saved data

        if (PlayerPrefs.HasKey("selected"))
        {
            selected = PlayerPrefs.GetInt("selected");
        }

        
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("selected", selected);
        for (int i = 0; i < heuristics.Length; i++)
            heuristics[i].SaveData();

    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("selected", selected);
        for (int i = 0; i < heuristics.Length; i++)
            heuristics[i].SaveData();
    }

    public void OnWindowOpen()
    {
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 70.0f;
        GUILayout.BeginHorizontal();

        selected = EditorGUILayout.Popup("Heuristics:", selected, dropdownStrings);
        GUILayout.EndHorizontal();

        if (selected > 0)
        {
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button("CLEAR"))
            {
                heuristics[selected-1].ClearCurrentInputs();
            }

            if (GUILayout.Button("IMPORT"))
            {
                string importPath = EditorUtility.OpenFilePanel("Import Evaluation", "ASSETS\\EvaluationFiles", "csv");

                if (importPath.Length != 0)
                {
                    heuristics[selected - 1].ImportInputs(importPath);
                }
            }

            if (GUILayout.Button("EXPORT"))
            {
                string importPath = EditorUtility.OpenFilePanel("Import Evaluation", "ASSETS\\EvaluationFiles", "csv");

                if (importPath.Length != 0)
                {
                    heuristics[selected - 1].ExportHeuristics(importPath);
                }
            }

            GUI.backgroundColor = bgColor;
        }

        GUILayout.EndHorizontal();

        if (dropdowns != (DropdownOptions)selected)
        {
            dropdowns = (DropdownOptions)selected;

            if (dropdowns == DropdownOptions.NONE)
            {
                return;
            }

            heuristics[selected - 1].loadedCategories.Clear();
            heuristics[selected - 1].LoadHeuristics(templateLocations[selected - 1]);
        }

        if (selected <= 0)
        {
            return;
        }

        heuristics[selected - 1].DrawHeuristics();
        heuristics[selected - 1].SaveData();
    }
}

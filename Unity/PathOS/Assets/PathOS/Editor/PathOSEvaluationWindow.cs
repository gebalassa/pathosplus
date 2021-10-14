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

//for the love of god please clean this up
class HeuristicSubcategories
{
    public string subcategoryName;
    public bool subcategoryFoldout = false;
    public List<string> heuristics = new List<string>();
    public List<string> heuristicInputs = new List<string>();
}

class HeuristicCategory
{
    public string categoryName;
    public bool categoryFoldout = false;
    public List<HeuristicSubcategories> subcategories = new List<HeuristicSubcategories>();
}

public class PathOSEvaluationWindow : EditorWindow
{
    List<HeuristicCategory> loadedCategories = new List<HeuristicCategory>();
    private GUIStyle foldoutStyle = GUIStyle.none;
    private Color bgColor, btnColor;

    //Dropdown enums
    enum DropdownOptions
    {
        NONE = 0,
        PLAY_HEURISTICS = 1
    }

    public string[] dropdownStrings = new string[] { "NONE", "PLAY HEURISTICS" };
    DropdownOptions dropdowns = DropdownOptions.NONE;
    int selected = 0;

    private void OnEnable()
    {
        //Background color
        bgColor = GUI.backgroundColor;
        btnColor = new Color32(200, 203, 224, 255);
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
            if (GUILayout.Button("SAVE"))
            {

            }

            if (GUILayout.Button("CLEAR"))
            {
                ClearCurrentInputs();
            }

            if (GUILayout.Button("IMPORT"))
            {

            }

            if (GUILayout.Button("EXPORT"))
            {

            }
            GUI.backgroundColor = bgColor;
        }

        GUILayout.EndHorizontal();

        if (dropdowns != (DropdownOptions)selected)
        {
            dropdowns = (DropdownOptions)selected;

            switch (dropdowns)
            {
                case DropdownOptions.NONE:
                    loadedCategories.Clear();
                    break;
                case DropdownOptions.PLAY_HEURISTICS:
                    LoadHeuristics("ASSETS\\playheuristics.txt");
                    break;
            }
        }


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
                    EditorGUI.indentLevel+=2;
                    EditorGUILayout.LabelField(loadedCategories[t].subcategories[j].heuristics[i]);
                    loadedCategories[t].subcategories[j].heuristicInputs[i] = EditorGUILayout.TextArea(loadedCategories[t].subcategories[j].heuristicInputs[i]);
                    EditorGUI.indentLevel-=2;
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
        }

        reader.Close();
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
                }
            }
        }
    }
}

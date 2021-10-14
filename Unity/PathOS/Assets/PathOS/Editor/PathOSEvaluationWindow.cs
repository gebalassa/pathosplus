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

class HeuristicCategory : EditorWindow
{
    public string categoryName;
    public bool categoryFoldout = false;
    public List<HeuristicSubcategories> subcategories = new List<HeuristicSubcategories>();
}

public class PathOSEvaluationWindow : EditorWindow
{
    TextAsset loadedHeuristic;
    List<HeuristicCategory> loadedCategories = new List<HeuristicCategory>();
    private GUIStyle foldoutStyle = GUIStyle.none;

    private void OnEnable()
    {
    }

    public void OnWindowOpen()
    {
        if (GUILayout.Button("Load Play Heuristics"))
            LoadHeuristics("ASSETS\\playheuristics.txt");

        if (loadedCategories.Count <= 0) return;
        
        foldoutStyle = EditorStyles.foldout;
        foldoutStyle.fontStyle = FontStyle.Bold;

        //girl what is this
        for (int t = 0; t < loadedCategories.Count; t++)
        {
            loadedCategories[t].categoryFoldout = EditorGUILayout.Foldout(loadedCategories[t].categoryFoldout, loadedCategories[t].categoryName, foldoutStyle);

            if (!loadedCategories[t].categoryFoldout) continue;

            EditorGUI.indentLevel++;
            for (int j = 0; j < loadedCategories[t].subcategories.Count; j++)
            {
                loadedCategories[t].subcategories[j].subcategoryFoldout = EditorGUILayout.Foldout(loadedCategories[t].subcategories[j].subcategoryFoldout, loadedCategories[t].subcategories[j].subcategoryName, foldoutStyle);

                if (!loadedCategories[t].subcategories[j].subcategoryFoldout)
                    continue;

                for (int i = 0; i < loadedCategories[t].subcategories[j].heuristics.Count; i++)
                {
                    EditorGUILayout.LabelField(loadedCategories[t].subcategories[j].heuristics[i]);
                    EditorGUILayout.TextField("enter options here");
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
        }

        reader.Close();
    }
}

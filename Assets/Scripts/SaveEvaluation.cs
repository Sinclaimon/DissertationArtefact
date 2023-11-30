using System.Collections;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Class that takes care of saving evaluations from generated trees
/// </summary>
public static class SaveEvaluation
{
    static List<Population.PopulationStats> populationsToSave = new();

    internal static void PopulationIntoJSON(Population.PopulationStats populationStats)
    {
        populationsToSave.Add(populationStats);
    }

    internal static void SaveAllPopulations()
    {
        string allPopsData = JsonConvert.SerializeObject(populationsToSave, Formatting.Indented);

        string filename = "population_" + System.DateTime.Now.DayOfYear + Random.value.GetHashCode();
        WriteToFile(filename + ".json", allPopsData);

        //clear population
        populationsToSave = new();

    }

    internal static void SavePopulation(string filename, List<Population.PopulationStats> populationToSave)
    {
        string allPopsData = JsonConvert.SerializeObject(populationToSave, Formatting.Indented);
        WriteToFile(Path.GetFileName(filename), allPopsData);
    }

    /// <summary>
    /// Generates a json file from a string
    /// Source: https://myunity.dev/en/unity-save-load-json-file-tutorial-in-10-minutes/
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="json"></param>
    private static void WriteToFile(string fileName, string json)
    {
        string path = fileName;

        // If we only got a filename and not the full path
        if (!File.Exists(fileName))
            path = GetFilePath(fileName);

        FileStream fileStream = new(path, FileMode.Create);

        using (StreamWriter writer = new(fileStream))
        {
            writer.Write(json);
        }

        Debug.LogWarning("current population saved to: " + path);
    }

    /// <summary>
    /// Gets a valid filepath based on the project path
    /// Source: https://myunity.dev/en/unity-save-load-json-file-tutorial-in-10-minutes/
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static string GetFilePath(string fileName)
    {
        return Application.dataPath + "/" + fileName;
    }

}

using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System.Linq;

/// <summary>
/// Class that takes care of loading exisitng evaluations
/// </summary>
public static class LoadEvaluations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderName"></param>
    internal static void ReadPopulationDataFromFolder(string folderName)
    {
        if (Directory.Exists(folderName))
        {
            string[] folderFiles = Directory.GetFiles(folderName);

            for (int i = 0; i < folderFiles.Length; i++)
            {
                Debug.Log(folderFiles[i]);
                UpdateEvalFromFile(folderFiles[i]);
            }
        }
    }

    /// <summary>
    /// takes a json filename, deserializes all population stats in it,
    /// updates their metrics and saves them into new json files
    /// </summary>
    /// <param name="fileName"></param>
    private static void UpdateEvalFromFile(string fileName)
    {
        var oldPopulationData = JsonConvert.DeserializeObject<List<Population.PopulationStats>>(File.ReadAllText(fileName));

        List <Population.PopulationStats> allNewPopData = new();

        int genCount = 0;

        // loop through all the population data in the file 
        foreach (var population in oldPopulationData)
        {
            Debug.Log(population.lsystemsData);
            List<Lsystem.LsystemData> newLsystemdataList = new();
            // loop through all the tree data in population
            foreach (var treeData in population.lsystemsData)
            {
                treeData.RecalculateData(fileName);
                newLsystemdataList.Add(treeData);

            }

            // add all the new lsystem data to a population stat list
            allNewPopData.Add(new Population.PopulationStats(genCount, newLsystemdataList));

            genCount++;
        }

        //save the updated population data for each population
        SaveEvaluation.SavePopulation(fileName, allNewPopData);
        Debug.Log("Saved " + fileName);
    }

    /// <summary>
    /// Get the best trees and display them
    /// </summary>
    /// <param name="folderName"></param>
    public static List<string> DisplayBestTrees(string folderName)
    {
        if (Directory.Exists(folderName))
        {
            string[] folderFiles = Directory.GetFiles(folderName);
            var allPopsData = new List<Lsystem.LsystemData>();
            // go through all files in the folder
            for (int i = 0; i < folderFiles.Length; i++)
            {
                // get the population
                var onePopData = (JsonConvert.DeserializeObject<List<Population.PopulationStats>>(File.ReadAllText(folderFiles[i])));
                foreach (var item in onePopData)
                {
                    // add all the LsystemData into a single List
                    allPopsData.AddRange(item.lsystemsData);
                }
                
            }

            // pick top 10 trees and add them in a population
            var bestList = allPopsData.OrderByDescending(o => o.fitness.overallFitness).Take(10);

            // create a new list of strings to store all the best lsystems
            List<string> bestLsystems = new();
            foreach (var item in bestList)
            {
                // add the lsystem sentence from each LsystemData
                item.PrintResults();
                bestLsystems.Add(item.sentence);
            }

            return bestLsystems;
        }
        else
        {
            return null;
        }
    }
}

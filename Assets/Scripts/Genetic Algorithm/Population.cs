using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores all the relevant info of one population (generation)
/// <para>Takes care of Adding trees to the population, storing the list of trees, storing UI tree references, tree weight calculation based on pick order </para>
/// </summary>
[Serializable]
public partial class Population
{
    // stored all the trees of a given gen along
    public List<SimpleTree> treesInPop = new();
    public List<PickTreeFromUI> UItrees = new();
    public int generationNumber { get; }

    // counting how many picks wew have, on 3 we do a new generation and reset
    private int pickCount = 0;

    // default pick weight for trees
    public float defaultWeight;

    public int PickCount { get => pickCount; }

    public Population(float defaultWeight, List<PickTreeFromUI> treeUIs)
    {
        this.defaultWeight = defaultWeight;
        UItrees = treeUIs;

        //update ui tree's current pop
        foreach (var uitree in UItrees)
        {
            uitree.currentPopulation = this;
        }
    }

    // Make a new population in place of an old one
    public Population(Population oldPop, List<SimpleTree> newTrees, Transform[] spawns)
    {
        // Reset default weights, grab references to UI trees and increase generation number
        defaultWeight = oldPop.defaultWeight;
        UItrees = oldPop.UItrees;
        generationNumber = oldPop.generationNumber;
        generationNumber++;
        pickCount = 0;

        // Update ui tree's current population
        foreach (var uitree in UItrees)
        {
            uitree.currentPopulation = this;
        }

        Debug.Log("New population initialized, tree count: " + newTrees.Count);

    }

    /// <summary>
    /// Adds a new tree to the generation and assigns it to a UI tree
    /// </summary>
    /// <param name="treeToAdd"></param>
    /// <param name="IndexUI">position on the screen</param>
    public void AddTreeToPopulation(SimpleTree newTree, int IndexUI, Vector3 worldPosition, bool iterated)
    {
        if (treesInPop.Contains(newTree))
        {
            Debug.LogWarning("this  tree is already in the population: " + newTree.instanceID + ", index: " + IndexUI);
            return;
        }

        // Makes sure tree is in the correct position and has the default pickWeight
        newTree.transform.position = worldPosition;
        newTree.weight = defaultWeight;
        
        // Runs Tree setuo to get it ready to be displayed correctly before adding it to the list
        newTree.TreeSetup();
        treesInPop.Add(newTree);
        newTree.generationNumber = generationNumber;

        // Assigns the tree to a correct UI tree border
        UItrees[IndexUI].AssignTree(newTree);

    }


   
    /// <summary>
    /// Adds the population into the saveEval list to write, gives marks all the given trees
    /// </summary>
    /// <param name="populationTrees"></param>
    /// <param name="saveBranches">save all branch positions? </param>
    internal void SavePopulationStats(List<SimpleTree> populationTrees, bool saveBranches)
    {
        List<Lsystem.LsystemData> treesData = new();
        foreach (var tree in populationTrees)
        {
            if (saveBranches)
                treesData.Add(tree.lsystem.SaveData(tree.weight, tree.BranchCount, TreeMarking.MarkATree(tree), tree.currentBranches));

            else
                treesData.Add(tree.lsystem.SaveData(tree.weight, tree.BranchCount, TreeMarking.MarkATree(tree)));

        }

        PopulationStats populationData = new(generationNumber, treesData);
        SaveEvaluation.PopulationIntoJSON(populationData);

    }

    /// <summary>
    /// Assign a new weight to interacted tree, handle pickCount
    /// </summary>
    /// <param name="selected"></param>
    /// <returns></returns>
    public float SelectedTreeWeight(bool selected)
    {
        // If the tree was selected and the tree has default weight or higher
        if (selected)
        {
            pickCount++;
            // the weight is the Nth root of default weight, exponent as multiplies of 2 
            float weightNew = 1 - Mathf.Pow(defaultWeight, 1.0f / (pickCount * 2));

            return weightNew;
        }

        // If the tree was unselected, return its weight to default
        else
        {
            pickCount--;
            return defaultWeight;
        }
    }

    /// <summary>
    /// Pick a random tree based on the weights
    /// <para>
    /// credit https://tamcarbonart.wordpress.com/2018/10/09/c-pick-random-elements-based-on-probability/
    /// </para> 
    /// </summary>
    /// <returns></returns>
    public SimpleTree PickRandomTreeWeighted()
    {
        // Make sure we normalize weights before picking a random one
        if (!CheckIfWeightsNormalized())
            NormalizeWeights();

        float roll = UnityEngine.Random.value;

        double cumulative = 0.0;

        // Roll for a random tree based on normalized weights
        foreach (var tree in treesInPop)
        {
            cumulative += tree.weight;

            if (roll < cumulative)
            {
                return tree;
            }
        }

        Debug.LogError("No tree was selected");
        return null;
    }


    /// <summary>
    /// Normalize weights of all the trees in this population
    /// </summary>
    public void NormalizeWeights()
    {
        double sum = 0;

        // Get sum of all weights
        foreach (var tree in treesInPop)
        {
            sum += tree.weight;
        }
        Debug.Log("Sum of all weights before normalizing: " + sum);

        // Get the new scale from the sum of all weights
        double newScale = 1.0 / sum;

        // Multiply each weight with the new scale
        foreach (var tree in treesInPop)
        {
            tree.weight *= newScale;
        }

        CheckIfWeightsNormalized();

    }

    /// <summary>
    /// return true if weights are normalized, false if they are not, prints out the result
    /// </summary>
    /// <returns>returns false if the weights have not been normalized yet</returns>
    public bool CheckIfWeightsNormalized()
    {
        double sum = 0;
        foreach (var tree in treesInPop)
        {
            sum += tree.weight;
        }

        if (sum.Equals(1.0))
            return true;

        else
        {
            Debug.LogWarning("Sum of all weights: " + sum);
            return false;
        }
    }

    /// <summary>
    /// Normalizes weights between 0 and 1, this is called before the Selection process
    /// </summary>
    /// <param name="desiredMin"></param>
    /// <param name="desiredMax"></param>
    public void NormalizeStretchWeights(double desiredMin, double desiredMax)
    {
        // Setting helper var
        double minValue = double.MaxValue;
        double maxValue = double.MinValue;

        // Getting the min and max weights
        foreach (var tree in treesInPop)
        {
            if (tree.weight < minValue)
                minValue = tree.weight;
            if (tree.weight > maxValue)
                maxValue = tree.weight;
        }

        // Calc a scaler for normalizing
        double scaler = (desiredMax - desiredMin) / (maxValue - minValue);

        // Normalize all weights
        foreach (var tree in treesInPop)
        {
            tree.weight = (tree.weight - minValue) * scaler + desiredMin;
        }

    }
    public void resetPickCount() => pickCount = 0;

    #region LOG
    /// <summary>
    /// Helper function that will log each tree in a population
    /// </summary>
    public void LogAPopulation()
    {
        Debug.Log("---------------------------------------LOGGING POPULATION---------------------------------------------------");
        foreach (var tree in treesInPop)
        {
            tree.LogLsystem();
        }
    }


    #endregion
}

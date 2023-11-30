using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class GrammaticalEvolution : MonoBehaviour
{
    [Tooltip("All the Genetic operators this Grammatical evolution uses")]
    private List<GAOperator> genOperators = new();
    private Mutation mutation;

    // Default mutation rate and pick weights for trees
    public float mutationRate = .01f;
    public float defaultWeight = 0.01f;

    public Population population;
    [SerializeField] private GameObject treeObject;
    public int populationSize = 10;

    // All spawn points
    [Tooltip("All the spawnpoints alligned with the UI")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject UIBorders;

    private List<PickTreeFromUI> UItrees;
    // How many generations we want to evolve
    [SerializeField] private int requiredGenerations = 10;
    // How many trees we can pick before evolution
    [SerializeField] private int requiredPicks = 3;

    [SerializeField] private TextMeshProUGUI TitleInstructionsUI;
    [SerializeField] private TextMeshProUGUI GenerationCountUI;

    private bool evalSaved;
    #region UNITY START&UPDATE

    // Start is called before the first frame update
    void Start()
    {
        evalSaved = false;
        mutation = new Mutation(mutationRate);
        genOperators.Add(mutation);

        // init population and get all the UI trees
        UItrees = new(UIBorders.transform.GetComponentsInChildren<PickTreeFromUI>());



        population = new Population(defaultWeight, UItrees);


        // spawn trees based on the population size
        SpawnPopulation();

        // grows the basic trees
        IteratePopulation();

        DrawCurrentPopulation();
        GenerationCountUI.text = "Generation: " + population.generationNumber.ToString();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Handle manual keyboard controls and generation evolve triggers
    private void FixedUpdate()
    {
        // end the evolution cycle when the required number of generations is evolved
        if (!evalSaved && population.generationNumber >= requiredGenerations)
        {
            evalSaved = true;
            // if the static class has a problem with Unity and monobehaviour,
            // just make an instance of it here / start in this script

            //save the last population
            population.SavePopulationStats(population.treesInPop, true);

            // save all the previous populations into a file
            SaveEvaluation.SaveAllPopulations();
            // pause the app
            Time.timeScale = 0;
            TitleInstructionsUI.faceColor = Color.red;
            TitleInstructionsUI.text = "Congratulations, you've evolved the trees 10 times, please go fill out the survey!";

        }

        // evolve the current population if we have picked enough trees
        else if ( population.PickCount >= requiredPicks)
        {
            //evolve a generation
            EvolveOneGeneration(population);
            GenerationCountUI.text = "Generation: " + population.generationNumber.ToString();
        }

        // Evolve sooner (secret)
        if (Input.GetKeyDown(KeyCode.G))
        {
            population.SavePopulationStats(population.treesInPop, false);
        }

        // reset the population picks if needed
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetUI();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawCurrentPopulation();
        }

        // update metrics of a population folder
        if (Input.GetKeyDown(KeyCode.F))
        {
            var folderName = EditorInputDialog.Show("Population marks recalculation", "Please enter the folder path: ", "");
            if (!string.IsNullOrEmpty(folderName)) LoadEvaluations.ReadPopulationDataFromFolder(folderName);
        }
        
        // pick 10 Best and display them
        if (Input.GetKeyDown(KeyCode.B))
        {
            var folderName = EditorInputDialog.Show("Population showcase", "Please enter the folder path: ", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                var bestLsystems = LoadEvaluations.DisplayBestTrees(folderName);
                if (!bestLsystems.Equals(null))
                    DrawPopulationFromLsystems(bestLsystems);
                else
                    Debug.LogError("no lsystems loaded");
            }
            else
            {
                Debug.LogError("wrong folder path");
            }
        }

    }

    #endregion

    public void IteratePopulation()
    {
        foreach (var tree in population.treesInPop)
        {
            tree.GenerateAllIterations();
        }
    }

    private void DrawPopulationFromLsystems(List<string> lsystems)
    {
        List<SimpleTree> newTrees = new();

        for (int i = 0; i < lsystems.Count; i++)
        {
            // create a new child
            SimpleTree newTree = SpawnTree(i);
            newTree.lsystem.Sentence = lsystems[i];
            // assign the new lsystem
            AssignNewLsystem(newTree,
                newTree.lsystem,
                spawnPoints[i].transform.position);
            newTrees.Add(newTree);
        }
        if (newTrees.Count > 0)
            SpawnAndDisplayNewPopulation(population, newTrees);
        
    }

    /// <summary>
    /// Redraws all the trees in the given generation and resets it
    /// </summary>
    private void DrawCurrentPopulation()
    {
        foreach (var tree in population.treesInPop)
        {
            tree.DrawFullTree();
        }
    }

    /// <summary>
    /// Spawns a whole population of default trees
    /// </summary>
    private void SpawnPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            var newBasicTree = SpawnTree(i);
            population.AddTreeToPopulation(newBasicTree, i, spawnPoints[i].position, false);
        }
    }


    /// <summary>
    /// Spawns a whole population using lsystems from the list, 
    /// making sure there are no trees from the previous one
    /// </summary>
    /// <param name="treesToSpawn">Lsystems to assign in the new generation</param>
    private void SpawnPopulation(List<SimpleTree> treesToSpawn, Population pop)
    {
        Debug.Log("spawning pop gen number: " + pop.generationNumber + 
            ", mew tree count: " + treesToSpawn.Count );
        for (int i = 0; i < populationSize; i++)
        {
            try
            {
                // Spawn the new tree
                var newTree = SpawnTree(i, treesToSpawn[i].lsystem);

                // Add the new tree to the given population
                pop.AddTreeToPopulation(newTree, i, spawnPoints[i].position, treesToSpawn[i].lsystem.Iterated);
            
            }

            catch (System.Exception)
            {
                Debug.LogError("Failed to spawn a  tree, index: " + i + 
                    ", treeToSpawn count: " + treesToSpawn.Count + 
                    ", population trees count: " + pop.treesInPop.Count);
                throw;
            }
        }
    }


    /// <summary>
    /// Spawns a basic tree from one of the basic types
    /// </summary>
    /// <param name="treeIndex"></param>
    private SimpleTree SpawnTree(int treeIndex)
    {
        var newBasicTree = Instantiate(treeObject, spawnPoints[treeIndex])
            .transform.GetComponent<SimpleTree>();

        return newBasicTree;
    }

    /// <summary>
    /// Spawn an evolved tree and add it to the current population
    /// </summary>
    private SimpleTree SpawnTree(int treeIndex, Lsystem treeToAssign)
    {

        // Instantiate a new tree, set up it's lsystem
        var newTree = Instantiate(treeObject, spawnPoints[treeIndex])
            .transform.GetComponent<SimpleTree>();

        AssignNewLsystem(newTree, treeToAssign, spawnPoints[treeIndex].transform.position);

        return newTree;
    }

    /// <summary>
    /// Assigns a new lsystem to an Instantiated tree
    /// </summary>
    /// <param name="newTree"></param>
    /// <param name="LsystemToAssign"></param>
    /// <param name="resetPosition"></param>
    private void AssignNewLsystem(SimpleTree newTree, Lsystem LsystemToAssign, Vector3 resetPosition)
    {
        // Assign the new lsystem to the new tree
        newTree.lsystem = LsystemToAssign;

        // Reset turtle to the new spawn position, pass on the new DrawSegment delegate
        newTree.lsystem.ResetTurtle(resetPosition, newTree.DrawSegment);
        // Make sure the new lsystem has it's parent's iterations set as their current ones
        newTree.lsystem.CurrentIterations = LsystemToAssign.iterations;
    }


    /// <summary>
    /// Perform a full reproduction cycle on a population
    /// </summary>
    /// <param name="previousPopulation"></param>
    void EvolveOneGeneration(Population previousPopulation)
    {
        // Pick parents - population size - SELECTION
        List<SimpleTree> parents = Selection();

        // Save the current population data, save all branch positions if its the first generation
        if (population.generationNumber.Equals(0))
            population.SavePopulationStats(population.treesInPop, true);
        else
            population.SavePopulationStats(population.treesInPop, false);

        ////// REPRODUCTION //////////////////////////////////////////////////////////////////

        // CROSSOVER
        List<SimpleTree> children = ReproductionCrossover(parents);

        // MUTATION
        ReproductionMutation(children);

        SpawnAndDisplayNewPopulation(previousPopulation, children);
    }

    private void SpawnAndDisplayNewPopulation(Population previousPopulation, List<SimpleTree> children)
    {
        // Create a new population with the new children list
        Population newPopulation = new(previousPopulation, children, spawnPoints);

        Debug.Log("Children for next generation: ");
        foreach (var child in children)
        {
            Debug.Log(child.instanceID);
        }

        // Spawn the prepared new population
        SpawnPopulation(children, newPopulation);
        ResetUI();

        // Clear previous population list
        ClearPopulation(previousPopulation);

        population = newPopulation;
        population.LogAPopulation();

        // Draw the new generation
        DrawCurrentPopulation();
    }

    #region Evolution methods


    /// <summary>
    /// Based on normalized weights, select parents for next generation
    /// </summary>
    private List<SimpleTree> Selection()
    {
        List<SimpleTree> parents = new();


        // Find best weighted tree and add it to parents
        List<SimpleTree> sortedTrees = population.treesInPop.OrderBy(t => t.weight).ToList();
        parents.Add(sortedTrees[0]);

        // Normalize the weights so they sum up to 1.0
        population.NormalizeWeights();

        // The rest of parents is picked at random
        for (int i = 0; i < populationSize - 1; i++)
        {
            parents.Add(population.PickRandomTreeWeighted());
        }

        // Sets the default weight for all the parents
        foreach (var parentTree in parents)
        {
            parentTree.weight = defaultWeight;
        }

        return parents;
    }

    /// <summary>
    /// Goes through all the parents and produces children based on the set population size
    /// </summary>
    /// <param name="parents"></param>
    /// <returns></returns>
    private List<SimpleTree> ReproductionCrossover(List<SimpleTree> parents)
    {
        List<SimpleTree> children = new();

        for (int i = 0; i < populationSize; i++)
        {
            // pick two random parents
            SimpleTree parentA = parents[Random.Range(0, parents.Count - 1)];
            SimpleTree parentB = parents[Random.Range(0, parents.Count - 1)];


            //parentA.LogLsystem();
            // crossover
            MidPointCrossover crossover = new(parentA.lsystem);

            // create a new child
            SimpleTree child = SpawnTree(i);

            // assign the new lsystem and add it to the overall list
            AssignNewLsystem(child,
                crossover.PerformOperator(child.lsystem, parentB.lsystem),
                spawnPoints[i].transform.position);
            children.Add(child);
        }

        return children;
    }

    /// <summary>
    /// Given a mutation, mutate all the trees in a List
    /// </summary>
    /// <param name="mutation"></param>
    private void ReproductionMutation(List<SimpleTree> children)
    {
        foreach (var genome in children)
        {
            genome.lsystem = mutation.PerformOperator(genome.lsystem);
        }
    }

    #endregion

    #region Helper & Debug methods

    /// <summary>
    /// Helper function that checks spawns
    /// </summary>
    private void CheckAllSpawns()
    {
        for (int i = 0; i < populationSize; i++)
        {
            CheckSpawn(i);
        }
    }

    /// <summary>
    /// Reset UI trees's colours and pick status
    /// </summary>
    public void ResetUI()
    {
        foreach (var UItree in UItrees)
        {
            //unselecting all trees
            if (UItree.isPicked)
                UItree.ResetTreePick();
        }

        population.resetPickCount();
    }

    public void ClearPopulation(Population pop)
    {
        //clears out all the trees in the population
        foreach (var tree in pop.treesInPop)
        {
            tree.EraseTree();
            Destroy(tree.gameObject);
        }

        //resets the population
        ResetUI();

    }

    // Compare spawned trees and trees from the population's list
    private void CheckSpawn(int index)
    {
        try
        {
            Debug.Log("currentPopulation" + population);
            Debug.Log("currentPop gen: " + population.generationNumber);
            var SpawnTree = spawnPoints[index].GetComponentInChildren<SimpleTree>();
            Debug.Log("SpawnTree: " + SpawnTree);
            var ListTree = population.treesInPop[index];
            Debug.Log("ListTree: " + ListTree);

            Debug.Log("Index: " + index + 
                ", tree at spawn: " + SpawnTree.instanceID + 
                ", Gen: " + SpawnTree.generationNumber +
                ", tree from list: " + ListTree.instanceID + 
                ", gen: " + ListTree.generationNumber + 
                ", branch count: " + SpawnTree.BranchCount);
        }
        catch (System.ArgumentOutOfRangeException)
        {
            Debug.LogError("index failed: " + index);
        }
        catch (System.Exception)
        {
            Debug.LogError("index failed: " + index + 
                "Spawntree: " + spawnPoints[index].GetComponentInChildren<SimpleTree>().instanceID + 
                ", ListTree: " + population.treesInPop[index]);

            throw;
        }
    }
    #endregion
}

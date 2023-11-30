using System;
using System.Collections.Generic;
using UnityEngine;
using static Turtle;

/// <summary>
/// A MonoBehaviour class that gets initialized when a new tree is generated, has info on current branches and all the lsystem parameters that are available
/// </summary>
[Serializable]
public class SimpleTree : MonoBehaviour
{

    // with working line renderer in branch, we can do various colours / shapes, if i can get some presets for branches / leaves
    public GameObject Branch;

    [SerializeField] public List<Tuple<Vector2, Vector2>> currentBranches = new();
    private float angle = 15f;
    [SerializeField]
    private float branchLength = 2f;

    //private float branchWidth = 0.5f;

    public Color branchColour;

    [SerializeField]
    public GameObject branchesStart;

    private string axiom;

    [HideInInspector]
    public char[] alphabet;

    public Lsystem lsystem;
    public enum TreeType
    {
        set1,
        set2,
        set3,
        set4
    }

    [SerializeField]
    private TreeType selectedType;

    private Dictionary<char, string> ruleset = new();

    // weight given by the user selection
    public double weight;

    // helper for branch counts
    public int BranchCount;

    public string instanceID;
    // available commands of the drawing turtle
    private Dictionary<string, Action<Turtle>> commands;

    public int generationNumber;
    // Whether or not this tree has been initialized
    private bool init = false;

    

    /// <summary>
    /// Returns a random tree type
    /// source: https://www.reddit.com/r/Unity3D/comments/ax1tqf/unity_tip_random_item_from_enum/
    /// </summary>
    /// <typeparam name="TreeType"></typeparam>
    /// <param name="_enum_"></param>
    /// <returns></returns>
    public static TreeType RandomTreeType()
    {
        var enumValues = Enum.GetValues(enumType: typeof(TreeType));
        return (TreeType)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
    }


    // Assigns a new weight to this tree from its generation
    internal void UpdateWeight(bool selected, Population gen)
    {
        Debug.Log(gen);
        weight = gen.SelectedTreeWeight(selected);
        // Add normalizing weights call here
        Debug.Log("new weight for: " + this.transform.name + " new weight: " + weight);
    }

    /// <summary>
    /// Draw one tree branch / segment
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void DrawSegment(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.black, 100f);
        try
        {
            // Instantiate a new branch in a given position
            GameObject treeSegment = Instantiate(Branch, start, branchesStart.transform.rotation, branchesStart.transform);
           
            // Get the Line Renderer from the branch
            LineRenderer lr = treeSegment.GetComponent<LineRenderer>();
            lr.material.SetColor("_Color", branchColour);
            treeSegment.name = new string(instanceID + "_branch" + BranchCount.ToString());
            BranchCount++;

            Vector3 branchDataStart = start - branchesStart.transform.position;
            Vector3 branchDataEnd = end - branchesStart.transform.position;
            
            // Create new branchData with transform info
            Tuple<Vector2, Vector2> newBranchdata = new(new Vector2(branchDataStart.z, branchDataEnd.y),new Vector2(branchDataEnd.z, branchDataEnd.y));

            currentBranches.Add(newBranchdata);

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        catch (Exception)
        {
            Debug.LogWarning("init" + init);
            Debug.LogWarning("Branch: " + Branch);
            Debug.LogWarning("start: " + start);
            Debug.LogWarning(": " + branchesStart);
            throw;
        }


    }

    // Gets called after instantiate, builds a tree based on the type provided 
    public void TreeSetup()
    {
        if (transform.childCount > 0)
            branchesStart = transform.GetChild(0).gameObject;
        else
            Debug.LogError("No brancheStart assigned, there is no child objects of " + instanceID);
        // Don't do anything if this tree is initialized or iterated already
        if (init)
        {

            return;
        }

        if (!selectedType.Equals(null))
        {
            selectedType = RandomTreeType();
            Debug.Log("Running tree setup, new tree type: " + selectedType);

        }


        switch (selectedType)
        {
            
            case TreeType.set1:

                axiom = "X";

                angle = 35f;

                break;

            case TreeType.set2:

                axiom = "G";

                angle = 25f;

                break;

            case TreeType.set3:

                axiom = "G";

                angle = 20f;

                break;

            case TreeType.set4:

                axiom = "Y";

                angle = 22.5f;

                break;
        }

        // add all possible rules
        ruleset = new Dictionary<char, string>
        {
            {'F', "FF" },
            {'X', "F[+X][-X]FX"},
            {'G', "G[+G]G[-G]G"},
            {'H', "H[+H]H[-H]H" },
            {'Y', "HH" }
        };
        // gets all the leters from the ruleset
        alphabet = GetAlphabet(ruleset);

        branchColour = UnityEngine.Random.ColorHSV();
        // tree initialized
        init = true;

    }

    /// <summary>
    /// Get all the characters in the rules, making an alphabet of the Tree
    /// </summary>
    /// <param name="rules"></param>
    /// <returns></returns>
    char[] GetAlphabet(Dictionary<char, string> rules)
    {
        if (alphabet.Length != 0)
            return alphabet;

        List<char> allLetters = new()
        {
            //adding basic letters that all trees will have
            '+',
            '-',
            '[',
            ']',
            'F'
        };

        foreach (var rule in rules)
        {
            //if the char is not in the list of letters, add it
            if (!allLetters.Contains(rule.Key))
            {
                allLetters.Add(rule.Key);
            }
        }

        return allLetters.ToArray();
    }

    void Awake()
    {
        TreeSetup();
        instanceID = transform.parent.GetInstanceID().ToString();
        transform.name = "Tree " + instanceID;
        branchesStart.name = "branches-Tree" + instanceID;
        commands = new Dictionary<string, Action<Turtle>>
         {
             {"F", turtle => turtle.Translate(Vector3.up * branchLength)},
             {"G", turtle => turtle.Translate(Vector3.up * branchLength)},
             {"H", turtle => turtle.Translate(Vector3.up * branchLength)},
             {"+", turtle => turtle.Rotate(new Vector3(angle, 0, 0))},
             {"-", turtle => turtle.Rotate(new Vector3(-angle, 0, 0))},
             {"[", turtle => turtle.Push()},
             {"]", turtle => turtle.Pop()},
         };

        // Create a new Lsystem with the selected axiom, alphabet, position, rules and Draw function
        lsystem = new Lsystem(axiom, 
            alphabet, 
            ruleset, 
            commands, 
            transform.position, 
            DrawSegment);

    }

    /// <summary>
    /// Function that makes sure the tree has the exact number of iterations required
    /// </summary>
    public void GenerateAllIterations()
    {
        if (lsystem.Iterated)
            return;

        int i;
        for (i = lsystem.CurrentIterations; i < lsystem.iterations; i++)
        {
            // TODO: separate iterating and generation
            lsystem.Generate();
        }

        lsystem.CurrentIterations = i;
        lsystem.Iterated = true;
    }

    // generates a full grown tree and draws it 
    public void DrawFullTree()
    {
        // if the tree is not generated, iterate over it
        if (!lsystem.Iterated)
        {
            Debug.Log("drawing a new tree, generation: " + generationNumber);
            GenerateAllIterations();
        }

        lsystem.DrawSystem();

    }


    // get rid of all the 1es from the previous iteration
    public void EraseTree()
    {
        foreach (Transform child in branchesStart.transform)
        {
            Destroy(child.gameObject);
        }
        BranchCount = 0;
        currentBranches.Clear();
    }

    // Prints out the current sentence of the tree 
    public void LogLsystem()
    {
        try
        {
            Debug.Log("Logging tree: " + instanceID +
                " weight: " + weight +
                "alphabet length: " + alphabet.Length
                );
            Debug.Log(lsystem.Sentence);
            LogRules();

        }
        catch (Exception)
        {
            Debug.Log("sentence too long");
            throw;
        }

    }

    // Helper function that logs all the rules of the tree
    private void LogRules()
    {
        Debug.Log("rule Count: " + ruleset.Count);
        foreach (KeyValuePair<char, string> entry in ruleset)
        {
            Debug.Log(entry.Key + "->" + entry.Value);
        }
    }
}

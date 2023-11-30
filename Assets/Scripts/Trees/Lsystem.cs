using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static TreeMarking;

/// <summary>
/// Class that takes care of the turtle commands as well as drawing a given tree, has helper functions regarding brackets
/// <para> does not get instantiated itself </para>
/// </summary>
public partial class Lsystem
{
    // The whole grammar of the Lsystem
    public string sentence;
    private int extraLeftBracketCount;
    private int leftBracketCount;
    private int rightBracketCount;
    // Lsystem rules
    private Dictionary<char, string> rules;

    private char[] symbols;
    // Helper var for lsystem string
    private string currentString = string.Empty;

    // List of delegates for the turtle commands
    private Dictionary<string, Action<Turtle>> turtleCommands;

    // Delegate for turtle 
    private Turtle.TreeSegment BranchDraw;
    private Turtle turtle;

    public int iterations = 3;
    private int currentIterations = 0;
    public bool Iterated { get; set; } = false;
    // Struct to save all the fitness function results
    public MarkingResults fitnessResults;

    public string Sentence { get => sentence; set => sentence = value; }
    public string CurrentString { get => currentString;}
    public int CurrentIterations { get => currentIterations; set => currentIterations = value; }
    public char[] Symbols { get => symbols; set => symbols = value; }
    public int ExtraLeftBracketCount { get => extraLeftBracketCount; set => extraLeftBracketCount = value; }

    // Default constructor
    public Lsystem(string axiom, char[] alphabet, Dictionary<char, string> ruleset, Dictionary<string, Action<Turtle>> turtleCommands
        , Vector3 initialPosition, Turtle.TreeSegment draw)
    {
        sentence = axiom;
        rules = ruleset;
        this.turtleCommands = turtleCommands;
        BranchDraw = draw;
        Symbols = alphabet;

        turtle = new Turtle(initialPosition, BranchDraw);

    }

    public Lsystem(Lsystem systemToCopy, Vector3 initialPosition)
    {
        sentence = systemToCopy.Sentence;
        rules = systemToCopy.rules;
        this.turtleCommands = systemToCopy.turtleCommands;
        BranchDraw =systemToCopy.BranchDraw;
        Symbols = systemToCopy.Symbols;

        turtle = new Turtle(initialPosition, BranchDraw);

    }

    /// <summary>
    /// Generate a new iteration of this tree
    /// </summary>
    /// <returns></returns>
    public string Generate()
    {
        sentence = IterateSentence(sentence);

        return sentence;
    }

    /// <summary>
    /// Construct the next itteration of the sentence - L system itterations
    /// </summary>
    /// <param name="oldSentence"></param>
    /// <returns></returns>
    private string IterateSentence(string oldSentence)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char c in oldSentence)
        {
            // if the character is a key in rules, add the value of the key, otherwise add the character itself
            stringBuilder.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());

        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Make sure there are no additional left brackets in the grammar, counts brackets as well
    /// </summary>
    private void FixExtraBrackets()
    {
        Tuple<int, int> extraBrackets =  CountBrackets(currentString);

        leftBracketCount = extraBrackets.Item1;
        rightBracketCount = extraBrackets.Item2;


        ExtraLeftBracketCount = leftBracketCount - rightBracketCount;
        // counting the extra left brackets of the new system
        if (ExtraLeftBracketCount > 0)
            Debug.Log("extra brackets before fixing: " + ExtraLeftBracketCount);

        // unity freezes without this if, check in loop not enough?
        if (ExtraLeftBracketCount < 0)
            return;

        // making sure all the brackets are closed
        for (int i = ExtraLeftBracketCount; i < 0; i--)
        {
            sentence += ']';
            ExtraLeftBracketCount--;
        }
    }

    /// <summary>
    /// Counts all the brackets in the given Lsystem
    /// </summary>
    /// <param name="lsystem"></param>
    /// <returns></returns>
    public static Tuple<int, int> CountBrackets(string lsystem)
    {
        int leftBracketCount = 0, rightBracketCount = 0;

        foreach (char c in lsystem)
        {
            if (c.Equals('['))
            {
                leftBracketCount++;

            }
            else if (c.Equals(']'))
            {
                rightBracketCount++;

            }
        }
        // return the counts as a tuple
        return new Tuple<int, int>(leftBracketCount, rightBracketCount);
    }

    /// <summary>
    /// Draw the tree, fixing brackets beforehand
    /// </summary>
    public void DrawSystem()
    {
        FixExtraBrackets();

        foreach (var instruction in sentence)
        {
            if (turtleCommands.TryGetValue(instruction.ToString(), out var command))
            {
                command(turtle);
            }
        }
    }

    /// <summary>
    /// Resets the turtle to the initial position, assigns a new draw delegate
    /// </summary>
    /// <param name="initPosititon"></param>
    /// <param name="draw"></param>
    internal void ResetTurtle(Vector3 initPosititon, Turtle.TreeSegment draw)
    {
        turtle = new Turtle(initPosititon, draw);
    }

    /// <summary>
    /// Combines rules and alphabet with another lsystem
    /// </summary>
    /// <param name="secondLsystem"></param>
    /// <returns></returns>
    public List<char> CombineLsystemSymbols(Lsystem secondLsystem)
    {
        List<char> newSymbols = new List<char>(Symbols);

        // if we find any symbols from the second system that this one doesnt have, add them
        foreach (var symbol in secondLsystem.Symbols)
        {
            if (!Symbols.Contains(symbol))
            {
                newSymbols.Add(symbol);
            }
        }

        return newSymbols;
    }

    /// <summary>
    /// Save the data on specific tree along with its marks
    /// </summary>
    /// <param name="treeWeight"></param>
    /// <param name="finalBranchCount"></param>
    /// <param name="marking"></param>
    /// <param name="branches"></param>
    /// <returns></returns>
    public LsystemData SaveData(double treeWeight, int finalBranchCount, MarkingResults marking, List<Tuple<Vector2, Vector2>> branches = null)
    {
        return new LsystemData(sentence, rules, Symbols, treeWeight,  finalBranchCount, branches ,marking);
    }
}


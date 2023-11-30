using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TreeMarking;

public partial class Lsystem
{
    [Serializable]
    public struct LsystemData
    {
        [JsonProperty("sentence")]
        public readonly string sentence;

        [JsonProperty("rules")]
        public readonly Dictionary<char, string> rules;

        [JsonProperty("alphabet")]
        public readonly List<char> alphabet;

        [JsonProperty("finalWeight")]
        public readonly double finalWeight;

        [JsonProperty("finalBranchCount")]
        public readonly int finalBranchCount;

        [JsonProperty("branches")]
        public readonly List<Tuple<Vector2, Vector2>> branches;

        [JsonProperty("fitness")]
        public MarkingResults fitness;

        public LsystemData(string sentence, Dictionary<char, string> rules, char[] alphabet, double finalWeight, int finalBranchCount, List<Tuple<Vector2, Vector2>> branches, MarkingResults fitnessRes)
        {
            this.sentence = sentence;
            this.rules = rules;
            this.alphabet = alphabet.ToList();
            this.finalWeight = finalWeight;
            this.finalBranchCount = finalBranchCount;
            this.branches = branches;
            this.fitness = fitnessRes;
        }

        

        public void RecalculateData(string filename)
        {
            Debug.Log("tree recalc, file name: " + filename + ", tree name: " + fitness.treeName);
            Debug.Log("sentence length: " + sentence.Length);
            fitness = RemarkATree(fitness, branches, sentence);
            PrintResults();
        }



        public void PrintResults()
        {
            Debug.Log("Tree name: " + fitness.treeName
                + ", phototrop: " + fitness.positivePhototropism
                + ", branchingProportions: " + fitness.branchingPointsProportion
                + ", bilateral Symmetry: " + fitness.bilateralSymmetry
                + ", overall Fitness: " + fitness.overallFitness);
        }
    }
}


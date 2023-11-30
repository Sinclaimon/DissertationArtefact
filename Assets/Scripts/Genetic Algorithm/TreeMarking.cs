using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

//Mark each tree based on Niklas's realistic fitness function
public static class TreeMarking
{
    // Struct containing all the marks and their respective weight in the overall scoring
    public struct MarkingResults
    {
        // Original tree
        [JsonProperty("treeName")]
        public readonly string treeName;


        // How high the tree is, 100 weight
        [JsonProperty("positivePhototropism")]
        public float positivePhototropism;

        [JsonProperty("treeHeight")]
        public readonly float treeHeight;

        public static float phototropismWeight = 100;


        // Balance of the tree - left / right ratio fitness, between -1 and 1

        [JsonProperty("bilateralSymmetry")]
        public readonly float bilateralSymmetry;

        public static float symmetryWeight = 90;

        // Overall surface area of the ending segments
        public readonly float lightGathering;

        public static float lightGatheringWeight = 40;

        // Total number of branching points
        [JsonProperty("branchingPointsProportion")]
        public readonly float branchingPointsProportion;

        public static float branchPointsWeight = 30;

        [JsonProperty("overallFitness")]
        public float overallFitness;

        public MarkingResults(string treeName, float positivePhototrop, float bilateralSymmetry, float branchingPointsProportion)
        {
            this.treeName = treeName;
            this.positivePhototropism = positivePhototrop;
            this.bilateralSymmetry = bilateralSymmetry;
            //this.lightGathering = lightGathering;
            this.branchingPointsProportion = branchingPointsProportion;
            overallFitness = 0;
            lightGathering = 0;
            treeHeight = 0;
        }

        public static void PrintWeights()
        {
            Debug.Log("phototropism weight: " + phototropismWeight + ", symmetry weight: " + symmetryWeight + ", branching points weight: " + branchPointsWeight);
        }
    }


    /// <summary>
    /// Creates a marking sheet for a specific tree and returns it
    /// </summary>
    /// <param name="treeToMark"></param>
    /// <returns></returns>
    public static MarkingResults MarkATree(SimpleTree treeToMark)
    {
        MarkingResults results = new(treeToMark.instanceID,
            CalcTreePhototropism(treeToMark.currentBranches),
            CalcTreeBalance(treeToMark.currentBranches),
            CalcBranchingPoints(treeToMark.lsystem.CurrentString));

        results.overallFitness = CalcOverallFitness(results);
        return results;
    }

    /// <summary>
    /// Recalculates marks to include new overall fitness, branching points and phototropism calculations
    /// </summary>
    /// <param name="oldResults"></param>
    /// <param name="branches"></param>
    /// <param name="lsystem"></param>
    /// <returns>new MarkingResults</returns>
    public static MarkingResults RemarkATree(MarkingResults oldResults, List<Tuple<Vector2, Vector2>> branches, string lsystem )
    {
        MarkingResults recalcResults = new(oldResults.treeName,
            CalcPhototropismScore(oldResults.treeHeight),
            CalcTreeBalance(branches),
            CalcBranchingPoints(lsystem));
        recalcResults.overallFitness = CalcOverallFitness(recalcResults);
        
        return recalcResults;
    }


    #region Marking functions

    /// <summary>
    /// Assigns overall fitnesses to a whole population of marked trees
    /// </summary>
    /// <param name="marksPopulation"></param>
    public static float CalcOverallFitness(MarkingResults marksTree)
    {

        float weightsCombined = MarkingResults.phototropismWeight + MarkingResults.symmetryWeight + MarkingResults.branchPointsWeight;

        // use 1 - Abs of symmetry for symmetry, since symmetry fitness has right or left side leaning in it as well
        float symmetryFitness = 1 - Mathf.Abs(marksTree.bilateralSymmetry);

        float overallFitness = marksTree.positivePhototropism * MarkingResults.phototropismWeight +
            symmetryFitness * MarkingResults.symmetryWeight +
            marksTree.branchingPointsProportion * MarkingResults.branchPointsWeight;

        // divide all fitness with combined weights value and return the result
        return overallFitness / weightsCombined;

    }

    /// <summary>
    /// go through all branches and find the one with the highest y - spawnLocation
    /// </summary>
    /// <param name="branches">all the branches of a said tree</param>
    /// <returns>phototropism fitness ("mark") </returns>
    public static float CalcTreePhototropism(List<Tuple<Vector2, Vector2>> branches)
    {
        // Starting height for the tree
        float highestY = branches[0].Item1.y;

        if (branches.Count.Equals(0))
            return 0;

        // Get the highest branch end (y axis) 
        foreach (var branch in branches)
        {
            if (branch.Item2.y > highestY)
            {
                highestY = branch.Item2.y;
            }
        }

        // Return the calculated score
        return CalcPhototropismScore(highestY);

    }

    // Formula for calculating phototropism marking
    private static float CalcPhototropismScore(float highestY)
    {
        return highestY / (highestY + 1);
    }

    /// <summary>
    /// calculates tree balance based on x coordinates of tree branches
    /// </summary>
    /// <param name="branches"></param>
    /// <returns></returns>
    public static float CalcTreeBalance(List<Tuple<Vector2, Vector2>> branches)
    {
        // Branch transforms are in local space so the centre will always be 0
        float branchCentre = 0;

        // Set sums of right and left to 0
        float right = 0;
        float left = 0;
         
        // If the tree has no branches, balance will be 0
        if (branches == null)
            return -1;
        // Add abs value to left or right variable based on the branch start and end position
        foreach (var branch in branches)
        {
            // Branch is on the right side of the tree
            if (branch.Item1.x < branchCentre || branch.Item2.x < branchCentre)
            {
                right += MathF.Abs(Vector3.Distance(branch.Item1, branch.Item2));
            }

            // Branch is on the left side of the tree
            if (branch.Item1.x > branchCentre || branch.Item2.x > branchCentre)
            {
                left += MathF.Abs(Vector3.Distance(branch.Item1, branch.Item2));
            }

        }

        // return the calculated balance score
        return BalanceRatioScore(left, right);
    }

    /// <summary>
    /// takes the left/right ratio of branches and calculates a fitness score based on it.
    /// <para>Fitness score is the ratio clamped between -1 and 1 </para> 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>a value between -1 and 1</returns>
    private static float BalanceRatioScore(float left, float right)
    {
        // Means the tree is unbalanced
        if (left.Equals(0) || right.Equals(0))
            return -1;

        float ratio = left / right;
        float fitness = Mathf.Clamp(ratio, -1, 1);

        return fitness;
    }

    /// <summary>
    /// find all [ to find branching points in given an lsystem string
    /// </summary>
    /// <param name="lsystem"></param>
    /// <returns>score based on the Branching points formula</returns>
    public static float CalcBranchingPoints(string lsystem)
    {
        Tuple<int, int> brackets = Lsystem.CountBrackets(lsystem);

        // left bracket count is the first item that gets counted in Lsystem helper function above
        int leftBracketCount = brackets.Item1;

        // we dont have info on the itterations, so we will set it to the default of 3
        return CalcBranchingPointsScore(leftBracketCount, 3);
    }

    // formula for calculating branching points score
    private static float CalcBranchingPointsScore(int segmentCount, int iterations)
    {
        return segmentCount / (segmentCount + Mathf.Pow(iterations, 3));
    }
    #endregion
}

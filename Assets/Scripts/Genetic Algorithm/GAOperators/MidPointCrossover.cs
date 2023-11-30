using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Random midpoint crossover
/// </summary>
public class MidPointCrossover : GAOperator
{
    private readonly Lsystem firstParent;
    public MidPointCrossover(Lsystem parentA)
    {
        firstParent = parentA;
    }

    public Lsystem PerformOperator(Lsystem child, Lsystem parentB)
    {
        // Create a child with all the rules and symbols of the parents
        child.Symbols = firstParent.CombineLsystemSymbols(parentB).ToArray();

        // Create an empty lsystem sentence var
        List<char> childSentence = new();

        // - 1 from the Length because Random.Range is inclusive
        int midPointA = Random.Range(0, firstParent.Sentence.Length - 1);
        int midPointB = Random.Range(0, parentB.Sentence.Length - 1);

        // Copying head from parent1
        for (int i = 0; i < midPointA; i++)
        {
            childSentence.Add(firstParent.Sentence[i]);
        }

        // Copying tail from parent2
        for (int i = midPointB; i < parentB.Sentence.Length; i++)
        {
            childSentence.Add(parentB.Sentence[i]);
        }


        child.Sentence = new string(childSentence.ToArray());

        return child;
    }

    public override Lsystem PerformOperator(Lsystem genotype)
    {
        throw new System.NotImplementedException();
    }
}

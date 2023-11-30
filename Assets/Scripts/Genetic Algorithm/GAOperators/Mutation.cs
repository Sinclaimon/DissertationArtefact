using System.Collections.Generic;
using UnityEngine;

public class Mutation : GAOperator
{
    private readonly float mRate;

    public Mutation(float mutationRate)
    {
        mRate = mutationRate;
    }

    /// <summary>
    /// Mutate given genome based on the mutationRate of this Mutation operator
    /// </summary>
    /// <param name="genotype">genome that will be mutated</param>
    /// <returns>new mutated genome in as string</returns>
    public override Lsystem PerformOperator(Lsystem genotype)
    {
        char[] mutadedGenotype = genotype.Sentence.ToCharArray();
        
        // Go through the whole sentence
        for (int i = 0; i < mutadedGenotype.Length; i++)
        {
            if (Random.Range(0f, 1f) < mRate)
            {
                // replace the given letter with a random letter from the alphabet, excluding brackets
                if (NotBracket(mutadedGenotype[i]))
                    mutadedGenotype[i] = getRandomLetter(genotype.Symbols, true);

            }
        }
        genotype.Sentence = new string(mutadedGenotype);

        return genotype;
    }

    /// <summary>
    /// Given an alphabet, return a random letter from that alphabet
    /// </summary>
    /// <param name="alphabet"></param>
    /// <returns>new random letter</returns>
    private char getRandomLetter(char[] alphabet, bool withOutBrackets)
    {
        if (withOutBrackets)
        {
            var newAlphabet = RemoveBracketsFromAlphabet(alphabet);
            return newAlphabet[Random.Range(0, newAlphabet.Length - 1)];
        }
        else
            return alphabet[Random.Range(0, alphabet.Length - 1)];
    }

    /// <summary>
    /// given an alphabet, remove all the brackets from it
    /// </summary>
    /// <param name="alphabet"></param>
    /// <returns></returns>
    private char[] RemoveBracketsFromAlphabet(char[] alphabet)
    {
        List<char> newSymbols = new();
        for (int i = 0; i < alphabet.Length; i++)
        {
            if (NotBracket(alphabet[i]))
                newSymbols.Add(alphabet[i]);
        }
        return newSymbols.ToArray();
    }

    /// <summary>
    /// Check if the given symbol is a letter - not a bracket
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private bool NotBracket(char symbol)
    {
        return !symbol.Equals('[') && !symbol.Equals(']');
    }

}

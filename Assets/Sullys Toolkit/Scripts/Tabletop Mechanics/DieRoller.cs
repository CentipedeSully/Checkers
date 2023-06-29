using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SullysToolkit.TableTop
{
    public static class DieRoller
    {
        //Static Commands
        public static int RollDie(int numberOfSides)
        {
            if (numberOfSides > 0)
                return Random.Range(1, numberOfSides + 1);
            else
            {
                Debug.LogWarning($"Warning: invalid die size {numberOfSides} provided to DieRoller. Returning 1");
                return 1;
            }
        }

        public static int RollDieWithModifier(int numberOfSides, int modifier)
        {
            return RollDie(numberOfSides) + modifier;
        }

        public static int RollManyDice(int numberOfSides, int numberOfDice)
        {
            int result = 0;
            for (int i = 0; i < numberOfDice; i++)
                result += RollDie(numberOfSides);
            return result;
        }

        public static int RollManyDice(int numberOfSides, int numberOfDice, out int[] rollResults)
        {
            int totalValue = 0;
            int result = 0;
            int[] resultsCollection = new int[numberOfDice];


            for (int i = 0; i < numberOfDice; i++)
            {
                result = RollDie(numberOfSides);
                resultsCollection[i] = result;
                totalValue += result;
            }

            rollResults = resultsCollection;   
            return totalValue;
        }
    }
}


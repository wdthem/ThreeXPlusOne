using ThreeXPlusOne.Code.Interfaces;

namespace ThreeXPlusOne.Code;

public class Algorithm : IAlgorithm
{
    /// <summary>
    /// Execute the the 3x+1 algorithm for all numbers either supplied by the user or generated randomly
    /// </summary>
    /// <param name="inputValues"></param>
    /// <returns></returns>
    public List<List<int>> Run(List<int> inputValues)
    {
        if (inputValues.Count == 0)
        {
            return [];
        }

        List<List<int>> returnValues = [];

        foreach (int value in inputValues)
        {
            if (value <= 0)
            {
                continue;
            }

            List<int> outputValues = [];

            int calculatedValue = value;

            //add the first number in the series
            outputValues.Add(calculatedValue);

            //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
            while (calculatedValue > 1)
            {
                //perform the two rules of the Collatz Conjecture
                if (calculatedValue % 2 == 0)
                {
                    calculatedValue /= 2;
                }
                else
                {
                    calculatedValue = (calculatedValue * 3) + 1;
                }

                outputValues.Add(calculatedValue);
            }

            returnValues.Add(outputValues);
        }

        return returnValues;
    }
}
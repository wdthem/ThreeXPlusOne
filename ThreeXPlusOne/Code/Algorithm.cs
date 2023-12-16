using ThreeXPlusOne.Code.Interfaces;

namespace ThreeXPlusOne.Code;

public class Algorithm : IAlgorithm
{
    public List<List<int>> Run(List<int> inputValues)
    {
        if (inputValues.Count == 0)
        {
            return new List<List<int>>();
        }

        List<List<int>> returnValues = new List<List<int>>();

        foreach (var value in inputValues)
        {
            if (value <= 0)
            {
                continue;
            }

            List<int> outputValues = new List<int>();

            int calculatedValue = value;

            //add the first number in the series
            outputValues.Add(calculatedValue);

            //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
            while (calculatedValue > 1)
            {
                if (calculatedValue % 2 != 0)
                {
                    calculatedValue = (calculatedValue * 3) + 1;
                }
                else
                {
                    calculatedValue /= 2;
                }

                outputValues.Add(calculatedValue);
            }

            returnValues.Add(outputValues);
        }

        return returnValues;
    }
}
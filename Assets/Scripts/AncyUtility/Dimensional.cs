using UnityEngine;

namespace AncyUtility
{
    public class Dimensional
    {
        public static T[,] Convert1DTo2D<T>(T[] _1dArray, int rowCount, int columnCount)
        {
            T[,] result = new T[rowCount,columnCount];
            
            int index = 0;
            for (int row = 0; row < rowCount; ++row)
            {
                for (int column = 0; column < columnCount; ++column)
                {
                    result[row, column] = _1dArray[index++];
                }
            }

            return result;
        }
        
        public static T[] Convert2Dto1D<T>(T[,] _2dArray)
        {
            int rowCount = _2dArray.GetLength(0);
            int columnCount = _2dArray.GetLength(1);
            
            T[] result = new T[rowCount * columnCount];
            int index = 0;
            for (int row=0; row<rowCount;++row)
            {
                for (int column = 0; column < columnCount; ++column)
                {
                    result[index++] = _2dArray[row, column];
                }
            }

            return result;
        }
        
        public static float GetMinimumValueInFloat2DArray(float[,] array)
        {
            // Initialize minimum value with the maximum possible float value
            float minValue = float.MaxValue;

            // Iterate through the array to find the minimum value
            foreach (float value in array)
            {
                if (value < minValue)
                {
                    minValue = value;
                }
            }

            return minValue;
        }
        
        public static Vector2Int GetMinimumIndexInFloat2DArray(float[,] array)
        {
            // Initialize minimum value with the maximum possible float value
            float minValue = float.MaxValue;
            var minIndex = new Vector2Int(0, 0);

            // Iterate through the array to find the minimum value
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i,j] < minValue)
                    {
                        minValue = array[i, j];
                        minIndex = new Vector2Int(i, j);
                    }
                }
            }

            return minIndex;
        }
        
        public static float GetMaximumValueInFloat2DArray(float[,] array)
        {
            // Initialize maximum value with the minimum possible float value
            float maxValue = float.MinValue;

            // Iterate through the array to find the maximum value
            foreach (float value in array)
            {
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }

            return maxValue;
        }
        
        public static Vector2Int GetMaximumIndexInFloat2DArray(float[,] array)
        {
            // Initialize minimum value with the maximum possible float value
            float maxValue = float.MaxValue;
            var maxIndex = new Vector2Int(0, 0);

            // Iterate through the array to find the minimum value
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i,j] > maxValue)
                    {
                        maxValue = array[i, j];
                        maxIndex = new Vector2Int(i, j);
                    }
                }
            }

            return maxIndex;
        }
    }
}
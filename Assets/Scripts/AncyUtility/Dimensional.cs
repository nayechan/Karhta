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
    }
}
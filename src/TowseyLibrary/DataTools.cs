// <copyright file="DataTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics;

    public class DataTools
    {
        private static readonly string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";

        private static void Main()
        {
            LoggedConsole.WriteLine("TESTING METHODS IN CLASS DataTools");

            bool doit1 = false;
            if (doit1) // test Submatrix()
            {
                LoggedConsole.WriteLine("");
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = FileTools.ReadDoubles2Matrix(fName);

                // int rowCount = matrix.GetLength(0);//height
                // int colCount = matrix.GetLength(1);//width
                // LoggedConsole.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                writeMatrix(matrix);
                LoggedConsole.WriteLine("");
                int r1 = 2;
                int c1 = 3;
                int r2 = 4;
                int c2 = 6;
                LoggedConsole.WriteLine("RowTop=" + r1 + " ColumnLeft=" + c1 + " RowBottom=" + r2 + " ColumnRight=" + c2);
                LoggedConsole.WriteLine("Indices start at [0,0] in top left.");
                int smRows = r2 - r1 + 1;
                int smCols = c2 - c1 + 1;
                LoggedConsole.WriteLine("Submatrix has " + smRows + " rows and " + smCols + " columns");
                double[,] sub = Submatrix(matrix, r1, c1, r2, c2);
                writeMatrix(sub);
            }// end test ReadDoubles2Matrix(string fName)

            if (true) // test NormaliseMatrixValues(double[,] m, double normMin, double normMax)
            {
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                LoggedConsole.WriteLine("\n");
                writeMatrix(matrix);
                double normMin = -2.0;
                double normMax = 1.0;
                matrix = Normalise(matrix, normMin, normMax);
                LoggedConsole.WriteLine("\n\n");
                writeMatrix(matrix);
                matrix = Normalise(matrix, 0, 1);
                LoggedConsole.WriteLine("\n\n");
                writeMatrix(matrix);
            }// end test NormaliseMatrixValues(double[,] m, double normMin, double normMax)

            // COPY THIS TEST TEMPLATE
            if (false) // test Method(parameters)
            {
            }// end test Method(string fName)

            LoggedConsole.WriteLine("FINISHED"); // end
            LoggedConsole.WriteLine("FINISHED"); // end
        }// end of MAIN

        // ***************************************************************************
        // ***************************************************************************
        // ***************************************************************************
        // ***************************************************************************

        public static double AntiLogBase10(double value)
        {
            return Math.Exp(value * Math.Log(10));
        }

        public static double AntiLog(double value, double logBase)
        {
            return Math.Exp(value * Math.Log(logBase));
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return x != 0 && (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Returns a subarray of the passed array of any type.
        /// </summary>
        /// <param name="array">an array of any type</param>
        /// <param name="start">the first element of the subarray.</param>
        /// <param name="length">number of elements in subarray.</param>
        public static T[] Subarray<T>(T[] array, int start, int length)
        {
            int end = start + length - 1;
            if (end >= array.Length)
            {
                // LoggedConsole.WriteLine("WARNING! DataTools.Subarray(): subarray extends to far.");
                return null;
            }

            var sa = new T[length];

            for (int i = 0; i < length; i++)
            {
                sa[i] = array[start + i];
            }

            return sa;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="A"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int[] Subarray(int[] A, int start, int length)
        {
            int end = start + length - 1;
            if (end >= A.Length)
            {
                // LoggedConsole.WriteLine("WARNING! DataTools.Subarray(): subarray extends to far.");
                return null;
            }

            int[] sa = new int[length];

            for (int i = 0; i < length; i++)
            {
                sa[i] = A[start + i];
            }

            return sa;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="A"></param>
        /// <param name="start"></param>
        /// <param name="length">the length of the subarray to be returned </param>
        /// <returns></returns>
        public static double[] Subarray(double[] A, int start, int length)
        {
            int end = start + length - 1;
            if (end >= A.Length)
            {
                // LoggedConsole.WriteLine("WARNING! DataTools.Subarray(): subarray extends to far.");
                // return null;
                end = A.Length - 1;
                length = end - start + 1;
            }

            double[] sa = new double[length];

            for (int i = 0; i < length; i++)
            {
                sa[i] = A[start + i];
            }

            return sa;
        }

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// Assume that RowTop < RowBottom, ColumnLeft < ColumnRight.
        /// Row, column indices start at 0
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static double[,] Submatrix(double[,] M, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;

            double[,] sm = new double[subRowCount, subColCount];

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {
                    sm[i, j] = M[r1 + i, c1 + j];
                }
            }

            return sm;
        }

        /// <summary>
        /// returns a list of acoustic events defined by start, end and intensity score.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="threshold"></param>
        /// <returns>List of double[] containing three values</returns>
        public static List<double[]> SegmentArrayOnThreshold(double[] values, double threshold)
        {
            int count = values.Length;
            var events = new List<double[]>();
            bool isHit = false;
            int startFrame = 0;

            for (int i = 0; i < count; i++) // pass over all frames
            {
                if (isHit == false && values[i] > threshold) // start of an event
                {
                    isHit = true;
                    startFrame = i;
                }
                else // check for the end of an event
                    if (isHit == true && values[i] <= threshold) // this is end of an event, so initialise it
                    {
                        isHit = false;
                        var segment = new double[3];
                        segment[0] = startFrame;
                        segment[1] = i;  // endFrame

                        // obtain average value.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++)
                    {
                        av += values[n];
                    }

                        segment[2] = av / (i - startFrame + 1); // average value.
                        events.Add(segment);
                    }
            } // end of pass over all frames

            return events;
        }// end method SegmentArrayOnThreshold()

        public static List<double[]> RemoveNullElementsFromList(List<double[]> list)
        {
            var newList = new List<double[]>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    newList.Add(list[i]);
                }
            }

            return newList;
        }

        public static double[,] ConvertList2Matrix(List<double[]> list)
        {
            int rows = list.Count;
            int cols = list[0].Length; // assume all vectors in list are of same length
            double[,] op = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    op[i, j] = list[i][j];
                }
            }

            return op;
        }

        public static double[] ConvertStringArrayToDoubles(string[] list)
        {
            int length = list.Length;
            var op = new double[length];
            for (int i = 0; i < length; i++)
            {
                    op[i] = double.Parse(list[i]);
            }

            return op;
        }

        /// <summary>
        /// converts an array of arrays to a matrix
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[,] ConvertJaggedToMatrix(double[][] list)
        {
            int rows = list.Length;
            int cols = list[0].Length; // assume all vectors in list are of same length
            double[,] op = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    op[i, j] = list[i][j];
                }
            }

            return op;
        }

        /*
         * converts a matrix to a vector by concatenating columns.
         */
        public static byte[,] Array2Matrix(byte[] array, int width, int height)
        {
            byte[,] M = new byte[height, width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    M[row, col] = array[id++];
                }
            }

            return M;
        }

        /// <summary>
        /// Converts a matrix to a vector by concatenating columns.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] Matrix2Array(double[,] m)
        {
            int ht = m.GetLength(0);
            int width = m.GetLength(1);
            double[] v = new double[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {
                    v[id++] = m[row, col];
                }
            }

            return v;
        }

        // *
        // * converts a matrix to a vector by concatenating columns.
        public static int[] Matrix2Array(int[,] m)
        {
            int ht = m.GetLength(0);
            int width = m.GetLength(1);
            int[] v = new int[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {
                    v[id++] = m[row, col];
                }
            }

            return v;
        }

/*
        /// <summary>
        /// Converts a matrix to a vector by concatenating columns.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static T[] Matrix2Array(T[,] m)
        {
            int ht = m.GetLength(0);
            int width = m.GetLength(1);
            T[] v = new T[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {
                    v[id++] = m[row, col];
                }
            }
            return v;
        }

*/

        public static byte[] Matrix2Array(byte[,] M)
        {
            int ht = M.GetLength(0);
            int width = M.GetLength(1);
            byte[] v = new byte[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {
                    v[id++] = M[row, col];
                }
            }

            return v;
        }

        /*
         * converts a 3-D matrix to a vector by concatenating columns and columns.
         */
        public static double[] Matrix2Array(double[,,] M)
        {
            int d1 = M.GetLength(0);
            int d2 = M.GetLength(1);
            int d3 = M.GetLength(2);
            double[] v = new double[d1 * d2 * d3];

            int id = 0;
            for (int row = 0; row < d1; row++)
            {
                for (int col1 = 0; col1 < d2; col1++)
                {
                    for (int col2 = 0; col2 < d2; col2++)
                    {
                        v[id++] = M[row, col1, col2];
                    }
                }
            }

            return v;
        }

        /*
         * converts a 3-D matrix to a vector by concatenating columns and columns.
         */
        public static int[] Matrix2Array(int[,,] M)
        {
            int d1 = M.GetLength(0);
            int d2 = M.GetLength(1);
            int d3 = M.GetLength(2);
            int[] v = new int[d1 * d2 * d3];

            int id = 0;
            for (int row = 0; row < d1; row++)
            {
                for (int col1 = 0; col1 < d2; col1++)
                {
                    for (int col2 = 0; col2 < d3; col2++)
                    {
                        v[id++] = M[row, col1, col2];
                    }
                }
            }

            return v;
        }

        /*
         * converts a matrix of doubles to binary using passed threshold
         */
        public static double[,] Matrix2Binary(double[,] M, double threshold)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > threshold)
                    {
                        op[r, c] = 1.0;
                    }
                }
            }

            return op;
        }

        public static double[,] TwoOfThree(double[,] m1, double[,] m2, double[,] m3)
        {
            int rows = m1.GetLength(0); // assume all matrices have same dimensions
            int cols = m2.GetLength(1);
            double[,] M = new double[rows, cols];

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    int count = 0;
                    if (m1[row, col] == 1.0)
                    {
                        count++;
                    }

                    if (m2[row, col] == 1.0)
                    {
                        count++;
                    }

                    if (m3[row, col] == 1.0)
                    {
                        count++;
                    }

                    if (count >= 2)
                    {
                        M[row, col] = 1;
                    }
                }
            }

            return M;
        }

        /// <summary>
        /// reduces length of the passed vector by combining consecutive values into an average
        /// </summary>
        /// <param name="v">the vector</param>
        /// <param name="factor">reduction factor</param>
        /// <returns>the reduced vector</returns>
        public static double[] VectorReduceLength(double[] v, int factor)
        {
            int newLength = v.Length / factor;
            double[] newVector = new double[newLength];

            for (int i = 0; i < newLength; i++)
            {
                int index = i * factor;
                double sum = 0.0;
                for (int j = 0; j < factor; j++)
                {
                    sum += v[index + j];
                }

                newVector[i] = sum / factor;
            }

            return newVector;
        }

        /// <summary>
        /// reduces length of the passed vector by combining consecutive values into an average
        /// </summary>
        /// <param name="v">the vector</param>
        /// <returns>the reduced vector</returns>
        public static double[] VectorDoubleLengthByAverageInterpolation(double[] v)
        {
            int newLength = v.Length * 2;
            double[] newVector = new double[newLength];

            for (int i = 0; i < newLength - 1; i++)
            {
                int originalIndex = i / 2;
                if (i.IsEven())
                {
                    newVector[i] = v[originalIndex];
                }
                else
                {
                    // index is odd
                    newVector[i] = (v[originalIndex] + v[originalIndex + 1]) / 2;
                }
            }

            newVector[newLength - 1] = v[v.Length - 1];
            return newVector;
        }

        /// <summary>
        /// returns the euclidian length of vector
        /// </summary>
        public static double VectorEuclidianLength(double[] v)
        {
            int length = v.Length;

            double d = 0.0;

            for (int i = 0; i < length; i++)
            {
                d += v[i] * v[i];
            }

            return Math.Sqrt(d);
        }

        public static double[] Vector_NormLength(double[] v)
        {
            double euclidLength = VectorEuclidianLength(v);
            double[] vnorm = new double[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                vnorm[i] = v[i] / euclidLength;
            }

            // now that length = 1.0;
            double length = VectorEuclidianLength(vnorm);
            if (length > 1.00001)
            {
                LoggedConsole.WriteLine("WARNING:DataUtilities.Vector_NormLength() LENGTH=" + length);
            }

            if (length < 0.99999)
            {
                LoggedConsole.WriteLine("WARNING:DataUtilities.Vector_NormLength() LENGTH=" + length);
            }

            return vnorm;
        }

        /// <summary>
        /// returns the vector normalised for min and max values
        /// </summary>
        public static double[] Vector_NormRange(double[] v)
        {
            MinMax(v, out var min, out var max);
            double range = max - min;

            double[] Vnorm = new double[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                Vnorm[i] = (v[i] - min) / range;
            }

            return Vnorm;
        }

        /// <summary>
        /// subtracts the mean from each value of an array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] SubtractMean(double[] v)
        {
            double mean = 0.0;
            for (int i = 0; i < v.Length; i++)
            {
                mean += v[i];
            }

            mean /= v.Length;
            double[] vOut = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vOut[i] = v[i] - mean;
            }

            return vOut;
        }

        /// <summary>
        /// subtracts the value from each value of an array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] SubtractValue(double[] v, double value)
        {
            double[] vOut = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vOut[i] = v[i] - value;
            }

            return vOut;
        }

        /// <summary>
        /// subtracts the value from each value of an array
        /// If below zero, truncate to zero.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] SubtractValueAndTruncateToZero(double[] v, double value)
        {
            double[] vOut = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vOut[i] = v[i] - value;
                if (vOut[i] < 0.0)
                {
                    vOut[i] = 0.0;
                }
            }

            return vOut;
        }

        /// <summary>
        /// return median of an array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double GetMedian(double[] v)
        {
            Array.Sort(v);
            int halfway = v.Length / 2;

            return v[halfway];
        }

        /// <summary>
        /// counts the zero crossings in a signal
        ///
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static int ZeroCrossings(double[] signal)
        {
            int length = signal.Length;
            int count = 0;
            for (int j = 1; j < length; j++) // foreach sample
            {
                count += Math.Abs(Math.Sign(signal[j]) - Math.Sign(signal[j - 1]));
            }

            return count / 2;
        }

        /// <summary>
        /// counts the times signal drops from above to below zero
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static int ZeroDippings(double[] signal)
        {
            int length = signal.Length;
            int count = 0;
            for (int j = 1; j < length; j++) // foreach sample
            {
                if (signal[j - 1] >= 0.0 && signal[j] < 0.0)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// counts the times signal rises from below to above zero
        /// </summary>
        public static int ZeroRisings(double[] signal)
        {
            int length = signal.Length;
            int count = 0;
            for (int j = 1; j < length; j++)
            {
                if (signal[j - 1] <= 0.0 && signal[j] > 0.0)
                {
                    count++;
                }
            }

            return count;
        }

// =============================================================================

        public static double[] reverseArray(double[] V)
        {
            int L = V.Length;
            double[] newArray = new double[L];

            for (int i = 0; i < L; i++)
            {
                newArray[i] = V[L - i - 1];
            }

            return newArray;
        }

        public static int[] reverseArray(int[] V)
        {
            int L = V.Length;
            int[] newArray = new int[L];

            for (int i = 0; i < L; i++)
            {
                newArray[i] = V[L - i - 1];
            }

            return newArray;
        }

        /// <summary>
        /// bounds a sequence of numbers between a minimum and a maximum.
        /// Numbers that fall outside the bound are truncated to the bound.
        /// </summary>
        /// <param name="A">array to be bound</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        public static double[] BoundArray(double[] A, double min, double max)
        {
            int L = A.Length;
            double[] newArray = new double[L];

            for (int i = 0; i < L; i++)
            {
                if (A[i] < min)
                {
                    newArray[i] = min;
                }
                else if (A[i] > max)
                {
                    newArray[i] = max;
                }
                else
                {
                    newArray[i] = A[i];
                }
            }

            return newArray;
        }

        // ###############################################################################################################
        // METHODS TO GET THE TYPE OF AN ARRAY OF STRING - used by CSVTools when reading in CSV file to a DataTable.
        public static Type[] GetArrayTypes(List<string[]> listOfArrays)
        {
            Type[] types = new Type[listOfArrays.Count];

            for (int i = 0; i < listOfArrays.Count; i++)
            {
                types[i] = TypeOfArray(listOfArrays[i]);
            }

            return types;
        }

        public static Type TypeOfArray(string[] array)
        {
            if (IsArrayOfBoolean(array))
            {
                return typeof(bool);
            }

            if (IsArrayOfInt(array))
            {
                return typeof(int);
            }

            if (IsArrayOfDouble(array))
            {
                return typeof(double);
            }

            return typeof(string);
        }

        public static bool IsArrayOfInt(string[] array)
        {
            int result = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!int.TryParse(array[i], out result))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsArrayOfDouble(string[] array)
        {
            double result = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!double.TryParse(array[i], out result))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsArrayOfBoolean(string[] array)
        {
            bool result = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (!bool.TryParse(array[i], out result))
                {
                    return false;
                }
            }

            return true;
        }

        // ###############################################################################################################

        /// <summary>
        /// sorts an array of doubles in DESCENDING order i.e. max first.
        /// returns both the sorted array (Item2) and the array indices in rank order (Item1)
        /// </summary>
        public static System.Tuple<int[], double[]> SortArray(double[] array)
        {
            double[] clone = (double[])array.Clone();
            int[] rankOrder = new int[array.Length];
            double[] sort = new double[clone.Length];
            for (int i = 0; i < array.Length; i++)
            {
                int maxIndex = GetMaxIndex(clone);
                rankOrder[i] = maxIndex;
                sort[i] = clone[maxIndex];

                // if(i % 100==0)
                //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, maxIndex, array[maxIndex]);
                clone[maxIndex] = -double.MaxValue;
            }

            return Tuple.Create(rankOrder, sort);
        }

        /// <summary>
        /// sorts an array of doubles in ASCENDING order.
        /// Returns both the sorted array (Item2) and the original array indices in sort order (Item1)
        /// this is a totally crude sort algorithm but I could not find a MathsNet sort
        /// that also returned the sorted indices.
        /// </summary>
        public static System.Tuple<int[], double[]> SortArrayInAscendingOrder(double[] array)
        {
            double[] clone = (double[])array.Clone();
            int[] rankOrder = new int[array.Length];

            double[] sort = new double[clone.Length];

            for (int i = 0; i < array.Length; i++)
            {
                int minIndex = GetMinIndex(clone);
                rankOrder[i] = minIndex;
                sort[i] = clone[minIndex];

                // if(i % 100==0)
                //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, maxIndex, array[maxIndex]);
                clone[minIndex] = double.MaxValue;
            }

            return Tuple.Create(rankOrder, sort);
        }

        /// <summary>
        /// sorts a list of integers.
        /// returns both the sorted array (Item2) and the array indices in rank order (Item1)
        /// </summary>
        public static System.Tuple<int[], int[]> SortArray(List<int> array)
        {
            // Array.Sort(array);
            int[] rankOrder = new int[array.Count];
            int[] sort = new int[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                int maxIndex = GetMaxIndex(array);
                rankOrder[i] = maxIndex;
                sort[i] = array[maxIndex];

                // if(i % 100==0)
                //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, maxIndex, array[maxIndex]);
                array[maxIndex] = -int.MaxValue;
            }

            return Tuple.Create(rankOrder, sort);
        }

        public static System.Tuple<int[], int[]> SortArray(int[] a)
        {
            // Array.Sort(array);
            int[] array = (int[])a.Clone();
            int[] rankOrder = new int[array.Length];
            int[] sort = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                int maxIndex = GetMaxIndex(array);
                rankOrder[i] = maxIndex;
                sort[i] = array[maxIndex];

                // if(i % 100==0)
                //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, maxIndex, array[maxIndex]);
                array[maxIndex] = -int.MaxValue;
            }

            return Tuple.Create(rankOrder, sort);
        }

        /// <summary>
        /// bounds a matrix of numbers between a minimum and a maximum.
        /// Numbers that fall outside the bound are truncated to the bound.
        /// </summary>
        /// <param name="M">the matrix to be bound</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        public static double[,] BoundMatrix(double[,] M, double min, double max)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (M[i, j] <= min)
                    {
                        newM[i, j] = min;
                    }
                    else if (M[i, j] >= max)
                    {
                        newM[i, j] = max;
                    }
                    else
                    {
                        newM[i, j] = M[i, j];
                    }
                }
            }

            return newM;
        }

        /// <summary>
        /// returns the min and max percentile values of the values in passed matrix.
        /// Must have previously calculated the min and max values in the matrix.
        /// </summary>
        /// <param name="matrix">the matrix</param>
        /// <param name="min">min power value in matrix</param>
        /// <param name="max">max power value in matrix</param>
        /// <param name="minPercentile"></param>
        /// <param name="maxPercentile"></param>
        /// <param name="minCut">power value equivalent to minPercentile</param>
        /// <param name="maxCut">power value equivalent to maxPercentile</param>
        public static void PercentileCutoffs(double[,] matrix, double minPercentile, double maxPercentile, out double minCut, out double maxCut)
        {
            if (maxPercentile < minPercentile)
            {
                throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
            }

            if (minPercentile < 0.0)
            {
                minPercentile = 0.0;
            }

            if (maxPercentile > 1.0)
            {
                maxPercentile = 1.0;
            }

            // if (minPercentile < 0.0) throw new ArgumentException("minPercentile must be at least 0.0");
            // if (maxPercentile > 1.0) throw new ArgumentException("maxPercentile must be at most 1.0");
            MinMax(matrix, out var min, out var max);
            if (max <= min)
            {
                throw new ArgumentException("max=" + max + " must be > min=" + min);
            }

            minCut = min;
            maxCut = max;
            if (minPercentile == 0.0 && maxPercentile == 1.0)
            {
                return;
            }

            const int n = 1024;      // number of bins for histogram
            int[] bins = new int[n]; // histogram of power in sonogram
            int M = matrix.GetLength(0); // width
            int N = matrix.GetLength(1); // height
            double range = max - min;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                { // NormaliseMatrixValues power for given min and max
                    int k = (int)Math.Floor(n * (matrix[i, j] - min) / range); // NormaliseMatrixValues
                    if (k < 0)
                    {
                        k = 0;      // range check
                    }

                    if (k >= n)
                    {
                        k = n - 1;
                    }

                    bins[k]++;
                }
            }

            int minThres = (int)Math.Floor(minPercentile * M * N);
            minCut = min;
            for (int k = 0; k < n; k++)
            {
                minThres -= bins[k];
                if (minThres < 0.0)
                {
                    minCut = min + (k * range / n);
                    break;
                }
            }

            int maxThres = (int)Math.Ceiling((1.0 - maxPercentile) * M * N);
            maxCut = max;
            for (int k = n; k > 0; k--)
            {
                maxThres -= bins[k - 1];
                if (maxThres < 0.0)
                {
                    maxCut = min + (k * range / n);
                    break;
                }
            }
        }

        /// <summary>
        /// counts the positive values in an array. Called by Classifier.Scan(Sonogram s)
        /// </summary>
        public static int CountPositives(double[] values)
        {
            int count = 0;
            foreach (double d in values)
            {
                if (d > 0.0)
                {
                    count++;
                }
            }

            return count;
        }

        public static int CountPositives(int[] values)
        {
            int count = 0;
            foreach (int v in values)
            {
                if (v > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public static int CountTrues(bool[] peaks)
        {
            int count = 0;
            foreach (bool b in peaks)
            {
               if (b)
                    {
                        count++;
                    }
                }

            return count;
        }

        public static double[] Bool2Binary(bool[] boolArray)
        {
            int L = boolArray.Length;
            double[] binary = new double[L];
            for (int i = 0; i < L; i++) // iterate through boolArray
                {
                    if (boolArray[i])
                    {
                        binary[i] = 1.0;
                    }
                }

            return binary;
        }

        public static List<int> GetEventLengths(bool[] boolArray)
        {
            int L = boolArray.Length;
            List<int> list = new List<int>();
            bool inEvent = false;
            int counter = 0;

            for (int i = 0; i < L; i++) // iterate through boolArray
            {
                if (boolArray[i])
                {
                    counter++;
                    if (!inEvent)
                    {
                        inEvent = true;
                    }
                }
                else if (!boolArray[i] && inEvent)
                {
                    list.Add(counter);
                    inEvent = false;
                    counter = 0;
                }
            }

            if (boolArray[L - 1] && inEvent)
            {
                list.Add(counter);
            }

            return list;
        }

        // =============================================================================

        /// <summary>
        /// prunes the start and end of an array to remove low values.
        /// </summary>
        /// <param name="data">an array of double</param>
        /// <param name="threshold">must be in range [0,1].</param>
        public static int[] Peaks_CropLowAmplitude(double[] data, double threshold)
        {
            int length = data.Length;
            int[] bounds = new int[2];

            // double min, max;
            // DataTools.MinMax(data, out min, out max);
            // double range = max - min;
            // double threshold = min + (range * severity); //must be 10% of max.

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] >= threshold)
                {
                    bounds[0] = i;
                    break;
                }
            }

            for (int i = data.Length - 1; i > 1; i--)
            {
                if (data[i] >= threshold)
                {
                    bounds[1] = i;
                    break;
                }
            }

            return bounds;
        }

        /// <summary>
        /// Returns the location of the first and last peaks
        /// </summary>
        public static int[] Peaks_CropToFirstAndLast(double[] data, double severity)
        {
            int length = data.Length;
            int[] peaks = new int[2];
            MinMax(data, out var min, out var max);
            double range = max - min;
            double lowThreshold = min + (range * severity);      // must be 10% of max.
            double topThreshold = min + (range * (1 - severity)); // must be 90% of max.

            for (int i = 1; i < data.Length - 1; i++)
            {
                double Dm = data[i] - data[i - 1];
                double Dp = data[i + 1] - data[i];
                bool peak = false;
                if (Dm > 0.0 && Dp < 0.0)
                    {
                        peak = true;
                    }

                if (!peak || data[i] < lowThreshold)
                    {
                        continue;
                    }

                if (peak || data[i] > topThreshold) // which ever comes first
                {
                    peaks[0] = i;
                    break;
                }
            }

            for (int i = data.Length - 2; i > 1; i--)
            {
                double Dm = data[i] - data[i - 1];
                double Dp = data[i + 1] - data[i];
                bool peak = false;
                if (Dm > 0.0 && Dp < 0.0)
                    {
                        peak = true;
                    }

                if (!peak || data[i] < lowThreshold)
                    {
                        continue;
                    }

                if (peak || data[i] > topThreshold) // which ever comes first
                {
                    peaks[1] = i;
                    break;
                }
            }

            return peaks;
        }

        /// <summary>
        /// Finds the local peaks in a vector
        /// </summary>
        public static bool[] GetPeaks(double[] data)
        {
            int length = data.Length;
            bool[] peaks = new bool[length];
            for (int i = 1; i < data.Length - 1; i++)
            {
                double dm = data[i] - data[i - 1];
                double dp = data[i + 1] - data[i];
                if (dm > 0.0 && dp < 0.0)
                {
                    peaks[i] = true;
                }
            }

            return peaks;
        }

        /// <summary>
        /// Finds the local peaks in a vector whose value exceeds the passed threshold
        /// </summary>
        public static bool[] GetPeaks(double[] data, double threshold)
        {
            int length = data.Length;
            bool[] peaks = new bool[length];
            for (int i = 1; i < data.Length - 1; i++)
            {
                double dm = data[i] - data[i - 1];
                double dp = data[i + 1] - data[i];
                if (dm > 0.0 && dp < 0.0 && data[i] > threshold)
                {
                    peaks[i] = true;
                }
            }

            return peaks;
        }

        public static bool[] GetTroughs(double[] data)
        {
            int length = data.Length;
            bool[] troughs = new bool[length];
            for (int i = 1; i < data.Length - 1; i++)
            {
                double deltaMinus = data[i] - data[i - 1];
                double deltaPlus = data[i + 1] - data[i];
                if (deltaMinus < 0.0 && deltaPlus > 0.0)
                    {
                        troughs[i] = true;
                    }
                    else
                    {
                        troughs[i] = false;
                    }
                }

            return troughs;
        }

        public static int[] GetHistogramOfDistancesBetweenEveryPairOfPeaks(double[] data, double threshold)
    {
        List<int> peakLocations = PeakLocations(data, threshold);

        List<int> list = new List<int>();

        for (int i = 0; i < peakLocations.Count - 1; i++)
        {
            for (int j = i + 1; j < peakLocations.Count - 1; j++)
            {
                list.Add(peakLocations[j] - peakLocations[i]);
            }
        }

        int[] histo = Histogram.Histo(list.ToArray());
        return histo;
    }

        public static void CountPeaks(double[] array, out int count, out double sum)
    {
        count = 0;
        sum = 0.0;
        int L = array.Length;

        for (int i = 1; i < L - 1; i++) // iterate through array
            {
                if (array[i] > array[i - 1] && array[i] > array[i + 1])
            {
                count++;
                sum += array[i];
            }
            }
        }

        /// <summary>
        /// returns an array showing locaiton of peaks
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        public static void PeakLocations(double[] array, double threshold, out int count, out double[] locations)
    {
        count = 0;
        int L = array.Length;
        locations = new double[L];

        for (int i = 1; i < L - 1; i++) // iterate through array
        {
            if (array[i] < threshold)
                {
                    continue;
                }

            if (array[i] > array[i - 1] && array[i] > array[i + 1])
            {
                count++;
                locations[i] = 1.0;
            }
        }
    }

    /// <summary>
    /// returns a list containing integer index of every peak > threshold.
    /// </summary>
        public static List<int> PeakLocations(double[] array, double threshold)
    {
        int L = array.Length;
        var locations = new List<int>();

        for (int i = 1; i < L - 1; i++) // iterate through array
        {
            if (array[i] < threshold)
            {
                continue;
            }

            if (array[i] > array[i - 1] && array[i] > array[i + 1])
            {
                locations.Add(i);
            }
        }

        return locations;
    }

        /// <summary>
        /// returns an array showing values at the peaks.
        /// </summary>
        public static double[] GetPeakValues(double[] array)
        {
            var values = new double[array.Length];

            // iterate through array
            for (int i = 1; i < array.Length - 1; i++)
            {
                if (array[i] > array[i - 1] && array[i] > array[i + 1])
                {
                    values[i] = array[i];
                }
            }

            return values;
        }

    // returns the locations of peaks ranked from highest value to lowest valued peak
    public static int[] GetOrderedPeakLocations(double[] peakValues, int count)
    {
        var peakLocations = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int location = GetMaxIndex(peakValues);
            peakLocations.Add(location);
            peakValues[location] = 0.0;
        }

        return peakLocations.ToArray();
    }

    // =============================================================================

        /// <summary>
        /// returns a list of gaps between 1s in a binary array.
        /// </summary>
        public static List<int> GapLengths(double[] binaryArray)
        {
            var gaps = new List<int>();
            int prev = 0;

            // iterate through array
            for (int i = 1; i < binaryArray.Length; i++)
            {
                if (binaryArray[i] == 1.0 && prev > 0)
                {
                    gaps.Add(i - prev);
                    prev = i;
                }
                else if (binaryArray[i] == 1.0)
                {
                    prev = i;
                }
            }

            return gaps;
        }

        /// <summary>
        /// returns a list of gaps between the trues in a boolean array.
        /// </summary>
        public static List<int> GapLengths(bool[] binaryArray)
        {
            var gaps = new List<int>();
            int prev = 0;

            // iterate through array
            for (int i = 1; i < binaryArray.Length; i++)
            {
                if (binaryArray[i] && prev > 0)
                {
                    gaps.Add(i - prev);
                    prev = i;
                }
                else if (binaryArray[i])
                {
                    prev = i;
                }
            }

            return gaps;
        }

// =============================================================================
//public static double[] counts2RF(int[] counts)
//  {
//        int L = counts.Length;
//        double[] rf = new double[L];

//    // get the sum
//        int sum = 0;
//        for (int i = 0; i < L; i++) // iterate through array
//        {
//            sum += counts[i];
//        }

//        for (int i = 0; i < L; i++) // iterate through array
//        {
//            rf[i] = counts[i] / (double)sum;
//        }

//        // check total sums to 1.0;
//        double p = 0.0;
//        for (int i = 0; i < L; i++)
//        {
//            p += rf[i];
//        }

//        if (p > 1.00001)
//        {
//            LoggedConsole.WriteLine("WARNING:DataUtilities.counts2RF() total prob=" + p);
//        }

//        if (p < 0.99999)
//        {
//            LoggedConsole.WriteLine("WARNING:DataUtilities.counts2RF() total prob=" + p);
//        }

//        return rf;
//  }

  //      public static double[] values2RF(double[] values)
  //{
  //      int length = values.Length;
  //      double[] rf = new double[length];

  //  // get the sum
  //      double sum = 0;
  //      for (int i = 0; i < length; i++) // iterate through array
  //  {
  //      sum += values[i];
  //  }

  //      for (int i = 0; i < length; i++) // iterate through array
  //          {
  //              rf[i] = values[i] / sum;
  //          }

  //          // check total sums to 1.0;
  //      double p = 0.0;
  //      for (int i = 0; i < length; i++)
  //          {
  //              p += rf[i];
  //          }

  //      if (p > 1.00001)
  //          {
  //              LoggedConsole.WriteLine("WARNING:DataUtilities.counts2RF() total prob=" + p);
  //          }

  //      if (p < 0.99999)
  //          {
  //              LoggedConsole.WriteLine("WARNING:DataUtilities.counts2RF() total prob=" + p);
  //          }

  //      return rf;
  //}

// =============================================================================

        public static void WriteArrayList(List<string> list)
  {
      for (int i = 0; i < list.Count; i++)
            {
                LoggedConsole.WriteLine(i + "  " + list[i]);
            }
        }

        public static void WriteArray(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                LoggedConsole.WriteLine(i + "  " + array[i]);
            }
        }

        public static void writeArray(int[] array)
  {
      for (int i = 0; i < array.Length; i++)
            {
                LoggedConsole.WriteLine(i + "  " + array[i]);
            }
        }

        public static string Array2String(int[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i] + ", ");
            }

            return sb.ToString();
          }

        public static string Array2String(string[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i] + ", ");
            }

            return sb.ToString();
        }

        public static void WriteArray(double[] array)
        {
            string format = "F3";
            writeArray(array, format);
        }

        public static void writeArray(double[] array, string format)
  {
      for (int i = 0; i < array.Length; i++)
            {
                LoggedConsole.WriteLine(i + "  " + array[i].ToString(format));
            }
        }

        public static void writeArray(bool[] array)
  {
      for (int i = 0; i < array.Length; i++)
            {
                LoggedConsole.WriteLine(i + "  " + array[i]);
            }
        }

        public static string WriteArrayAsCsvLine(double[] array, string format)
    {
        var sb = new StringBuilder(array[0].ToString(format));
        int count = array.Length; // dimension
        for (int i = 1; i < count; i++)
        {
            sb.Append("," + array[i].ToString(format));
        }

        return sb.ToString();
    }

        public static void WriteArrayInLine(double[] array, string format)
  {
      int count = array.Length; // dimension
      for (int i = 0; i < count; i++)
      {
          LoggedConsole.Write(" " + array[i].ToString(format));
      }

      LoggedConsole.WriteLine();
  }

        public static void writeMatrix(double[,] matrix, string format)
  {
      int rowCount = matrix.GetLength(0); // height
      int colCount = matrix.GetLength(1); // width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              LoggedConsole.Write(" " + matrix[i, j].ToString(format));
              if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

          LoggedConsole.WriteLine();
      }
  }

        public static void writeMatrix(double[,] matrix)
  {
      writeMatrix(matrix, "F2");
  }

        public static void WriteMatrix(int[,] matrix)
  {
      int rowCount = matrix.GetLength(0); // height
      int colCount = matrix.GetLength(1); // width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              LoggedConsole.Write(" " + matrix[i, j]);
              if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

          LoggedConsole.WriteLine();
      }
  }

        public static void writeMatrix2File(double[,] matrix, string fPath)
  {
      int rowCount = matrix.GetLength(0); // height
      int colCount = matrix.GetLength(1); // width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              LoggedConsole.Write(" " + matrix[i, j].ToString("F2"));
              if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

          LoggedConsole.WriteLine();
      }
  }

        /// <summary>
        /// this method written to display silence/noise profile of wav file. May not fit general use.
        /// Must be shifted and scaled because all values are neg
        /// </summary>
        /// <param name="data"></param>
        public static void writeBarGraph(double[] data)
  {
      LoggedConsole.WriteLine("BAR GRAPH OF DOUBLES DATA");
      MinMax(data, out var min, out var max);
      if (max < 10.0)
      {
          min = 0;
          max = 80;
          data = Normalise(data, min, max);
      }

      int[] intdata = new int[data.Length];
      for (int i = 0; i < data.Length; i++)
            {
                intdata[i] = (int)Math.Round(2 * (data[i] - min)); // subtract min to remove neg values
            }

      writeBarGraph(intdata);
  }

        public static void writeBarGraph(int[] data)
  {
      MinMax(data, out var min, out var max);
    int sf = 1;  // scaling factor for graphing bars
    if (max > 80)
            {
                sf = 2;
            }

    if (max > 160)
            {
                sf = 3;
            }

    if (max > 240)
            {
                sf = 4;
            }

    if (max > 320)
            {
                sf = 5;
            }

    if (max > 400)
            {
                sf = 6;
            }

    if (max > 480)
            {
                sf = 7;
            }

    if (max > 560)
            {
                sf = 8;
            }

    if (max > 640)
            {
                sf = 9;
            }

    if (max > 720)
            {
                sf = 10;
            }

    if (max > 800)
            {
                sf = 20;
            }

    if (max > 1600)
            {
                sf = 30;
            }

    if (max > 3200)
            {
                sf = 50;
            }

    if (max > 4800)
            {
                sf = 70;
            }

    if (max > 6400)
            {
                sf = 90;
            }

    if (max > 7200)
            {
                sf = 100;
            }

    if (max > 8000)
            {
                sf = 200;
            }

    if (max > 16000)
            {
                sf = 300;
            }

    if (max > 32000)
            {
                sf = 600;
            }

    if (max > 64000)
            {
                sf = 1200;
            }

    if (max > 128000)
            {
                sf = 2400;
            }

    for (int i = 0; i < data.Length; i++)
    {
      if (max < 20)
                {
                    data[i] *= 2;
                }

      if (i < 10)
                {
                    LoggedConsole.Write(" "); // formatting only
                }

      LoggedConsole.Write(" " + i + "|");
      double v = data[i] / (double)sf;
      int ht = (int)Math.Floor(v);
      if (v > 0.00 && v < 1.00)
                {
                    ht = 1;
                }

      for (int j = 0; j < ht; j++)
                {
                    LoggedConsole.Write("=");
                }

      LoggedConsole.WriteLine();

      // if(i % 50 == 0) Console.ReadLine();
    }

    LoggedConsole.WriteLine("Min=" + min + " Max=" + max + "  Scaling factor=" + sf);
    LoggedConsole.WriteLine();
  }

          /// <summary>
          /// Logical AND of two vectors vector v2 to v1
          /// </summary>
        public static byte[] LogicalORofTwoVectors(byte[] v1, byte[] v2)
          {
              int L1 = v1.Length;
              int L2 = v2.Length;
              if (L1 != L2)
                    {
                        throw new Exception("ERROR! Vectors must be of same length.");
                    }

              byte[] addition = new byte[L1];
              for (int i = 0; i < L1; i++)
              {
                  if (v1[i] >= 1 || v2[i] >= 1)
                        {
                            addition[i] = 1;
                        }
                    }

              return addition;
          }

        public static double[] ConcatenateVectors(params double[][] vectors)
        {
            int length = vectors.Sum(x => x.Length);

            var output = new double[length];

            int index = 0;
            foreach (var vector in vectors)
            {
                Buffer.BlockCopy(vector, 0, output, index * sizeof(double), vector.Length * sizeof(double));
                index += vector.Length;
            }

            return output;
        }

        public static double[] ConcatenateVectors(List<double[]> list)
        {
            int length = 0;
            foreach (double[] v in list)
            {
                length += v.Length;
            }

            var outputVector = new double[length];

            int id = 0;
            foreach (double[] v in list)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    outputVector[id] = v[i];
                    id++;
                }
            }

            return outputVector;
        }

        /// <summary>
        /// Add vector v2 to v1
        /// </summary>
        public static double[] AddVectors(double[] v1, double[] v2)
          {
              if (v1.Length != v2.Length)
                {
                    throw new Exception("ERROR! Vectors must be of same length.");
                }

              double[] addition = new double[v1.Length];
              for (int i = 0; i < v1.Length; i++)
                  {
                      addition[i] = v1[i] + v2[i];
                  }

              return addition;
          }

          /// <summary>
          /// Subtract vector v2 from vector v1
          /// </summary>
        public static double[] SubtractVectors(double[] v1, double[] v2)
          {
              int L1 = v1.Length;
              int L2 = v2.Length;
              if (L1 != L2)
                    {
                        throw new Exception("ERROR! Vectors must be of same length.");
                    }

              double[] difference = new double[L1];
              for (int i = 0; i < L1; i++)
              {
                  difference[i] = v1[i] - v2[i];
              }

              return difference;
          }

  /// <summary>
  /// ADD matrix m2 to matrix m1
  /// </summary>
  /// <param name="m1"></param>
  /// <param name="m2"></param>
  /// <returns></returns>
        public static double[,] AddMatrices(double[,] m1, double[,] m2)
  {
      int m1Rows = m1.GetLength(0);
      int m1Cols = m1.GetLength(1);
      int m2Rows = m2.GetLength(0);
      int m2Cols = m2.GetLength(1);
      if (!(m1Rows == m2Rows))
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

      if (!(m1Cols == m2Cols))
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

      double[,] newMatrix = (double[,])m1.Clone();
      for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
          {
              newMatrix[i, j] = m1[i, j] + m2[i, j];
          }
            }

      return newMatrix;
  }

  /// <summary>
  /// Subtract matrix m2 from matrix m1
  /// </summary>
  /// <param name="m1"></param>
  /// <param name="m2"></param>
  /// <returns></returns>
        public static double[,] SubtractMatrices(double[,] m1, double[,] m2)
  {
      int m1Rows = m1.GetLength(0);
      int m1Cols = m1.GetLength(1);
      int m2Rows = m2.GetLength(0);
      int m2Cols = m2.GetLength(1);
      if (!(m1Rows == m2Rows))
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

      if (!(m1Cols == m2Cols))
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

      double[,] newMatrix = (double[,])m1.Clone();
      for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
          {
              newMatrix[i, j] = m1[i, j] - m2[i, j];
          }
            }

      return newMatrix;
  }

  /// <summary>
  /// Returns binary matrix with values set = 1 if they exceed the threshold else set = 0;
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
        public static double[,] Threshold(double[,] matrix, double threshold)
  {
      int height = matrix.GetLength(0);
      int width = matrix.GetLength(1);
      double[,] M = new double[height, width];

      for (int col = 0; col < width; col++) // for all cols
      {
          for (int y = 0; y < height; y++) // for all rows
          {
              if (matrix[y, col] < threshold)
                    {
                        M[y, col] = 0.0;
                    }
                    else
                    {
                        M[y, col] = 1.0;
                    }
                }
      }// for all cols

      return M;
  }// end of Threshold()

        public static double[] GetColumn(double[,] m, int colID)
  {
      int rows = m.GetLength(0);
      double[] column = new double[rows];
      for (int i = 0; i < rows; i++)
            {
                column[i] = m[i, colID];
            }

      return column;
  }

        public static void SetColumn(double[,] m, int colID, double[] array)
  {
      int rows = m.GetLength(0);
      for (int r = 0; r < rows; r++)
            {
                m[r, colID] = array[r];
            }
        }

  /// <summary>
  /// set all values in the passed column to zero.
  /// </summary>
  /// <param name="m"></param>
  /// <param name="colID"></param>
        public static void SetColumnZero(byte[,] m, int colID)
  {
      int rows = m.GetLength(0);
      for (int r = 0; r < rows; r++)
            {
                m[r, colID] = 0;
            }
        }

        public static int SumColumn(int[,] m, int colID)
  {
      int rows = m.GetLength(0);
      int sum = 0;
      for (int i = 0; i < rows; i++)
            {
                sum += m[i, colID];
            }

      return sum;
  }

        public static double[] GetRow(double[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      double[] row = new double[cols];
      for (int i = 0; i < cols; i++)
            {
                row[i] = m[rowID, i];
            }

      return row;
  }

        public static byte[] GetRow(byte[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      byte[] row = new byte[cols];
      for (int i = 0; i < cols; i++)
            {
                row[i] = m[rowID, i];
            }

      return row;
  }

        public static double GetRowSum(double[,] m, int rowID)
  {
      double[] row = GetRow(m, rowID);
      return row.Sum();
  }

        public static int GetRowSum(byte[,] m, int rowID)
  {
      int sum = 0;
      for (int j = 0; j < m.GetLength(1); j++)
            {
                sum += m[rowID, j];
            }

      return sum;
  }

        public static int[] GetRowSums(byte[,] m)
  {
      int[] rowSums = new int[m.GetLength(0)];
      for (int i = 0; i < m.GetLength(0); i++)
      {
          int sum = 0;
          for (int j = 0; j < m.GetLength(1); j++)
                {
                    sum += m[i, j];
                }

          rowSums[i] = sum;
      }

      return rowSums;
  }

        public static int[] GetColumnSums(byte[,] m)
  {
      int[] colSums = new int[m.GetLength(1)];
      for (int c = 0; c < m.GetLength(1); c++)
      {
          int sum = 0;
          for (int r = 0; r < m.GetLength(0); r++)
                {
                    sum += m[r, c];
                }

          colSums[c] = sum;
      }

      return colSums;
  }

        public static int GetRowSum(int[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      int sum = 0;
      for (int i = 0; i < cols; i++)
            {
                sum += m[rowID, i];
            }

      return sum;
  }

        public static int GetColumnSum(int[,] m, int colID)
  {
      int rowCount = m.GetLength(0);
      int sum = 0;
      for (int r = 0; r < rowCount; r++)
            {
                sum += m[r, colID];
            }

      return sum;
  }

        public static double[] SumColumns(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double sum = 0.0;
      double[] colSums = new double[cols];
      for (int j = 0; j < cols; j++)
      {
          sum = 0.0;
          for (int i = 0; i < rows; i++)
          {
              sum += m[i, j];
          }

          colSums[j] = sum;
      }

      // sum = 0.0;
      // for (int j = 0; j < cols; j++) sum += colSums[j];
      // LoggedConsole.WriteLine("sum="+sum.ToString("F5"));
      return colSums;
  }

        public static double[] GetColumnsAverages(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double sum = 0.0;
      double[] colSums = new double[cols];
      for (int j = 0; j < cols; j++)
      {
          sum = 0.0;
          for (int i = 0; i < rows; i++)
          {
              sum += m[i, j];
          }

          colSums[j] = sum / rows;
      }

      return colSums;
  }

        public static double[] GetWeightedCombinationOfColumns(List<double[]> arrays, double[] wts)
  {
      int arrayLength = arrays[0].Length;
      double[] weightedCombo = new double[arrayLength];

      for (int i = 0; i < arrayLength; i++)
      {
          double combo = 0.0;
          for (int c = 0; c < arrays.Count; c++)
          {
              combo += wts[c] * arrays[c][i];
          }

          weightedCombo[i] = combo;
      }

      return weightedCombo;
  }

        public static double[] SumMinusDifference(double[] lowerArray, double[] upperArray)
    {
        if (lowerArray.Length != upperArray.Length)
        {
            LoggedConsole.WriteWarnLine("DataTools.SumMinusDifference(): Two vectors are not the same length");
            return null;
        }

        var smdVector = new double[lowerArray.Length];
        for (int i = 0; i < lowerArray.Length; i++)
        {
            smdVector[i] = lowerArray[i] + upperArray[i] - Math.Abs(lowerArray[i] - upperArray[i]);
        }

        return smdVector;
    }

        // copy first n values of vector1 into vector 2}
        public static double[] CopyVector(int n, double[] v1)
    {
        double[] v2 = new double[v1.Length];
        for (int i = 0; i < n; i++)
            {
                v2[i] = v1[i];
            }

        return v2;
    }

    // returns copy of a vector
        public static double[] CopyVector(double[] v1)
    {
        double[] v2 = new double[v1.Length];
        for (int i = 0; i < v1.Length; i++)
            {
                v2[i] = v1[i];
            }

        return v2;
    }

        public static double[] ScaleArray(double[] v, int newLength)
        {
            int L = v.Length;
            if (newLength == L)
            {
                return v;
            }

            double[] ret = new double[newLength];
            double ratio = newLength / (double)L;

            for (int i = 0; i < newLength; i++)
            {
                int index = (int)(i / ratio);
                ret[i] = v[index];
            }

            return ret;
        }

        /// <summary>
        /// returns an array of double initialised with passed value
        /// </summary>
        public static double[] GetInitialisedArray(int length, double iniValue)
        {
          double[] array = new double[length];
          for (int i = 0; i < length; i++)
            {
                array[i] = iniValue;
            }

          return array;
        }

        public double NormaliseValue(double val, double normMin, double normMax)
        {
            double range = normMax - normMin;
            double norm = (val - normMin) / range;
            if (norm > 1.0)
            {
                norm = 1.0;
            }
            else if (norm < 0.0)
            {
                norm = 0.0;
            }

            return norm;
        }

        /// <summary>
        /// normalised matrix of real values to [0,1].
        /// </summary>
        public static double[,] normalise(double[,] m)
        {
            double min = double.MaxValue;
            double max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }
                    else
                        if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;

            double[,] ret = new double[rows, cols];
            if (range == 0.0)
            {
                return ret;
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ret[i, j] = (m[i, j] - min) / range;
                }
            }

            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// Used for Syntactic pattern recognition
        /// Converts Image matrix to Spectrogram data orientation
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static char[,] MatrixRotate90Clockwise(char[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new char[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[rows - r, c];
                }
            }

            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static double[,] MatrixRotate90Clockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[rows - r - 1, c];
                }
            }

            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees anticlockwise.
        /// Used for Syntactic pattern recognition
        /// Converts Image matrix to Spectrogram data orientation
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static double[,] MatrixRotate90Anticlockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[r, cols - 1 - c];
                }
            }

            return ret;
        }

        /// <summary>
        /// performs a matrix transform
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>
        public static double[,] MatrixTranspose(double[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] Mt = new double[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Mt[c, r] = M[r, c];
                }
            }

            return Mt;
        }

        /// <summary>
        /// transforms a matrix of char.
        /// Used for Syntatctic pttern recognition
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>

        public static char[,] MatrixTranspose(char[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new char[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[r, c];
                }
            }

            return ret;
        }

        /// <summary>
        /// performs a matrix transform
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>
        public static byte[,] MatrixTranspose(byte[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            byte[,] Mt = new byte[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Mt[c, r] = M[r, c];
                }
            }

            return Mt;
        }

        public static double[,] ScaleMatrix(double[,] matrix, int newRowCount, int newColCount)
    {
            int mrows = matrix.GetLength(0);
            int mcols = matrix.GetLength(1);
            var newMatrix = new double[newRowCount, newColCount];
            double rowScale = newRowCount / (double)matrix.GetLength(0);
            double colScale = newColCount / (double)matrix.GetLength(1);
            for (int r = 0; r < mrows; r++)
            {
                    for (int c = 0; c < mcols; c++)
                    {
                        if (matrix[r, c] == 0.0)
                    {
                        continue;
                    }

                        int newRow = (int)Math.Round(r * rowScale);
                        int newCol = (int)Math.Round(c * colScale);
                        newCol = (int)Math.Round(c * 4.1);

                        // newCol = 1;
                        if (newRow >= newRowCount)
                    {
                        newRow = newRowCount - 1;
                    }

                        if (newCol >= newColCount)
                    {
                        newCol = newColCount - 1;
                    }

                    // if (newMatrix[newRow, newCol] < matrix[r, c])
                        newMatrix[newRow, newCol] = matrix[r, c];
                    }
            }

            return newMatrix;
    }

// ===========================================================================================================================================================

        /// <summary>
        /// This algorithm is derived from the Lamel et al algorithm used in the SNR class.
        /// Only difference is return the true model value whereever it is.
        /// The relevant lines have been commented.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Q"></param>
        /// <param name="oneSD"></param>
        public static void ModalValue(double[] array, out double Q, out double oneSD)
        {
            int L = array.Length;

            // CALCULATE THE MIN AND MAX OF THE ARRAY
            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = 0; i < L; i++)
            {
                if (array[i] < min)
                {
                    min = array[i];
                }
                else
                    if (array[i] > max)
                {
                    max = array[i];
                }
            }

            // set up Histogram
            int binCount = 100;
            double binWidth = (max - min) / binCount;
            int[] histo = new int[binCount];

            for (int i = 0; i < L; i++)
            {
                int id = (int)((array[i] - min) / binWidth);
                if (id >= binCount)
                {
                    id = binCount - 1;
                }
                else
                    if (id < 0)
                {
                    id = 0;
                }

                histo[id]++;
            }

            double[] smoothHisto = filterMovingAverage(histo, 3);

            // DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            // FIND MAX VALUE IN BOTTOM FRACTION OF RANGE. ASSUMES NOISE IS GAUSSIAN and that their is some signal.
            // int upperBound = (int)(binCount * SNR.FRACTIONAL_BOUND_FOR_MODE);
            // for (int i = upperBound; i < binCount; i++) smoothHisto[i] = 0;//set top N% of intensity bins = 0.
            int peakID = GetMaxIndex(smoothHisto);
            Q = min + ((peakID + 1) * binWidth); // modal noise level

            // calculate SD of the background noise
            double total = 0;
            double ssd = 0.0; // sum of squared deviations
            for (int i = 0; i < peakID; i++)
            {
                total += smoothHisto[i];
                double dev = (peakID - i) * binWidth;
                ssd += dev * dev; // sum of squared deviations
            }

            if (peakID > 0)
            {
                oneSD = Math.Sqrt(ssd / total);
            }
            else
            {
                oneSD = Math.Sqrt(binWidth * binWidth / smoothHisto[0]); // deal with case where peakID = 0 to prevent division by 0;
            }
        }

        /// <summary>
        /// shift values by their mean.
        /// </summary>
        public static double[,] DiffFromMean(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            NormalDist.AverageAndSD(m, out var av, out var sd);

            double[,] ret = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ret[i, j] = m[i, j] - av;
                }
            }

            return ret;
        }

        /// <summary>
        /// shift values by their mean.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] DiffFromMean(double[] V)
        {
            int L = V.Length;
            NormalDist.AverageAndSD(V, out var av, out var sd);

            double[] returnV = new double[L];

            for (int i = 0; i < L; i++)
            {
                returnV[i] = V[i] - av;
            }

            return returnV;
        }

        /// <summary>
        /// shift values by their mean.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] Vector2Zscores(double[] V)
        {
            int L = V.Length;
            NormalDist.AverageAndSD(V, out var av, out var sd);

            double[] returnV = new double[L];

            for (int i = 0; i < L; i++)
            {
                returnV[i] = (V[i] - av) / sd;
            }

            return returnV;
        }

        public static double DotProduct(double[] v1, double[] v2)
        {
            // assume v1 and v2 have same dimensions
            int L = v1.Length;
            double sum = 0.0;
            for (int i = 0; i < L; i++)
            {
                sum += v1[i] * v2[i];
            }

            return sum;
        }

        public static double PatternSimilarity(double[] pattern, double[] data)
        {
            // assume vectors have same dimensions

            var normalisedPattern = DataTools.normalise(pattern);
            var normalisedData = DataTools.Normalise(data, -1.0, 1.0);
            double sum = 0.0;
            for (int i = 0; i < pattern.Length; i++)
            {
                sum += normalisedPattern[i] * normalisedData[i];
            }

            double similarity = sum / pattern.Length;

            // if (similarity < 0)
            // {
            //    similarity = 0.0;
            // }

            return similarity;
        }

        public static double CosineSimilarity(double[] v1, double[] v2)
        {
            // assume v1 and v2 have same dimensions
            double sum = 0.0;
            double v1Squared = 0.0;
            double v2Squared = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += v1[i] * v2[i];
                v1Squared += v1[i] * v1[i];
                v2Squared += v2[i] * v2[i];
            }

            return sum / (Math.Sqrt(v1Squared) * Math.Sqrt(v2Squared));
        }

        public static double CosineSimilarity(double[,] m1, double[,] m2)
        {
            // assume m1 and m2 have same dimensions
            double sum = 0.0;
            double m1Squared = 0.0;
            double m2Squared = 0.0;
            for (int r = 0; r < m1.GetLength(0); r++)
            {
                for (int c = 0; c < m1.GetLength(1); c++)
                {
                    sum += m1[r, c] * m2[r, c];
                    m1Squared += m1[r, c] * m1[r, c];
                    m2Squared += m2[r, c] * m2[r, c];
                }
            }

            return sum / (Math.Sqrt(m1Squared) * Math.Sqrt(m2Squared));
        }

        /// <summary>
        /// Calculates Hamming distance for two vectors of doubles.
        /// d[i] = 1 if((int)Math.Round(Math.Abs(v1[i] - v2[i])) == 1 )
        /// </summary>
        public static int HammingDistance(double[] v1, double[] v2)
        {
            // assume v1 and v2 have same dimensions
            int L = v1.Length;
            int d = 0;
            for (int i = 0; i < L; i++)
            {
                if ((int)Math.Round(Math.Abs(v1[i] - v2[i])) == 1)
                {
                    d++;
                }
            }

            return d;
        }

        /// <summary>
        /// returns EUCLIDIAN DISTANCE BETWEEN two matrices
        /// </summary>
        public double EuclideanDistance(double[,] m1, double[,] m2)
        {
            // check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2)
            {
                throw new Exception("Matrices have unequal row numbers.");
            }

            if (cols1 != cols2)
            {
                throw new Exception("Matrices have unequal column numbers.");
            }

            // calculate euclidean distance
            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    double v = m1[i, j] - m2[i, j];
                    sum += v * v;
                }
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// returns EUCLIDEAN DISTANCE BETWEEN two vectors.
        /// </summary>
        public static double EuclideanDistance(double[] v1, double[] v2)
        {
            // check v1 and v2 have same length
            if (v1.Length != v2.Length)
            {
                throw new Exception("Vectors have unequal length.");
            }

            // calculate euclidean distance
            double sum = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                double v = v1[i] - v2[i];
                sum += v * v;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// returns EUCLIDEAN DISTANCE BETWEEN two vectors of byte.
        /// </summary>
        public static double EuclideanDistance(byte[] v1, byte[] v2)
        {
            // check v1 and v2 have same length
            if (v1.Length != v2.Length)
            {
                throw new Exception("Vectors have unequal length.");
            }

            // calculate euclidean distance
            double sum = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                double v = v1[i] - v2[i];
                sum += v * v;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// returns MANHATTAN DISTANCE BETWEEN two vectors.
        /// </summary>
        public static double ManhattanDistance(double[] v1, double[] v2)
        {
            // check v1 and v2 have same length
            if (v1.Length != v2.Length)
            {
                throw new Exception("Vectors have unequal length.");
            }

            double sum = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                    double diff = Math.Abs(v1[i] - v2[i]);
                    sum += diff;
            }

            return sum;
        }

        public static double DotProduct(double[,] m1, double[,] m2)
        {
            // check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2)
            {
                throw new Exception("Matrices have unequal row numbers.");
            }

            if (cols1 != cols2)
            {
                throw new Exception("Matrices have unequal column numbers.");
            }

            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    sum += m1[i, j] * m2[i, j];
                }
            }

            return sum;
        }

        /// <summary>
        /// convert values to Decibels.
        /// Assume that all values are positive
        /// </summary>
        /// <param name="m">matrix of positive power values</param>
        /// <returns></returns>
        public static double[,] DeciBels(double[,] m, out double min, out double max)
        {
            min = double.MaxValue;
            max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double dBels = 10 * Math.Log10(m[i, j]);    // convert power to decibels

                    // NOTE: the decibels calculation should be a ratio.
                    // Here the ratio is implied ie relative to the power in the original normalised signal
            //        if (dBels <= min) min = dBels;
              //      else
                //    if (dBels >= max) max = dBels;
                    ret[i, j] = dBels;
                }
            }

            return ret;
        }

        public static double[] Order(double[] array, double[] order)
        {
            int length = array.Length;
            double[] orderedArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                int location = (int)order[i];
                orderedArray[location] = array[i];
            }

            return orderedArray;
        }

        /// <summary>
        /// normalises the values in a matrix between the passed min and max.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] Normalise(double[,] m, double normMin, double normMax)
        {
            // m = NormaliseMatrixValues(m);
            double min = double.MaxValue;
            double max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }

                    if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;
            double normRange = normMax - normMin;

            // LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }
            }

            return ret;
        }

        /// <summary>
        /// normalises the values in a vector between the passed min and max.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[] Normalise(double[] v, double normMin, double normMax)
        {
            // m = NormaliseMatrixValues(m);
            double min = double.MaxValue;
            double max = -double.MaxValue;

            int length = v.Length;
            double[] ret = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (v[i] > max)
                {
                    max = v[i];
                }

                if (v[i] < min)
                {
                    min = v[i];
                }
            }

            double range = max - min;
            double normRange = normMax - normMin;

            // LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

            for (int i = 0; i < length; i++)
            {
                double norm01 = (v[i] - min) / range;
                ret[i] = normMin + (norm01 * normRange);
            }

            return ret;
        }

        /// <summary>
        /// normalises the values in a vector such that the passed min value = 0
        /// and the passed max value = 1.0
        /// Values LT 0.0 and GT 1.0 are truncated.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double NormaliseInZeroOne(double value, double normMin, double normMax)
        {
            double range = normMax - normMin;
            double norm = (value - normMin) / range;
            if (norm > 1.0)
            {
                norm = 1.0;
            }

            if (norm < 0.0)
            {
                norm = 0.0;
            }

            return norm;
        }

        /// <summary>
        /// normalises the values in a vector such that the passed min value = 0
        /// and the passed max value = 1.0
        /// Values LT 0.0 and GT 1.0 are truncated.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[] NormaliseInZeroOne(double[] v, double normMin, double normMax)
        {
            int length = v.Length;
            double[] ret = new double[length];
            double range = normMax - normMin;

            for (int i = 0; i < length; i++)
            {
                double value = (v[i] - normMin) / range;
                if (value > 1.0)
                {
                    value = 1.0;
                }

                if (value < 0.0)
                {
                    value = 0.0;
                }

                ret[i] = value;
            }

            return ret;
        }

        public static double[,] NormaliseInZeroOne(double[,] m, double normMin, double normMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];
            double range = normMax - normMin;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double value = (m[r, c] - normMin) / range;
                    if (value > 1.0)
                    {
                        value = 1.0;
                    }

                    if (value < 0.0)
                    {
                        value = 0.0;
                    }

                    ret[r, c] = value;
                }
            }

            return ret;
        }

        public static double[,] NormaliseReverseInZeroOne(double[,] m, double normMin, double normMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];
            double range = normMax - normMin;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double value = (m[r, c] - normMin) / range;
                    if (value > 1.0)
                    {
                        value = 1.0;
                    }

                    if (value < 0.0)
                    {
                        value = 0.0;
                    }

                    ret[r, c] = 1 - value;
                }
            }

            return ret;
        }

        /// <summary>
        /// normalizes the passed array between 0,1.
        /// Ensures all values are positive.
        /// WARNING: Where the min = max (e.g. a uniform distribution),
        ///          this method will return an array of NaN.
        ///          The calling method needs to check for this.
        /// </summary>
        public static double[] normalise(int[] v)
        {
            // find min an max
            int min = int.MaxValue;
            int max = -int.MaxValue;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max)
                {
                    max = v[i];
                }

                if (v[i] < min)
                {
                    min = v[i];
                }
            }

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                ret[i] = (v[i] - min) / (double)(max - min);
            }

            return ret;
        }

        public static double[] NormaliseValues(int[] val, int normMin, int normMax)
        {
            double range = normMax - normMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - normMin) / range;
                if (norms[i] > 1.0)
                {
                    norms[i] = 1.0;
                }
                else if (norms[i] < 0.0)
                {
                    norms[i] = 0.0;
                }
            }

            return norms;
        }

        /// <summary>
        /// normalizes the passed array between 0,1.
        /// Ensures all values are positive
        /// </summary>
        public static double[] normalise(double[] v)
        {
            // find min an max
            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max)
                {
                    max = v[i];
                }

                if (v[i] < min)
                {
                    min = v[i];
                }
            }

            double diff = max - min;

            if (diff == 0.0)
            {
                return v;
            }

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                ret[i] = (v[i] - min) / diff;
            }

            return ret;
        }

        /// <summary>
        /// Normalises the passed vector by keeping the vector's zero
        /// but scaling the max value down to 1.0.
        /// WARNING: This method assumes that all values in the passed array are >= zero.
        /// </summary>
        public static double[] NormaliseByScalingMaxValueToOne(double[] v)
        {
            // find min an max
            double min = 0.0;
            double max = -double.MaxValue;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max)
                {
                    max = v[i];
                }
            }

            if (max == 0.0)
            {
                return v;
            }

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                ret[i] = (v[i] - min) / max;
            }

            return ret;
        }

        /// <summary>
        /// Normalises a vector in 0, 1 and also returns a threshold value accordingly normalised.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="threshold"></param>
        /// <param name="output"></param>
        /// <param name="normalisedThreshold"></param>
        public static void Normalise(double[] v, double threshold, out double[] output, out double normalisedThreshold)
        {
            // find min and max
            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max)
                {
                    max = v[i];
                }

                if (v[i] < min)
                {
                    min = v[i];
                }
            }

            var diff = max - min;
            output = new double[v.Length];
            normalisedThreshold = 0.0;

            double tolerance = 0.00000000001;
            if (Math.Abs(diff) < tolerance)
            {
                LoggedConsole.WriteErrorLine("Cannot NormaliseMatrixValues vector in method DataTools.Normalise(). Min = Max.");
                return;
            }

            // calculate the scaling factor for normalisation
            normalisedThreshold = threshold / max;
            for (int i = 0; i < v.Length; i++)
            {
                output[i] = (v[i] - min) / diff;
            }
        }

        /// <summary>
        /// Normalizes an array so that the sum of its values (area under curve) = 1.0
        /// Use to express data as probability function.
        /// WARNING: This method will NOT work where have a uniform distribution of negative values.
        /// </summary>
        public static double[] NormaliseArea(int[] array)
        {
            int length = array.Length;
            double[] v = normalise(array); // ensures all values in 0,1
            double sum = v.Sum();
            double[] ret = new double[length];

            // Check for case of uniform distribution or where normalise(array) has returned NaN values.
            if (double.IsNaN(sum))
            {
                var arraySum = array.Sum();
                for (int i = 0; i < length; i++)
                {
                    ret[i] = array[i] / (double)arraySum;
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    ret[i] = v[i] / sum;
                }
            }

            return ret;
        }

        /// <summary>
        /// Normalizes an array so that the sum of its values (area under curve) = 1.0
        /// Use to express data as probability function.
        /// WARNING: ONLY USE THIS METHOD IF ARRAY CONTAINS NEGATIVE VALUES
        /// First of all normalises array into [0,1]
        /// WARNING: This method will NOT work where have a uniform distribution of negative values.
        /// </summary>
        public static double[] NormaliseArea(double[] data)
        {
            double[] v = normalise(data); // ensures all values in 0, 1
            double sum = 0.0;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i];
            }

            if (sum == 0.0)
            {
                return data;
            }

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                ret[i] = v[i] / sum;
            }

            return ret;
        }

        /// <summary>
        /// normalises an array of doubles to probabilities that sum to one.
        /// assumes that values in the data vector are >= zero.
        /// </summary>
        public static double[] Normalise2Probabilites(double[] data)
        {
            int length = data.Length;

            double sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }

            double[] probs = new double[length];
            if (sum == 0.0)
            {
                return probs;
            }

            for (int i = 0; i < length; i++)
            {
                probs[i] = data[i] / sum;
            }

            return probs;
        } // end NormaliseProbabilites()

        /// <summary>
        /// calculates the cumulative probabilities from a prob array.
        /// assumes that values in the data vector are >= zero and sum = 1.0.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] ConvertProbabilityDistribution2CummulativeProbabilites(double[] data)
        {
            int length = data.Length;

            double sum = 0;
            double[] probs = new double[length];
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
                probs[i] = sum;
            }

            // Console.WriteLine("cumulative prob = "+sum);
            return probs;
        } // end ConvertProbabilityDistribution2CummulativeProbabilites()

        /// <summary>
        /// normalises the values in a matrix such that the minimum value
        /// is the average of the edge values.
        /// Truncate thos original values that are below the edge average.
        /// This method is used to NormaliseMatrixValues image templates where there should be no power at the edge
        /// </summary>
        /// <param name="m"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] normalise_zeroEdge(double[,] m, double normMin, double normMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double edge = 0.0;
            for (int i = 0; i < rows; i++)
            {
                edge += m[i, 0] + m[i, cols - 1]; // sum the sides
            }

            for (int i = 1; i < cols - 1; i++)
            {
                edge += m[0, i] + m[rows - 1, i]; // sum top and bottom
            }

            edge /= (2 * rows) + (2 * cols) - 4;

            // truncate everything to edge average
            // for (int i = 0; i < rows; i++)
            //    for (int j = 0; j < cols; j++) if (m[i, j] < edge) m[i, j] = edge;
            // set all edges to edge average
            for (int i = 0; i < rows; i++)
            {
                m[i, 0] = edge;
            }

            for (int i = 0; i < rows; i++)
            {
                m[i, cols - 1] = edge;
            }

            for (int i = 1; i < cols - 1; i++)
            {
                m[0, i] = edge;
            }

            for (int i = 1; i < cols - 1; i++)
            {
                m[rows - 1, i] = edge;
            }

            // find min and max
            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }

                    if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;
            double normRange = normMax - normMin;

            // LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }
            }

            return ret;
        }

        public static double[] normalise2UnitLength(int[] v)
  {
    double SS = 0.0;
    int length = v.Length;

    for (int i = 0; i < length; i++)
    {
      SS = SS + (v[i] * v[i]);
    }

    // square root of sum to get vector mapLength
    double norm = Math.Sqrt(SS);

    // LoggedConsole.WriteLine("SS="+SS+"  norm="+norm);

    double[] ret = new double[length]; // vector to return
    for (int i = 0; i < length; i++)
    {
      ret[i] = v[i] / norm;
    }

    return ret;
  }

        public static double[] normalise2UnitLength(double[] v)
  {
      double SS = 0.0; // sum of squares
      int length = v.Length;

      for (int i = 0; i < length; i++)
            {
                SS += v[i] * v[i];
            }

            // square root of sum to get vector mapLength
      double norm = Math.Sqrt(SS);

      // LoggedConsole.WriteLine("SS=" + SS + "  norm=" + norm);

      double[] ret = new double[length]; // vector to return
      for (int i = 0; i < length; i++)
      {
          ret[i] = v[i] / norm;
      }

      return ret;
  }

  // NormaliseMatrixValues and compress/bound the values
        public static double[,] Clip(double[,] m, double minPercentile, double maxPercentile)
  {
      PercentileCutoffs(m, minPercentile, maxPercentile, out var minCut, out var maxCut);
      return BoundMatrix(m, minCut, maxCut);
  }

        public static int Sum(int[] data)
  {
      if (data == null)
            {
                return 0;
            }

      int sum = 0;
      for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

      return sum;
  }

        public static double Sum(double[] data)
  {
      if (data == null)
            {
                return 0;
            }

      double sum = 0;
      for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

      return sum;
  }

        public static double[] SquareValues(double[] data)
  {
      if (data == null)
            {
                return null;
            }

      var squaredArray = new double[data.Length];
      for (int i = 0; i < data.Length; i++)
            {
                squaredArray[i] = data[i] * data[i];
            }

      return squaredArray;
  }

        public static double[] SquareRootOfValues(double[] data)
  {
      if (data == null)
            {
                return null;
            }

      var sqrtArray = new double[data.Length];
      for (int i = 0; i < data.Length; i++)
            {
                sqrtArray[i] = Math.Sqrt(data[i]);
            }

      return sqrtArray;
  }

        public static double[] LogTransform(double[] data)
        {
            if (data == null)
            {
                return null;
            }

            var logArray = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] <= 0.0)
                {
                    logArray[i] = 0.0;
                }
                else
                {
                    logArray[i] = Math.Log10(1 + data[i]);
                }
            }

            return logArray;
        }

        public static double[] LogValues(double[] data)
  {
      if (data == null)
            {
                return null;
            }

      var logArray = new double[data.Length];
      for (int i = 0; i < data.Length; i++)
      {
          if (data[i] <= 0.0)
                {
                    logArray[i] = -1000000.0;
                }
                else
                {
                    logArray[i] = Math.Log10(data[i]);
                }
            }

      return logArray;
  }

        public static int PowerOf2Exponent(int number)
  {
      int exponent = (int)(Math.Log(number) / ln2);
      return exponent;
  }

// ***************************************************************************************************************************************

        public const double ln2 = 0.69314718;   // log 2 base e

  /// <summary>
  /// Calculates the entropy of the passed discrete distribution.
  ///
  /// It is assumed that each of the elements in distr[] represent the
  /// probability of that state and that the probabilities sum to 1.0
  ///
  /// Math.log() is base e. To convert to log base 2 need to divide by the natural log of 2 = ln2 = 0.69314.
  /// NOTE: In the limit as rf approaches 0, rf*log(rf) = 0.
  ///
  /// </summary>
  /// <param name="data"></param>
  /// <returns></returns>
        public static double Entropy(double[] distr)
  {
      double H = 0.0;

      for (int i = 0; i < distr.Length; i++)
      {
          if (distr[i] != 0.00)
          {
              H -= distr[i] * Math.Log(distr[i]);
          }
      }

      return H / ln2;
  }

        public static double Entropy(double[,] matrixDistr)
          {
              double H = 0.0;
              int RowCount = matrixDistr.GetLength(0);
              int ColCount = matrixDistr.GetLength(1);

              for (int i = 0; i < RowCount; i++)
              {
                  for (int j = 0; j < ColCount; j++)
                  {
                      if (matrixDistr[i, j] != 0.00)
                            {
                                H -= matrixDistr[i, j] * Math.Log(matrixDistr[i, j]);
                            }
                        }
              }

              return H / ln2;
          }

        /// <summary>
        /// returns the entropy of a vector of values normalized for vector length
        /// </summary>
        public static double EntropyNormalised(double[] v)
        {
            // some safety checks but unlikely to happen
            int posCount = v.Count(p => p > 0.0);
            if (posCount == 0)
            {
                return double.NaN; // cannot calculate entropy
            }

            if (posCount == 1)
            {
                return 0.0; // energy concentrated in one value - i.e. zero entropy
            }

            double[] pmf2 = Normalise2Probabilites(v); // pmf = probability mass funciton
            double normFactor = Math.Log(v.Length) / ln2; // normalize for length of the array
            return Entropy(pmf2) / normFactor;
        }

        public static double EntropyNormalised(int[] v)
        {
            // some safety checks but unlikely to happen
            int posCount = v.Count(p => p > 0.0);
            if (posCount == 0)
                {
                    return double.NaN; // cannot calculate entropy
                }

            if (posCount == 1)
                {
                    return 0.0;        // energy concentrated in one value - i.e. zero entropy
                }

            double[] pmf2 = NormaliseArea(v); // pmf = probability mass funciton
            double normFactor = Math.Log(v.Length) / ln2; // normalize for length of the array
            return Entropy(pmf2) / normFactor;
        }

        public static double MutualInformation(int[,] counts)
        {
            // some safety checks but unlikely to happen
            // int posCount = v.Count(p => p > 0.0);
            // if (posCount == 0) return Double.NaN; // cannot calculate entropy
            // if (posCount == 1) return 0.0;        // energy concentrated in one value - i.e. zero entropy

            int rowCount = counts.GetLength(0);
            int colCount = counts.GetLength(1);

            int[] rowSums = new int[rowCount];
            int[] colSums = new int[colCount];
            int totalSum = 0;

            // accumulate counts
            for (int r = 0; r < rowCount; r++) // for all time frames
            {
                for (int c = 0; c < colCount; c++) // for all freq bins
                {
                    rowSums[r] += counts[r, c];
                    colSums[c] += counts[r, c];
                    totalSum += counts[r, c];
                }
            }

            // calcualte probabilities

            double[] rowProbs = new double[rowCount]; // pmf = probability mass funciton
            for (int r = 0; r < rowCount; r++) // for all time frames
            {
                rowProbs[r] = rowSums[r] / (double)totalSum;
            }

            // double Hrows = DataTools.EntropyNormalised(rowProbs);
            double[] colProbs = new double[colCount]; // pmf = probability mass funciton
            for (int c = 0; c < colCount; c++) // for all time frames
            {
                colProbs[c] = colSums[c] / (double)totalSum;
            }

            // double Hcols = DataTools.EntropyNormalised(colProbs);
            double[,] matrixProbs = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++) // for all time frames
            {
                for (int c = 0; c < colCount; c++) // for all freq bins
                {
                    matrixProbs[r, c] = counts[r, c] / (double)totalSum;
                }
            }

            double MI = 0.0;
            for (int r = 0; r < rowCount; r++) // for all time frames
            {
                for (int c = 0; c < colCount; c++) // for all freq bins.
                {
                    if (rowProbs[r] == 0)
                    {
                        continue;
                    }

                    if (colProbs[c] == 0)
                    {
                        continue;
                    }

                    if (matrixProbs[r, c] == 0)
                    {
                        continue;
                    }

                    MI += matrixProbs[r, c] * Math.Log(matrixProbs[r, c] / (rowProbs[r] * colProbs[c]), 2);
                }
            }

            // double normFactor = Math.Log(v.Length) / DataTools.ln2; //normalize for length of the array
            return MI;
        }

        /**
         * Calculates the relative entropy of the passed
         * discrete probability distribution.
         * It is assumed that each of the elements in dist[]
         * represents the probability of a symbol/state and the
         * probabilities sum to 1.0
         * The relative entropy is with respect to a uniform distribution.
         */
        public static double RelativeEntropy(double[] distr)
    {
        // some safety checks but unlikely to happen
        int posCount = distr.Count(p => p > 0.0);
        if (posCount == 0)
            {
                return double.NaN; // cannot calculate entropy
            }

        if (posCount == 1)
            {
                return 0.0;        // energy concentrated in one value - i.e. zero entropy
            }

        int length = distr.Length;

        // generate a uniform reference distribution
        double[] refDistr = new double[length];
        for (int i = 0; i < length; i++)
            {
                refDistr[i] = 1 / (double)length;
            }

        double H1 = Entropy(refDistr);
        double H2 = Entropy(distr);
        return H1 - H2;
    }

    /**
     * Calculates the relative entropy of the passed
     * discrete probability distribution.
     * It is assumed that each of the elements in dist[]
     * represents the probability of a symbol/state and the
     * probabilities sum to 1.0
     * The relative entropy is with respect to the background
     * or reference distribution contained in the array refDist.
     */
        public static double RelativeEntropy(double[] dist, double[] refDist)
    {
        double H1 = Entropy(refDist);
        double H2 = Entropy(dist);
        return H1 - H2;
    }

// ***************************************************************************************************************************************
  /// <summary>
  ///  Returns the min and max of a set of integer values in the passed array
  /// </summary>
  /// <param name="data">array ofintegers</param>
  /// <param name="min">min value to return</param>
  /// <param name="max">max value to return</param>
        public static void MinMax(int[] data, out int min, out int max)
  {
        min = data[0];
        max = data[0];
        for (int i = 1; i < data.Length; i++)
    {
        if (data[i] < min)
                {
                    min = data[i];
                }

        if (data[i] > max)
                {
                    max = data[i];
                }
            }
  }

        public static void MinMax(byte[] data, out byte min, out byte max)
        {
            min = data[0];
            max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }

                if (data[i] > max)
                {
                    max = data[i];
                }
            }
        }

        // =============================================================================

        public static void MinMax(double[] data, out double min, out double max)
        {
            min = data[0];
            max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }
                else
                if (data[i] > max)
                {
                    max = data[i];
                }
            }
        }

        public static void MinMaxAv(double[] data, out double min, out double max, out double av)
        {
            min = data[0];
            max = data[0];
            av = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }
                else
                    if (data[i] > max)
                    {
                        max = data[i];
                    }

                av += data[i];
            }// end for loop

            av /= data.Length;
        }

        /// <summary>
        /// returns the min and max values in a matrix of doubles
        /// </summary>
        public static void MinMax(double[,] data, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
            {
                if (data[i, j] < min)
                    {
                        min = data[i, j];
                    }
                    else if (data[i, j] > max)
                    {
                        max = data[i, j];
                    }
                }// end double loop
            }
        }

        /// <summary>
        /// returns the min and max values in a matrix of byte
        /// </summary>
        public static void MinMax(byte[,] data, out byte min, out byte max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (data[i, j] < min)
                    {
                        min = data[i, j];
                    }
                    else if (data[i, j] > max)
                    {
                        max = data[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// returns the min and max values in a matrix of byte
        /// </summary>
        public static int[] GetByteDistribution(byte[,] data)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            var byteDistribution = new int[256];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    byteDistribution[data[i, j]]++;
                }
            }

            return byteDistribution;
        }

        /**
         * returns the min and max of an array of doubles
         * and the index for both.
         * @param data
         * @return
         */
        public static void MinMax(double[] data, out int indexMin, out int indexMax, out double min, out double max)
  {
      indexMin = 0;
      indexMax = 0;

      // if (data == null) return;
      min = data[0];
      max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] < min)
          {
              min = data[i];
              indexMin = i;
          }

          if (data[i] > max)
          {
              max = data[i];
              indexMax = i;
          }
      }
  }

        public static void WriteMinMaxOfArray(double[] data)
  {
      MinMax(data, out var min, out var max);
      LoggedConsole.WriteLine("Array Min={0:F5}  Array Max={1:F5}", min, max);
  }

        public static void WriteMinMaxOfFeatures(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double min = 0;
      double max = 0;
      double av = 0;
      for (int i = 0; i < cols; i++)
      {
          double[] column = GetColumn(m, i);
          MinMaxAv(column, out min, out max, out av);
          LoggedConsole.WriteLine("Column:{0:D} min={1:F4}  max={2:F4}  av={3:F4}", i, min, max, av);
      }
  }

        public static void WriteMinMaxOfArray(string arrayname, double[] data)
  {
      MinMax(data, out var min, out var max);
      LoggedConsole.WriteLine(arrayname + ":  Min={0:F5}  Max={1:F5}", min, max);
  }

  /// <summary>
  /// Same as above method but returns index instead of outting it!
  /// returns the index of max value in an array of doubles.
  /// array index starts at zero.
  /// If more than one value is equal max, then returns location of first.
  /// </summary>
  /// <param name="data"></param>
  /// <returns></returns>
        public static int GetMaxIndex(int[] data)
  {
      int indexOfMax = 0;
      int max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] > max)
          {
              max = data[i];
              indexOfMax = i;
          }
      }

      return indexOfMax;
  }

        public static int GetMaxIndex(List<int> data)
  {
      int indexOfMax = 0;
      int max = data[0];
      for (int i = 1; i < data.Count; i++)
      {
          if (data[i] > max)
          {
              max = data[i];
              indexOfMax = i;
          }
      }

      return indexOfMax;
  }

  /**
   * returns the index of max value in an array of doubles.
   * array index starts at zero.
   * @param data
   * @return
   */
        public static void getMaxIndex(double[] data, out int indexMax)
  {
    // if(data == null) return -1;
    indexMax = 0;
    double max = data[0];
    for (int i = 1; i < data.Length; i++)
    {
        if (data[i] > max)
      {
            max = data[i];
            indexMax = i;

         // LoggedConsole.WriteLine("max="+max+"@ i="+i);
      }
    }
  }

    /// <summary>
    /// Same as above method but returns index instead of outting it!
    /// returns the index of max value in an array of doubles.
    /// array index starts at zero.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
        public static int GetMaxIndex(double[] data)
  {
      // if(data == null) return -1;
      int indexMax = 0;
      double max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] > max)
          {
              max = data[i];
              indexMax = i;
          }
      }

      return indexMax;
  }

        /// <summary>
        /// same as above but returns the index of data element having minimum value
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetMinIndex(double[] data)
  {
      // if(data == null) return -1;
      int indexMin = 0;
      double min = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] < min)
          {
              min = data[i];
              indexMin = i;
          }
      }

      return indexMin;
  }

  /**
   * returns the index of max value in an array of int
   * @param data
   * @return
   */
        public static void getMaxIndex(int[] data, out int indexMax)
  {
    // if(data == null) return -1;
    indexMax = 0;
    int max = data[0];
    for (int i = 1; i < data.Length; i++)
    {
        if (data[i] > max)
      {
            max = data[i];
            indexMax = i;

        // LoggedConsole.WriteLine("max="+max+"@ i="+i);
      }
    }
  }

        public static bool ValueInList(int value, int[] data)
   {
       for (int i = 0; i < data.Length; i++)
       {
           if (value == data[i])
                {
                    return true;
                }
            }

       return false;
   }

  /**
   * returns an array of indices where the indices point to
   * the data array with values ranked in descending order
   * @param data
   * @return
   */
        public static int[] GetRankedIndicesInDecendingOrder(double[] data)
  {
    if (data == null || data.Length == 0)
            {
                return null;
            }

    double[] dataCopy = (double[])data.Clone();
    int[] order = new int[data.Length];

    for (int i = 0; i < data.Length; i++)
    {
        getMaxIndex(dataCopy, out var maxIndex);
        order[i] = maxIndex;
        dataCopy[maxIndex] = -double.MaxValue;
    }

    return order;
  }

  /**
   * returns an array of indices where the indices point to
   * the data array with values ranked in ascending order
   * @param data
   * @return
   */
        public static int[] GetRankedIndicesInAscendingOrder(double[] data)
  {
      if (data == null || data.Length == 0)
            {
                return null;
            }

      double[] dataCopy = (double[])data.Clone();
      int[] order = new int[data.Length];
      int minIndex;

      for (int i = 0; i < data.Length; i++)
      {
          minIndex = GetMinIndex(dataCopy);
          order[i] = minIndex;
          dataCopy[minIndex] = double.MaxValue;
      }

      return order;
  }

        public static double GetNthSmallestValue(double[] data, int N)
  {
      if (data == null || data.Length == 0 || data.Length < N)
            {
                return -double.MaxValue;
            }

      double[] dataCopy = (double[])data.Clone();
      int minIndex = 0;

      for (int i = 0; i < N; i++)
      {
          minIndex = GetMinIndex(dataCopy);
          dataCopy[minIndex] = double.MaxValue;
      }

      return data[minIndex];
  }

        public static double GetNthLargestValue(double[] data, int N)
  {
      if (data == null || data.Length == 0 || data.Length < N)
            {
                return -double.MaxValue;
            }

      double[] dataCopy = (double[])data.Clone();
      int maxValue = 0;

      for (int i = 0; i < N; i++)
      {
          maxValue = GetMaxIndex(dataCopy);
          dataCopy[maxValue] = -double.MaxValue;
      }

      return data[maxValue];
  }

   /**
   *
   * @param data
   * @param min
   * @param max
   * @param binWidth
   * @return
   */
        public static int[] Data2Bins(int[] data, int min, int binWidth, int numBins)
  {
    // LoggedConsole.WriteLine("numBins="+numBins);
    // initialise bin counts
    int[] binCounts = new int[numBins];
    for (int i = 0; i < numBins; i++)
            {
                binCounts[i] = 0;
            }

    for (int i = 0; i < data.Length; i++)
    {
        int bin = (data[i] - min) / binWidth;
        binCounts[bin]++;
    }

    return binCounts;
  }

        // =============================================================================

        /// <summary>
        /// This method is used to calculate the mean and SD of acoustic indices whose distributions are very skewed, e.g. temporal entropy and cover.
        /// It returns the min, max, mode and one-sided standard deviation of an array of doubles.
        /// NOTE: The mode is prevented from being in lowest two bins because we typically do not want the mode to be near the minimum value of the distribution.
        /// This method accomodates the possibility that the distribution of index values is a truncated Gaussian or a skewed Gaussian.
        /// Once the modal position has been determined, it is assumed that the Sd is to be determined from the long-tailed side.
        /// i.e. the modal position is assumed to be the average of the underlying distribution.
        /// </summary>
        /// <param name="values">an array of values.</param>
        /// <param name="histogram">histogram dervied from passed array.</param>
        /// <param name="min">min value.</param>
        /// <param name="max">max value in the array.</param>
        /// <param name="modalBin">bin having modal value.</param>
        /// <param name="mode">the value of the mode.</param>
        /// <param name="SD">standard deviation of the distribution.</param>
        public static void GetModeAndOneTailedStandardDeviation(double[] values, out int[] histogram, out double min, out double max, out int modalBin, out double mode, out double SD)
        {
            histogram = Histogram.Histo(values, binCount: 300, binWidth: out double binWidth, min: out min, max: out max);

            // This next step is a hack for spectral acoustic indices.
            // Set lowest two histogram bins to zero.
            // We do not want a modal value in these bins when calculated bounds for LDFC spectrograms.
            histogram[0] = 0;
            histogram[1] = 0;

            // Calculate the SD on longest tail. Assume that the tail is Gaussian.
            GetModeAndOneTailedStandardDeviation(histogram, out int indexOfMode, out int indexOfOneSd);
            mode = min + (indexOfMode * binWidth);
            modalBin = indexOfMode;
            int delta = Math.Abs(indexOfOneSd - indexOfMode);
            if (delta < 1)
            {
                delta = 1;
            }

            SD = delta * binWidth;

              // the below av and sd are just a check on the one-tailed calcualtion.
              // double avDist, sdDist;
              // NormalDist.AverageAndSD(values, out avDist, out sdDist);
              // double[] avAndsd = new double[2];
              // avAndsd[0] = avDist;
              // avAndsd[1] = sdDist;
              // Console.Write("Standard av & sd for data.");
              // Console.WriteLine(NormalDist.formatAvAndSD(avAndsd, 3));
        }

  /// <summary>
  /// Assuming the passed histogram represents a distribution of values (derived from acoustic indices). which a signal is added to Gaussian noise,
      /// This method accomodates the possibility that the distribution of values is a truncated Gaussian or a skewed Gaussian.
      /// Once the modal position has been determined, it is assumed that the Sd is to be determined from the long-tailed side.
      /// i.e. the modal position is assumed to be the average of the underlying distribution.
      /// This method is used to calculate the mean and SD of acoustic indices whose distrubtions are very skewed, e.g. temporal entropy and cover.
      /// </summary>
        public static void GetModeAndOneTailedStandardDeviation(int[] histo, out int indexOfMode, out int indexOfOneSD)
  {
      // the below smoothing was added on 15th April 2015. It may or may not be helpful.
      // It was tried on the assumption that some acoustic indices have flat distributions or not many values
      //  and therefore it is valid to smooth the distribution.
      double[] smoothedHisto = filterMovingAverage(histo, 3);
      indexOfMode = GetMaxIndex(smoothedHisto);

      // indexOfMode = DataTools.GetMaxIndex(histo); // this was first line in method prior to 15th April 2015.

      int halfway = histo.Length / 2;
      double totalAreaUnderLowerCurve = 0.0;
      double partialSum, thresholdSum; // 0.68 = area under one SD
      indexOfOneSD = 0;

      if (indexOfMode >= halfway)
      {
          for (int i = indexOfMode; i >= 0; i--) // sum area back to zero inex.
          {
              totalAreaUnderLowerCurve += histo[i];
          }

          thresholdSum = totalAreaUnderLowerCurve * 0.68; // 0.68 = area under one SD
          partialSum = 0.0;
          for (int i = indexOfMode; i >= 0; i--) // sum in direction of zero index.
          {
              partialSum += histo[i];
              indexOfOneSD = i;
              if (partialSum > thresholdSum) // we have passed the one SD point
              {
                  break;
              }
          }
      }
      else // bin count < halfway - //sum area up to length of array.
      {
          for (int i = indexOfMode; i < histo.Length; i++)
          {
              totalAreaUnderLowerCurve += histo[i];
          }

          thresholdSum = totalAreaUnderLowerCurve * 0.68; // 0.68 = area under one SD
          partialSum = 0.0;
          for (int i = indexOfMode; i < histo.Length; i++) // sum in direction of max index.
          {
              partialSum += histo[i];
              indexOfOneSD = i;
              if (partialSum > thresholdSum) // we have passed the one SD point
              {
                  break;
              }
          }
      }
  } // GetModeAndOneStandardDeviation()

// =============================================================================

  /**
   * returns the area under the curve between two points on
   * the curve given by x1,y1,x2,y2.
   * Assumes that y1 and y2 are both positive.
   */
        public static double areaUnderCurve(double x1, double y1, double x2, double y2)
  {
    double width = Math.Abs(x2 - x1);
    double area = (width * y1) + (width * (y2 - y1) / 2);

    // LoggedConsole.WriteLine(x1+"  "+y1+"  "+x2+"  "+y2+"  area="+area);
    return area;
  }

 // ******************************************************************************

  /**
   * CALCULATES THE COMBINATORIAL(N,K) as
   *                 N!
   *    C(N,K) = ---------
   *             (N-K)! K!
   *
   *    C(N,K) = C(N,N-K), so it is more efficient to calculate C(N,N-K)
   *                       if K is > half of N
   *
   * To minimise chance of out of range numbers, I calculate C(N,K) by
   * multiplying a sequence of numerator/denominator values.
   *
   * NOTE: This method was originally used to calculate word association statistics.
   *       But it is no longer necessary for these calculations. Hence method not used.
   *
   *
   * @param N
   * @param K
   * @return the COMBINATORIAL as integer
   */
        public static long combinatorial(int N, int K)
  {
    if (K == 0)
            {
                return 0;
            }

    if (K > N)
            {
                return 0;
            }

    if (K == N)
            {
                return 1;
            }

            // LoggedConsole.WriteLine("N="+N+", K="+K);

            // COMB(N,K)  = COMB(N, N-K);
            // more efficient to compute combinations if use K < N/2
    if (K > N / 2)
            {
                K = N - K;
            }

            // NOTE ON ALGORITHM:
            // factorial numbers can get very large. To prevent possible out of range errors
            // this method divides numerator and denominator numbers as it goes.
            // finally caste the double to a long.

            // first component of C(N,K)
    double comb = N / (double)(N - K);

    // LoggedConsole.WriteLine("c=0  comb="+comb);

    // now multiply by remaining components of C(N,K)
    for (int c = 1; c < N - K; c++)
    {
        comb *= (N - c) / (double)(N - K - c);

      // LoggedConsole.WriteLine("c="+c+"  NOM  ="+(N-c)+"  DENOM="+(N-K-c)+"   comb="+comb);
    }

    return (long)comb;
  }

// =============================================================================

        public static void TEST_FilterMovingAverage()
  {
      int window = 99;
      double[] V0 = new double[100];
      for (int i = 0; i < V0.Length; i++)
            {
                V0[i] = i;
            }

            // for (int i = 0; i < V0.Length; i++)
            //    V0[i] = 1.0;

      double[] V1 = filterMovingAverageOLD(V0, window);
      double[] V2 = filterMovingAverage(V0, window);

      for (int i = 0; i < V1.Length; i++)
            {
                if (V1[i] != V2[i])
                {
                    Console.WriteLine("index {0}: V0={1}    {2} != {3}", i, V0[i], V1[i], V2[i]);
                }
                else
                {
                    Console.WriteLine("index {0}: V0={1} ", i, V0[i], V1[i], V2[i]);
                }
            }
        }

  /**
   * wrapper so one can call moving average filter with array of float
   */
        public static float[] filterMovingAverage(float[] signal, int width)
  {
    int length = signal.Length;
    double[] dbSignal = new double[length];
    for (int i = 0; i < length; i++)
            {
                dbSignal[i] = signal[i];
            }

    dbSignal = filterMovingAverage(dbSignal, width);
    for (int i = 0; i < length; i++)
            {
                signal[i] = (float)dbSignal[i];
            }

    return signal;
  }

  /**
   * wrapper so one can call moving average filter with array of int
   */
        public static double[] filterMovingAverage(int[] signal, int width)
  {
    int length = signal.Length;
    double[] dbSignal = new double[length];
    for (int i = 0; i < length; i++)
            {
                dbSignal[i] = signal[i]; // clone
            }

    return filterMovingAverage(dbSignal, width);
  }

/// <summary>
/// An old and now deprecated version of the moving averge filter.
/// </summary>
/// <param name="signal"></param>
/// <param name="width"></param>
/// <returns></returns>
        public static double[] filterMovingAverageOLD(double[] signal, int width)
  {
      int length = signal.Length;
      if (length <= 3)
            {
                return signal;   // not worth the effort!
            }

      double[] fs = new double[length]; // filtered signal
      int edge = width / 2;            // half window width.

      // int    odd  = width%2;          // odd or even filter window.
      double sum = 0.0;

      // filter leading edge
      for (int i = 0; i < edge; i++)
      {
          sum = 0.0;
          for (int j = 0; j <= i + edge; j++)
            {
                sum += signal[j];
            }

          fs[i] = sum / (i + edge + 1);
      }

      for (int i = edge; i < length - edge; i++)
      {
          sum = 0.0;
          for (int j = 0; j < width; j++)
            {
                sum += signal[i - edge + j];
            }

          // sum = signal[i-1]+signal[i]+signal[i+1];
          fs[i] = sum / width;
      }

      // filter trailing edge
      for (int i = length - edge; i < length; i++)
      {
          sum = 0.0;
          for (int j = i; j < length; j++)
            {
                sum += signal[j];
            }

          fs[i] = sum / (length - i);
      }

      return fs;
  }

/// <summary>
/// A new and more accurate version of the moving average filter.
/// The previous version had some errors in calculation of the trailing edge.
/// This version is also more efficient because it does not have a double loop.
/// It also makes a distinction between odd and even window width.
/// </summary>
/// <param name="signal"></param>
/// <param name="width"></param>
/// <returns></returns>
        public static double[] filterMovingAverage(double[] signal, int width)
  {
      int length = signal.Length;

      if (width >= length)
            {
                return signal;
            }

      if (length < 3)
            {
                return signal;   // not worth the effort!
            }

      if (width % 2 == 0)
            {
                return filterMovingAverageEven(signal, width);
            }
            else
            {
                return filterMovingAverageOdd(signal, width);
            }
        }

        public static double[] filterMovingAverageEven(double[] signal, int width)
  {
      int length = signal.Length;
      double[] fs = new double[length]; // filtered signal
      int edge = width / 2;             // half window width.

      double sum = 0.0;

      // sum leading edge
      for (int i = 0; i < edge; i++)
      {
          sum += signal[i];
      }

      // filter leading edge
      for (int i = 0; i <= edge; i++)
      {
          sum += signal[edge + i];
          fs[i] = sum / (edge + i + 1);
      }

      // sum the start
      sum = 0.0;
      for (int i = 0; i < width; i++)
      {
          sum += signal[i];
      }

      // Console.WriteLine("sum={0}", sum);
      // sum the central part of signal
      for (int i = edge + 1; i < length - edge; i++)
      {
          // sum = sum - left edge + right edge
          sum = sum - signal[i - edge] + signal[i + edge];
          fs[i] = sum / width;
      }

      // sum trailing edge
      sum = 0.0;
      for (int i = 0; i <= edge; i++)
      {
          sum += signal[length - i - 1];
      }

      // filter trailing edge
      for (int i = 0; i < edge; i++)
      {
          sum += signal[length - edge - i];
          fs[length - i - 1] = sum / (edge + i + 2);
      }

      return fs;
  }

        public static double[] filterMovingAverageOdd(double[] signal, int width)
  {
      int length = signal.Length;
      if (length == 3)
      {
          signal[0] = (signal[0] + signal[1]) / 2;
          signal[1] = (signal[0] + signal[1] + signal[2]) / 3;
          signal[2] = (signal[1] + signal[2]) / 2;
          return signal;
      }

      double[] fs = new double[length]; // filtered signal
      int edge = width / 2;             // half window width.

      double sum = 0.0;

      // sum leading edge
      for (int i = 0; i < edge; i++)
      {
          sum += signal[i];
      }

      // filter leading edge
      for (int i = 0; i <= edge; i++)
      {
          sum += signal[edge + i];
          fs[i] = sum / (edge + i + 1);
      }

      sum = 0.0;
      for (int i = 0; i < width; i++)
      {
          sum += signal[i];
      }

      // Console.WriteLine("sum={0}", sum);
      for (int i = edge + 1; i < length - edge; i++)
      {
          // sum = sum - left edge + right edge
          sum = sum - signal[i - edge - 1] + signal[i + edge];
          fs[i] = sum / width;
      }

      // sum trailing edge
      sum = 0.0;
      for (int i = 0; i < edge - 1; i++)
      {
          sum += signal[length - i - 1];
      }

      // filter trailing edge
      for (int i = 0; i < edge; i++)
      {
          sum += signal[length - edge - i];
          fs[length - i - 1] = sum / (edge + i);
      }

      return fs;
  }

// =========================================================================================================================
//   THE FOLLOWING GROUP OF METHODS DETECT PERIODICITY IN ARRAYS
  // Used for harmonic detection but not found to be useful in the end!!!!!!!!!

        /// <summary>
        /// Returns for each position in an array a periodicity score.
        /// That score is the maximum obtained for a range of periods over three cycles.
        /// This allowes the periodicity to change over the array and still return the maximum periodicity score.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>
        public static double[] PeriodicityDetection(double[] values, int minPeriod, int maxPeriod)
  {
      int L = values.Length;
      double[] oscillationScores = new double[L];
      int midPeriod = minPeriod + ((maxPeriod - minPeriod) / 2);
      int buffer = (int)(maxPeriod * 2.5); // avoid end of recording/array

      for (int r = 0; r < L - buffer; r++)
      {
          double maxScore = -double.MaxValue;
          for (int period = minPeriod; period < maxPeriod; period++)
          {
              double periodScore = values[r] + values[r + period] + values[r + (period * 2)];
              double offPeriodScore = values[r + (int)(period * 0.5)] + values[r + (int)(period * 1.5)] + values[r + (int)(period * 2.5)];
              periodScore -= offPeriodScore;
              if (periodScore > maxScore)
                    {
                        maxScore = periodScore;
                    }
                }

          oscillationScores[r + midPeriod] = maxScore / 3;
      }

      return oscillationScores;
  }

        /// <summary>
        /// Searches an array of values for periodicity.
        /// Calls the method PeriodicityDetection() to obtain a score for every combination of period and phase between the passed min and max bounds.
        /// The score is an estimate of the maximum amplitude for all combinations of period and phase.
        /// More accurately, the score is the difference between the average of the periodic indices and the average of the mid-period indices.
        /// Returns the maximum periodic score and the period at which it was obtained
        /// </summary>
        /// <param name="values"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>

        public static Tuple<double, int, int> Periodicity(double[] values, int minPeriod, int maxPeriod)
  {
      double maxScore = -double.MaxValue;
      double returnScore = 0.0;
      int bestPeriod = 0;
      int bestPhase = 0;
      for (int period = minPeriod; period <= maxPeriod; period++)
      {
          for (int phase = 0; phase < period; phase++)
          {
              var result = PeriodicityAndPhaseDetection(values, period, phase);
              double onPeriodScore = result.Item1;

              // double offPeriodScore = result.Item2;
              // int onCount  = result.Item3;
              // int offCount = result.Item4;
              // double periodScore = onPeriodScore - offPeriodScore;
              double periodScore = onPeriodScore;

              if (periodScore > maxScore)
              {
                  maxScore = periodScore;
                  bestPeriod = period;
                  bestPhase = phase;

                  // returnScore = (onPeriodScore / onCount) - (offPeriodScore / offCount);
                  // returnScore = periodScore;
                  returnScore = maxScore;
              }
          }// phase
      }// period

      return Tuple.Create(returnScore, bestPeriod, bestPhase);
  }

  /// <summary>
  /// returns the amplitude of an oscillation in an array having the given period.
  /// </summary>
  /// <param name="values"></param>
  /// <param name="period"></param>
  /// <param name="phase"></param>
  /// <returns></returns>
        public static System.Tuple<double, int> PeriodicityAndPhaseDetection(double[] values, int period, int phase)
  {
      int L = values.Length;
      int midPeriod = period / 2;
      double amplitude = 0.0;
      double[] smooth = filterMovingAverage(values, midPeriod);
      bool[] peaks = GetPeaks(smooth);
      int peakCount = CountTrues(peaks);
      int count = 0;
      int index = phase;
      while (index < L - midPeriod)
      {
          amplitude += values[index + midPeriod] - values[index];
          count++;
          index += period;
      }

      amplitude /= peakCount;
      return Tuple.Create(amplitude, peakCount); // amplitude of oscillation i.e. difference between min and max values
  }

        public static Tuple<double, double, int> Periodicity_MeanAndSD(double[] values)
  {
      int L = values.Length;
      double[] smooth = filterMovingAverage(values, 3);
      bool[] peaks = GetPeaks(smooth);
      int peakCount = CountTrues(peaks);

      int previousPeakLocation = 0;
      for (int i = 0; i < L; i++)
      {
          if (peaks[i])
            {
                previousPeakLocation = i;
                break;
            }
      }

      // int index = previousPeakLocation;
      List<int> periods = new List<int>();
      for (int i = previousPeakLocation + 1; i < L; i++)
      {
          if (peaks[i])
          {
              periods.Add(i - previousPeakLocation);
              previousPeakLocation = i;
          }
      }

      NormalDist.AverageAndSD(periods.ToArray(), out var mean, out var sd);
      return Tuple.Create(mean, sd, peakCount);
  }

  // ============================================================================================================================

        public static string Time_ConvertSecs2Mins(double seconds)
  {
      int mins = (int)Math.Floor(seconds / 60);
      double remainder = seconds % 60;
      string str = $"{mins}m {remainder:f3}s";
      return str;
  }

        /// <summary>
        /// assumes date string has following 8 digit strcture YYYYMMDD
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
  // public static DateTime Time_ConvertDateString2DayOfYear(string dateString)
  // {
  //      // file name has following structure:  SERF_20130314.SpectralIndices.PivotTable.csv
  //      var yr = dateString.Substring(0, 4);
  //      var mn = dateString.Substring(4, 2);
  //      var dy = dateString.Substring(6, 2);

  // var dt = new DateTime(Int32.Parse(yr), Int32.Parse(mn), Int32.Parse(dy));
  //    return dt;
  // }

        public static string[] MonthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>
        /// Assume string has format YYYYMMDD_hhmmss*
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static DateTime Time_ConvertDateString2DateTime(string dateString)
    {
        // file name has following structure:  SERF_20130314.SpectralIndices.PivotTable.csv
        var yr = dateString.Substring(0, 4);
        var mn = dateString.Substring(4, 2);
        var dy = dateString.Substring(6, 2);

        var hur = dateString.Substring(9, 2);
        var min = dateString.Substring(11, 2);
        var sec = dateString.Substring(13, 2);

        var dt = new DateTime(int.Parse(yr), int.Parse(mn), int.Parse(dy), int.Parse(hur), int.Parse(min), int.Parse(sec));
        return dt;
    }

        // ============================================================================================================================

  // *************************************************************************************************************************************

  /**
   * Pads a string to given length.
   * @param str the string
   * @return formatted string.
   */
        public static string padString_pre(string str, int width)
  {
      while (str.Length < width)
            {
                str = " " + str;
            }

      return str;
  }

  /**
   * Pads a string to given length.
   * @param str the string
   * @return formatted string.
   */
        public static string padString_post(string str, int width)
  {
      while (str.Length < width)
            {
                str = str + " ";
            }

      return str;
  }

  /**
   * Rounds a double to n places and returns as double.
   * @param d the double value
   * @param places - maximum number of decimal places
   * @return formatted string.
   */
        public static double roundDouble(double d, int places)
  {
      int n = 10;
      for (int i = 1; i < places; i++)
            {
                n *= 10;
            }

      double rd = Math.Round(d * n) / n;
      return rd;
  }

  // =============================================================================

/// <summary>
/// Given an array of monotonically increasing or decreasing values and a reference value,
/// determine whether the ref value lies above or below the index halfway between the passed lower and upper indices.
/// This method is recursive. It determines the index of the array whose value is closest to the ref value.
/// </summary>
/// <param name="array"></param>
/// <param name="value"></param>
/// <param name="lowerIndex"></param>
/// <param name="upperIndex"></param>
/// <returns></returns>

        public static int WhichSideOfCentre(double[] array, double refValue, int lowerIndex, int upperIndex)
  {
      int distance = upperIndex - lowerIndex;
      int centre = new int();
      centre = upperIndex - (distance / 2);

      if (distance <= 1)
      {
          if (refValue < array[lowerIndex])
                {
                    return lowerIndex;
                }
                else
                {
                    return upperIndex;
                }
            }

      if (refValue >= array[centre])
            {
                centre = WhichSideOfCentre(array, refValue, centre, upperIndex);
            }
            else
            {
                centre = WhichSideOfCentre(array, refValue, lowerIndex, centre);
            }

      return centre;
  }

        public static int[] SampleArrayRandomlyWithoutReplacementUsingProbabilityDistribution(double[] distribution, int sampleCount, int seed)
        {
            RandomNumber generator = new RandomNumber(seed);

            // create list to handle non-resplacement of samples
            List<int> list = new List<int>();
            for (int i = 0; i < distribution.Length; i++)
            {
                list.Add(i);
            }

            int[] returnSampleArray = new int[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                // get a random number
                double rn = generator.GetDouble();

                // get its index from probability distribution
                int distributionIndex = WhichSideOfCentre(distribution, rn, 0, distribution.Length);

                // convert distribution array index to an index appropriate to non-replacement of samples
                int newIndex = (int)Math.Floor(distributionIndex * list.Count / (double)distribution.Length);

                // recover original index
                returnSampleArray[i] = list[newIndex];
                list.RemoveAt(newIndex);
            }

            return returnSampleArray;
        }
    }// class dataTools()
}

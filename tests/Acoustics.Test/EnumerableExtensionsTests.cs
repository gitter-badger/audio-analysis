﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumerableExtensionsTests
    {
        public class DummyData
        {
            public double[] Field1 { get; set; }

            public double[] Field2 { get; set; }
        }

        private IList<DummyData> dummyDatas;

        private readonly Dictionary<string, Func<DummyData, double[]>> selectors =
            new Dictionary<string, Func<DummyData, double[]>>()
                {
                    { "Field1", dd => dd.Field1 },
                    { "Field2", dd => dd.Field2 },
                };

        [TestInitialize]
        public void SetupData()
        {
            this.dummyDatas = new[]
                                  {
                                      new DummyData()
                                          {
                                              Field1 = new[] { 0.0, 1, 2, 3, 4, },
                                              Field2 = new[] { 5.0, 6, 7, 8, 9, },
                                          },
                                      new DummyData()
                                          {
                                              Field1 = new[] { 10.0, 11, 12, 13, 14 },
                                              Field2 = new[] { 15.0, 16, 17, 18, 19 },
                                          },
                                  };
        }

        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 0.0, 1, 2, 3, 4, 10.0, 11, 12, 13, 14 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 5.0, 6, 7, 8, 9, 15.0, 16, 17, 18, 19 };
            Test2DArray(field2, expected2);

            TestDims(field1, 2, 5);
        }

        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest_ColumnMajor()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors, TwoDimensionalArray.Transpose);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 0.0, 10.0, 1, 11, 2, 12, 3, 13, 4, 14 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 5.0, 15.0, 6, 16, 7, 17, 8, 18, 9, 19 };
            Test2DArray(field2, expected2);

            TestDims(field1, 5, 2);
        }

        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest_ColumnMajorFlipped()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors, TwoDimensionalArray.Rotate90ClockWise);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 4, 14, 3, 13, 2, 12, 1, 11, 0.0, 10.0 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 9, 19, 8, 18, 7, 17, 6, 16, 5.0, 15.0 };
            Test2DArray(field2, expected2);

            TestDims(field1, 5, 2);
        }

        private static void TestDims(double[,] matrix, int rows, int columns)
        {
            Assert.AreEqual(rows, matrix.GetLength(0));
            Assert.AreEqual(columns, matrix.GetLength(1));
        }

        private static void Test2DArray(double[,] matrix, double[] expected)
        {
            Assert.IsNotNull(matrix);
            Assert.AreEqual(2, matrix.Rank);

            var index = 0;
            foreach (var d in matrix)
            {
                Assert.AreEqual(expected[index], d);
                index++;
            }
        }

        [TestMethod]
        public void TestWindowedFunction()
        {
            var input = new[] { 3, 4, 5, 6, 7, 8, 9, 10 };
            int[][] expected =
                {
                    new[] { 3, 4 }, new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 8 },
                    new[] { 8, 9 }, new[] { 9, 10 },
                };

            input.Windowed(2).ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }

        [TestMethod]
        public void TestWindowedFunctionSingleItem()
        {
            var input = new[] { 3};

            var windowed = input.Windowed(2);
            Assert.AreEqual(0, windowed.Count());
            ////windowed.ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }

        [TestMethod]
        public void TestWindowedFunctionSize3()
        {
            var input = new[] { 3, 4, 5, 6, 7, 8, 9, 10 };
            int[][] expected =
                {
                    new[] { 3, 4, 5 }, new[] { 4, 5, 6 }, new[] { 5, 6, 7 }, new[] { 6, 7, 8 },
                    new[] { 7, 8, 9 }, new[] { 8, 9, 10 },
                };

            var windowed = input.Windowed(3);
            Assert.AreEqual(expected.Length, windowed.Count());
            windowed.ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }



        [TestMethod]
        public void TestWindowedOrDefaultFunction()
        {
            var input = new[] { 3, 4, 5, 6, 7, 8, 9, 10 };
            int[][] expected =
                {
                    new[] { int.MinValue, 3 },
                    new[] { 3, 4 }, new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 8 },
                    new[] { 8, 9 }, new[] { 9, 10 }, new[] {10, int.MinValue},
                };

            input.WindowedOrDefault(2, int.MinValue).ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }

        [TestMethod]
        public void TestWindowedOrDefaultFunctionSingleItem()
        {
            var input = new[] { 3 };
            int[][] expected =
                {
                    new[] { int.MinValue, 3 },
                    new[] { 3, int.MinValue },
                };

            var windowed = input.WindowedOrDefault(2, int.MinValue);
            Assert.AreEqual(expected.Length, windowed.Count());
            windowed.ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }


        [TestMethod]
        public void TestWindowedOrDefaultFunctionSize3()
        {
            var input = new[] { 3, 4, 5, 6, 7, 8, 9, 10 };
            int[][] expected =
                {
                    new[] { 0, 0, 3 }, new[] { 0, 3, 4 },
                    new[] { 3, 4, 5 }, new[] { 4, 5, 6 }, new[] { 5, 6, 7 }, new[] { 6, 7, 8 }, new[] { 7, 8, 9 },
                    new[] { 8, 9, 10 }, new[] { 9, 10, 0 }, new[] { 10, 0, 0 },
                };

            var windowed = input.WindowedOrDefault(3);
            Assert.AreEqual(expected.Length, windowed.Count());
            windowed.ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }

        [TestMethod]
        public void TestWindowedOrDefaultFunctionSize4()
        {
            var input = new[] { 3, 4, 5, 6};
            int[][] expected =
                {
                    new[] { 0, 0, 0, 3 }, new[] { 0, 0, 3, 4 },
                    new[] { 0, 3, 4, 5 }, new[] { 3, 4, 5, 6 },
                    new[] { 4, 5, 6, 0 }, new[] { 5, 6, 0, 0 },
                    new[] { 6, 0, 0, 0 },
                };

            var windowed = input.WindowedOrDefault(4);
            Assert.AreEqual(expected.Length, windowed.Count());
            windowed.ForEach((ints, i) => CollectionAssert.AreEqual(expected[i], ints));
        }
    }
}
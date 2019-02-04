// <copyright file="LDSpectrogramRGBTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisBase.ResultBases;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LDSpectrogramRGBTests : OutputDirectoryTest
    {
        [TestMethod]
        public void TestChromelessImage()
        {
            var indexPropertiesFile = ConfigFile.Default<IndexPropertiesCollection>();
            var indexProperties = ConfigFile.Deserialize<IndexPropertiesCollection>(indexPropertiesFile);

            var indexSpectrograms = new Dictionary<string, double[,]>(6);
            var indexStatistics = new Dictionary<string, IndexDistributions.SpectralStats>();
            var keys = (LDSpectrogramRGB.DefaultColorMap1 + "-" + LDSpectrogramRGB.DefaultColorMap2).Split('-');
            foreach (var key in keys)
            {
                var matrix = new double[256, 60].Fill(indexProperties[key].DefaultValue);
                indexSpectrograms.Add(key, matrix);

                indexStatistics.Add(key, IndexDistributions.GetModeAndOneTailedStandardDeviation(matrix, 300, IndexDistributions.UpperPercentileDefault));
            }

            var images = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                inputDirectory: null,
                outputDirectory: this.outputDirectory,
                ldSpectrogramConfig: new LdSpectrogramConfig(),
                indexPropertiesConfigPath: indexPropertiesFile,
                indexGenerationData: new IndexGenerationData()
                {
                    AnalysisStartOffset = 0.Seconds(),
                    FrameLength = 512,
                    IndexCalculationDuration = 60.0.Seconds(),
                    RecordingBasename = "RGB_TEST",
                    RecordingDuration = 60.0.Seconds(),
                    SampleRateResampled = 22050,
                },
                basename: "RGB_TEST",
                analysisType: AcousticIndices.AnalysisName,
                indexSpectrograms: indexSpectrograms,
                summaryIndices: Enumerable
                    .Range(0, 60)
                    .Select((x) => new SummaryIndexValues(60.0.Seconds(), indexProperties))
                    .Cast<SummaryIndexBase>()
                    .ToArray(),
                indexStatistics: indexStatistics,
                imageChrome: ImageChrome.Without);

            foreach (var (image, key) in images)
            {
                Assert.That.ImageIsSize(60, 256, image);
                Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(0, 0, 60, 256), Color.Black, (Bitmap)image);
            }
        }

        [TestMethod]
        public void TestDrawRgbColorMatrix()
        {
            // init three matrices
            double[,] redM = new double[5, 5];
            double[,] grnM = new double[5, 5];
            double[,] bluM = new double[5, 5];

            //convert some values to null or NaN
            redM[1, 1] = double.NaN;
            grnM[1, 1] = double.NaN;
            bluM[1, 1] = double.NaN;

            redM[2, 2] = 1.0;
            grnM[2, 2] = 1.0;
            bluM[2, 2] = double.NaN;

            redM[3, 3] = 0.01;
            grnM[3, 3] = 0.01;
            bluM[3, 3] = 0.11;

            var image = (Bitmap)LDSpectrogramRGB.DrawRgbColourMatrix(redM, grnM, bluM, doReverseColour: true);

            Assert.That.PixelIsColor(new Point(1, 1), Color.FromArgb(128, 128, 128), image);
            Assert.That.PixelIsColor(new Point(2, 2), Color.FromArgb(128, 128, 128), image);

            // empty values are rendered as white because of `doReverseColour`
            Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(0,0, 1,5), Color.FromArgb(255, 255, 255), image);
            Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(4,0, 5,5), Color.FromArgb(255, 255, 255), image);

            // To intensify the blue, this method adds 0.7 * d3 to the green value;
            // and it adds 0.2 to the blue value;
            // The effect is to create a more visible cyan color.
            int r = (1 - redM[3, 3]).ScaleUnitToByte();
            int g = (1 - (grnM[3, 3] + (0.7 * bluM[3, 3]))).ScaleUnitToByte();
            int b = (1 - (bluM[3, 3] + 0.2)).ScaleUnitToByte();
            var cyanColor = Color.FromArgb(r, g, b);
            Assert.That.PixelIsColor(new Point(3, 3), cyanColor, image);
        }
    }
}

// <copyright file="Oscillations2014.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.ConfigFile;
    using DSP;
    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// This is the latest of three implementations to detect oscillations in a spectrogram.
    /// This implementation is generic, that is, it attempts to find any and all oscillations in each of the
    /// frequency bins of a short duration spectorgram.
    ///
    /// There are three versions of the generic algorithm implemented in three different methods:
    /// 1) uses auto-correlation, then FFT
    /// 2) uses auto-correlation, then singular value decomposition, then FFT
    /// 3) uses wavelets
    ///
    /// I gave up on wavelets after some time. Might work with persistence!
    /// Singular value decomposition is used as a filter to select the dominant oscillations in the audio segment against noise.
    ///
    /// The Oscillations2012 class uses the DCT to find oscillations. It works well when the sought oscillation rate is known
    /// and the DCT can be tuned to find it. It works well, for example, to find canetoad calls.
    /// However it did not easily extend to finding generic oscillations.
    ///
    /// Oscillations2014 therefore complements the Oscillations2012 class but does not replace it.
    ///
    /// </summary>
    public static class Oscillations2014
    {
        // sampleLength is the number of frames taken from a frequency bin on which to do autocorr-fft.
        // longer sample lengths are better for longer duration, slower moving events.
        // shorter sample lengths are better for short duration, fast moving events.
        public static int DefaultSampleLength = 128;
        public static double DefaultSensitivityThreshold = 0.2;

        /// <summary>
        /// In line class used to return results from the static method Oscillations2014.GetFreqVsOscillationsDataAndImage();
        /// </summary>
        public class FreqVsOscillationsResult
        {
            //  path to spectrogram image
            public string SourceFileName { get; set; }

            public string AlgorithmName { get; set; }

            public int BinSampleLength { get; set; }

            public Image FreqOscillationImage { get; set; }

            public double[,] FreqOscillationData { get; set; }

            // the FreqOscillationData matrix reduced to a vector
            public double[] OscillationSpectralIndex { get; set; }
        }

        // ########################################  OSCILLATION SPECTROGRAM TEST METHOD HERE ######################################################

        public static void TESTMETHOD_DrawOscillationSpectrogram()
        {
            {
                var sourceRecording = @"C:\Work\GitHub\audio-analysis\tests\Fixtures\Recordings\BAC2_20071008-085040.wav".ToFileInfo();
                var output = @"C:\Ecoacoustics\SoftwareTests\TestOscillationSpectrogram".ToDirectoryInfo();
                var expectedResultsDir = new DirectoryInfo(Path.Combine(output.FullName, TestTools.ExpectedResultsDir));
                if (!expectedResultsDir.Exists)
                {
                    expectedResultsDir.Create();
                }

                // 1. set up a default config dictionary
                var configDict = GetDefaultConfigDictionary(sourceRecording);

                // 2. Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
                // This was reworked October 2018
                // Vertical grid lines located every 5 cycles per sec.
                var tuple = GenerateOscillationDataAndImages(sourceRecording, configDict, true);

                // (1) Save image file of this matrix.
                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                var fileName = sourceName + ".FreqOscilSpectrogram";
                var pathName = Path.Combine(output.FullName, fileName);
                var imagePath = pathName + ".png";
                tuple.Item1.Save(imagePath, ImageFormat.Png);

                // construct output file names
                fileName = sourceName + ".FreqOscilDataMatrix";
                pathName = Path.Combine(output.FullName, fileName);
                var csvFile1 = new FileInfo(pathName + ".csv");

                fileName = sourceName + ".OSCSpectralIndex";
                pathName = Path.Combine(output.FullName, fileName);
                var csvFile2 = new FileInfo(pathName + ".csv");

                // Save matrix of oscillation data stored in freqOscilMatrix1
                Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(csvFile1, tuple.Item2);

                // Do my version of UNIT TESTING - This is the File Equality Test.
                var expectedTestFile1 = new FileInfo(Path.Combine(expectedResultsDir.FullName, "OscillationSpectrogram_MatrixTest.EXPECTED.csv"));
                var expectedTestFile2 = new FileInfo(Path.Combine(expectedResultsDir.FullName, "OscillationSpectrogram_VectorTest.EXPECTED.csv"));
                TestTools.FileEqualityTest("Matrix Equality", csvFile1, expectedTestFile1);
                TestTools.FileEqualityTest("Vector Equality", csvFile2, expectedTestFile2);
                Console.WriteLine("\n\n");
            }
        }

        /// <summary>
        /// test method for getting a spectral index of oscillation values
        /// </summary>
        public static void TESTMETHOD_GetSpectralIndex_Osc()
        {
            // 1. set up the resources
            var sourceRecording = @"C:\Work\GitHub\audio-analysis\tests\Fixtures\Recordings\BAC2_20071008-085040.wav".ToFileInfo();
            var output = @"C:\Ecoacoustics\SoftwareTests\TestOscillationSpectrogram".ToDirectoryInfo();
            var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
            var expectedResultsDir = new DirectoryInfo(Path.Combine(output.FullName, TestTools.ExpectedResultsDir));
            if (!expectedResultsDir.Exists)
            {
                expectedResultsDir.Create();
            }

            var configDict = GetDefaultConfigDictionary(sourceRecording);

            // 2. Get the spectral index
            var spectralIndex = GetSpectralIndex_Osc(sourceRecording, configDict);

            // 3. Write the vector to csv file
            var fileName1 = sourceName + ".OSCSpectralIndex.csv";
            var pathName1 = new FileInfo(Path.Combine(output.FullName, fileName1));
            Acoustics.Shared.Csv.Csv.WriteToCsv(pathName1, spectralIndex);

            // 4. draw image of the vector for debug purposes.
            var vectorImage = ImageTools.DrawVectorInColour(DataTools.reverseArray(spectralIndex), cellWidth: 5);
            var fileName2 = sourceName + ".SpectralIndex.OSC.png";
            var pathName2 = Path.Combine(output.FullName, fileName2);
            vectorImage.Save(pathName2, ImageFormat.Png);
        }

        public static Dictionary<string, string> GetDefaultConfigDictionary(FileInfo sourceRecording)
        {
            var configDict = new Dictionary<string, string>
            {
                // Do not include any noise removal parameters
                // Noise removal is unnecessary because processing frequency bins only three at a time.
                [ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName,
                [ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name,
                [AnalysisKeys.ResampleRate] = "22050",
                [AnalysisKeys.FrameLength] = "256",
                [AnalysisKeys.FrameOverlap] = "0.0",
                [AnalysisKeys.AddAxes] = "true",
                [AnalysisKeys.AddSegmentationTrack] = "true",
                [AnalysisKeys.OscilDetection2014SampleLength] = DefaultSampleLength.ToString(CultureInfo.CurrentCulture),
                [AnalysisKeys.OscilDetection2014SensitivityThreshold] = DefaultSensitivityThreshold.ToString(CultureInfo.CurrentCulture),
        };

            // print out the sonogram parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }

            return configDict;
        }

        public static Dictionary<string, string> GetConfigDictionary(FileInfo configFile)
        {
            var configuration = ConfigFile.Deserialize(configFile);

            // var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            var configDict = new Dictionary<string, string>
            {
                // below three lines are examples of retrieving info from Config config
                // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
                // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
                // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;
                // ####################################################################

                [AnalysisKeys.ResampleRate] = configuration[AnalysisKeys.ResampleRate] ?? "22050",
                [AnalysisKeys.AddAxes] = (configuration.GetBoolOrNull(AnalysisKeys.AddAxes) ?? true ).ToString(),
                [AnalysisKeys.AddSegmentationTrack] = (configuration.GetBoolOrNull(AnalysisKeys.AddSegmentationTrack) ?? true ).ToString(),
                [AnalysisKeys.AddTimeScale] = configuration[AnalysisKeys.AddTimeScale] ?? "true",
                [AnalysisKeys.AddAxes] = configuration[AnalysisKeys.AddAxes] ?? "true",
            };


            // SET THE 2 KEY PARAMETERS HERE FOR DETECTION OF OSCILLATION
            // often need different frame size doing Oscil Detection
            const int oscilDetection2014FrameSize = 256;
            configDict[AnalysisKeys.OscilDetection2014FrameSize] = oscilDetection2014FrameSize.ToString();

            // Set the sample or patch length i.e. the number of frames used when looking for oscillations along freq bins
            // 64 is better where many birds and fast changing activity
            // 128 is better where slow moving changes to acoustic activity
            //const int sampleLength = 64;
            const int sampleLength = 128;
            configDict[AnalysisKeys.OscilDetection2014SampleLength] = sampleLength.ToString();

            const double sensitivityThreshold = 0.2;
            configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold] = sensitivityThreshold.ToString(CultureInfo.CurrentCulture);
            return configDict;
        }

        /// <summary>
        /// Generates the FREQUENCY x OSCILLATIONS Graphs and csv
        /// I have experimented with five methods to search for oscillations:
        ///  1: string algorithmName = "Autocorr-FFT";
        ///     use this if want more detailed output - but not necessrily accurate!
        ///  2: string algorithmName = "Autocorr-SVD-FFT";
        ///     use this if want only dominant oscillations
        ///  3: string algorithmName = "Autocorr-Cwt";
        ///     a Wavelets option but could not get it to work well
        ///  4: string algorithmName = "Autocorr-WPD";
        ///     another Wavelets option but could not get it to work well
        ///  5: Discrete Cosine Transform
        ///     The DCT only works well when you know which periodicity you are looking for. e.g. Canetoad.
        /// </summary>
        public static Tuple<Image, double[,]> GenerateOscillationDataAndImages(FileInfo audioSegment, Dictionary<string, string> configDict, bool drawImage = false)
        {
            // set two oscillation detection parameters
            double sensitivity = DefaultSensitivityThreshold;
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SensitivityThreshold))
            {
                sensitivity = double.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);
            }

            // Sample length i.e. number of frames spanned to calculate oscillations per second
            int sampleLength = DefaultSampleLength;
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
            {
                sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);
            }

            var sonoConfig = new SonogramConfig(configDict); // default values config
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014FrameSize))
            {
                sonoConfig.WindowSize = int.Parse(configDict[AnalysisKeys.OscilDetection2014FrameSize]);
            }

            var recordingSegment = new AudioRecording(audioSegment.FullName);
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);

            // Taking the square-root emphasizes the low amplitude features.
            // Could omit this but it gives fewer detections of oscillations
            // Taking the decibel spectrogram also works
            // BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // Do not do any noise removal - it is unnecessary because only taking frequency bins three at a time.
            sonogram.Data = MatrixTools.SquareRootOfValues(sonogram.Data);

            // remove the DC bin if it has not already been removed.
            // Assume test of divisible by 2 is good enough.
            int binCount = sonogram.Data.GetLength(1);
            if (!binCount.IsEven())
            {
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, binCount - 1);
            }

            var algorithmName1 = "autocorr-svd-fft";
            var freqOscilMatrix1 = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName1);
            var image1 = GetFreqVsOscillationsImage(freqOscilMatrix1, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName1);

            var algorithmName2 = "autocorr-fft";
            var freqOscilMatrix2 = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName2);
            var image2 = GetFreqVsOscillationsImage(freqOscilMatrix2, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName2);

            //IMPORTANT NOTE: To generate an OSC spectral index matrix for making LDFC spectrograms, use the following line:
            //var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix2);

            var compositeImage = ImageTools.CombineImagesInLine(new[] { image2, image1 });

            // Return (1) composite image of oscillations,
            //        (2) data matrix from only one algorithm,
            return Tuple.Create(compositeImage, freqOscilMatrix2);
        }

        /// <summary>
        /// Only call this method for short recordings.
        /// If accumulating data for long recordings then call the method for long recordings - i.e.
        /// double[] spectralIndex = GenerateOscillationDataAndImages(FileInfo audioSegment, Dictionary configDict, false, false);
        /// </summary>
        public static FreqVsOscillationsResult GetFreqVsOscillationsDataAndImage(BaseSonogram sonogram, string algorithmName)
        {
            double sensitivity = DefaultSensitivityThreshold;
            int sampleLength = DefaultSampleLength;
            var freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName);
            var image = GetFreqVsOscillationsImage(freqOscilMatrix, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName);
            var sourceName = Path.GetFileNameWithoutExtension(sonogram.Configuration.SourceFName);

            // get the OSC spectral index
            // var spectralIndex = ConvertMatrix2SpectralIndexBySummingFreqColumns(freqOscilMatrix, skipNRows: 1);
            var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix);
            if (spectralIndex == null)
            {
                throw new ArgumentNullException(nameof(spectralIndex));
            }

            // DEBUGGING
            // Add spectralIndex into the matrix because want to add it to image.
            // This is for debugging only and can comment this line
            int rowCount = freqOscilMatrix.GetLength(0);
            MatrixTools.SetRow(freqOscilMatrix, rowCount - 2, spectralIndex);

            var result = new FreqVsOscillationsResult
            {
                SourceFileName = sourceName,
                FreqOscillationImage = image,
                FreqOscillationData = freqOscilMatrix,
                OscillationSpectralIndex = spectralIndex,
            };
            return result;
        }

        /// <summary>
        /// Creates an image from the frequency/oscillations matrix.
        /// The y-axis scale = frequency bins as per normal spectrogram.
        /// The x-axis scale is oscillations per second.
        /// </summary>
        /// <param name="freqOscilMatrix">the input frequency/oscillations matrix</param>
        /// <param name="framesPerSecond">to give the time scale</param>
        /// <param name="freqBinWidth">to give the frequency scale</param>
        /// <param name="sampleLength">to allow calculation of the oscillations scale</param>
        /// <param name="algorithmName">the algorithm used to compute the oscillations.</param>
        /// <returns>bitmap image</returns>
        public static Image GetFreqVsOscillationsImage(double[,] freqOscilMatrix, double framesPerSecond, double freqBinWidth, int sampleLength, string algorithmName)
        {
            // remove the high cycles/sec end of the matrix because nothing really happens here.
            int maxRows = freqOscilMatrix.GetLength(0) / 2;
            freqOscilMatrix = MatrixTools.Submatrix(freqOscilMatrix, 0, 0, maxRows - 1, freqOscilMatrix.GetLength(1) - 1);

            // get the OSC spectral index
            var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix);

            // Convert spectrum index to oscillations per second
            double oscillationBinWidth = framesPerSecond / sampleLength;

            //draw an image
            freqOscilMatrix = MatrixTools.MatrixRotate90Anticlockwise(freqOscilMatrix);

            // each value is to be drawn as a 5 pixel x 5 pixel square
            int xscale = 5;
            int yscale = 5;
            var image1 = ImageTools.DrawMatrixInColour(freqOscilMatrix, xPixelsPerCell: xscale, yPixelsPerCell: yscale);

            var image2 = ImageTools.DrawVectorInColour(DataTools.reverseArray(spectralIndex), cellWidth: xscale);

            var image = ImageTools.CombineImagesInLine(new[] { image1, image2 });

            // place a grid line every 5 cycles per second.
            double cycleInterval = 5.0;
            double xTicInterval = cycleInterval / oscillationBinWidth * xscale;

            // a tic every 1000 Hz.
            int herzInterval = 1000;
            double yTicInterval = herzInterval / freqBinWidth * yscale;

            int xOffset = xscale / 2;
            int yOffset = yscale / 2;
            image = ImageTools.DrawYaxisScale(image, 10, herzInterval, yTicInterval, yOffset);
            image = ImageTools.DrawXaxisScale(image, 15, cycleInterval, xTicInterval, 10, -xOffset);

            var titleBar = DrawTitleBarOfOscillationSpectrogram(algorithmName, image.Width);
            var imageList = new List<Image> { titleBar, image };
            var compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;
        }

        public static double[,] GetFrequencyByOscillationsMatrix(double[,] spectrogram, double sensitivity, int sampleLength, string algorithmName)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            var freqByOscMatrix = new double[sampleLength / 2, freqBinCount];

            // over all frequency bins
            for (int bin = 0; bin < freqBinCount; bin++)
            {
                double[,] subM;

                // get average of three bins
                if (bin == 0)
                {
                    subM = MatrixTools.Submatrix(spectrogram, 0, 0, frameCount - 1, 2);
                }
                else // get average of three bins
                    if (bin == freqBinCount - 1)
                    {
                        subM = MatrixTools.Submatrix(spectrogram, 0, bin - 2, frameCount - 1, bin);
                    }
                    else
                    {
                        // get average of three bins
                        subM = MatrixTools.Submatrix(spectrogram, 0, bin - 1, frameCount - 1, bin + 1);
                    }

                var freqBin = MatrixTools.GetRowAverages(subM);

                // could alternatively take single bins but averaging three bins seems to work well
                //var freqBin = MatrixTools.GetColumn(spectrogram, bin);

                // vector to store the oscillations vector derived from one frequency bin.
                double[] oscillationsSpectrum = null;
                var xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);

                // Use the Autocorrelation - FFT option.
                // This option appears to work best for use as a spectral index to detect oscillations
                if (algorithmName.Equals("autocorr-fft"))
                {
                    oscillationsSpectrum = GetOscillationArrayUsingFft(xCorrByTimeMatrix, sensitivity);
                }

                // Use the Autocorrelation - SVD - FFT option.
                if (algorithmName.Equals("autocorr-svd-fft"))
                {
                    oscillationsSpectrum = GetOscillationArrayUsingSvdAndFft(xCorrByTimeMatrix, sensitivity, bin);
                }

                // Use the Wavelet Transform
                if (algorithmName.Equals("Autocorr-WPD"))
                {
                    oscillationsSpectrum = GetOscillationArrayUsingWpd(xCorrByTimeMatrix, sensitivity, bin);

                    //WaveletTransformContinuous cwt = new WaveletTransformContinuous(freqBin, maxScale);
                    //double[,] cwtMatrix = cwt.GetScaleTimeMatrix();
                    //oscillationsSpectrum = GetOscillationArrayUsingCWT(cwtMatrix, sensitivity, bin);
                    //double[] dynamicRanges = GetVectorOfDynamicRanges(freqBin, sampleLength);
                }

                // transfer final oscillation vector for single frequency bin to the Oscillations by frequency matrix.
                MatrixTools.SetColumn(freqByOscMatrix, bin, oscillationsSpectrum);
            } // foreach frequency bin

            return freqByOscMatrix;
        }

        /// <summary>
        /// Returns a matrix whose columns consist of autocorrelations of freq bin samples.
        /// The columns are non-overlapping.
        /// </summary>
        /// <param name="signal">an array corresponding to one frequency bin</param>
        /// <param name="sampleLength">the length of a sample or patch (non-overllapping) for which xcerrelation is obtained</param>
        public static double[,] GetXcorrByTimeMatrix(double[] signal, int sampleLength)
        {
            // NormaliseMatrixValues freq bin values to z-score. This is required else get spurious results
            signal = DataTools.Vector2Zscores(signal);

            // get number of complete non-overlapping samples or patches
            var sampleCount = signal.Length / sampleLength;
            var xCorrelationsByTime = new double[sampleLength, sampleCount];

            for (var s = 0; s < sampleCount; s++)
            {
                var start = s * sampleLength;
                var subArray = DataTools.Subarray(signal, start, sampleLength);

                // do xcorr which returns an array same length as the sample or patch.
                var autocor = AutoAndCrossCorrelation.AutoCorrelationOldJavaVersion(subArray);

                //DataTools.writeBarGraph(autocor);
                MatrixTools.SetColumn(xCorrelationsByTime, s, autocor);
            }

            // return a matrix of [xCorrLength, sampleLength]
            return xCorrelationsByTime;
        }

        /// <summary>
        ///  reduces the sequence of Xcorrelation vectors to a single summary vector.
        ///  Does this by:
        ///  (1) do SVD on the collection of XCORRELATION vectors
        ///  (2) select the dominant ones based on the eigen values - 90% threshold
        ///      Typically there are 1 to 10 eigen values depending on how busy the bin is.
        ///  (3) Do an FFT on each of the returned SVD vectors to pick the dominant oscillation rate.
        ///  (4) Accumulate the oscillations in a freq by oscillation rate matrix.
        ///      The amplitude value for the oscillation is the eigenvalue.
        /// #
        ///  NOTE: There should only be one dominant oscillation in any one freq band at one time.
        ///        Birds with oscillating calls do call simultaneously, but this technique will only pick up the dominant call.
        /// #
        /// </summary>
        /// <param name="xCorrByTimeMatrix">double[,] xCorrelationsByTime = new double[sampleLength, sampleCount]; </param>
        /// <param name="sensitivity">can't remember what this does</param>
        /// <param name="binNumber">only used when debugging</param>
        public static double[] GetOscillationArrayUsingSvdAndFft(double[,] xCorrByTimeMatrix, double sensitivity, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);

            // int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // do singular value decomp on the xcorrelation vectors.
            // we want to compute the U and V matrices of singular vectors.
            //var svd = DenseMatrix.OfArray(xCorrByTimeMatrix).Svd(true);
            var svd = DenseMatrix.OfArray(xCorrByTimeMatrix).Svd();

            // svd.S returns the singular values in a vector
            Vector<double> singularValues = svd.S;

            // get total energy in first singular values
            double energySum = 0.0;
            foreach (double v in singularValues)
            {
                energySum += v * v;
            }

            // get the 90% most significant ####### THis is a significant parameter but not critical. 90% is OK
            double significanceThreshold = 0.9;
            double energy = 0.0;
            int countOfSignificantSingularValues = 0;
            for (int n = 0; n < singularValues.Count; n++)
            {
                energy += singularValues[n] * singularValues[n];
                double fraction = energy / energySum;
                if (fraction > significanceThreshold)
                {
                    countOfSignificantSingularValues = n + 1;
                    break;
                }
            }

            //foreach (double d in singularValues)
            //    Console.WriteLine("singular value = {0}", d);
            //Console.WriteLine("Freq bin:{0}  Count Of Significant SingularValues = {1}", binNumber, countOfSignificantSingularValues);

            // svd.U returns the LEFT singular vectors in matrix
            Matrix<double> uMatrix = svd.U;

            //Matrix<double> relevantU = UMatrix.SubMatrix(0, UMatrix.RowCount-1, 0, eigenVectorCount);

            //Console.WriteLine("\n\n");
            //MatrixTools.writeMatrix(UMatrix.ToArray());
            //string pathUmatrix1 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U1.png";
            //ImageTools.DrawReversedMDNMatrix(UMatrix, pathUmatrix1);
            //string pathUmatrix2 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U2.png";
            //ImageTools.DrawReversedMDNMatrix(relevantU, pathUmatrix2);

            // loop over the singular values and
            // transfer data from SVD.UMatrix to a single vector of oscilation values
            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < countOfSignificantSingularValues; e++)
            {
                double[] autocor = uMatrix.Column(e).ToArray();

                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                {
                    for (int i = 0; i < autocor.Length; i++)
                    {
                        autocor[i] *= -1.0;
                    }
                }

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.66;

                spectrum = DataTools.SquareValues(spectrum);

                // get relative power in the three bins around max.
                double sumOfSquares = spectrum.Sum();
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];

                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[1] + spectrum[2];
                }
                else if (maxIndex >= spectrum.Length - 1)
                {
                    powerAtMax += spectrum[maxIndex - 1] + spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1] + spectrum[maxIndex + 1];
                }

                double relativePower = powerAtMax / sumOfSquares;

                // if the relative power of the max oscillation is large enough,
                // then accumulate its power into the oscillationsVector
                if (relativePower > sensitivity)
                {
                    oscillationsVector[maxIndex] += powerAtMax;
                }
            }

            return LogTransformOscillationVector(oscillationsVector, countOfSignificantSingularValues);
        }

        /// <summary>
        /// returns an oscillation array for a single frequency bin
        /// </summary>
        /// <param name="xCorrByTimeMatrix">derived from single frequency bin</param>
        /// <param name="sensitivity">a threshold used to ignore low ascillation intensities</param>
        /// <returns>vector of oscillation values</returns>
        public static double[] GetOscillationArrayUsingFft(double[,] xCorrByTimeMatrix, double sensitivity)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // set up vector to contain fft output
            var oscillationsVector = new double[xCorrLength / 2];

            // loop over all the Auto-correlation vectors and do FFT
            for (int e = 0; e < sampleCount; e++)
            {
                double[] autocor = MatrixTools.GetColumn(xCorrByTimeMatrix, e);

                // zero mean the auto-correlation vector before doing FFT
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in the low coeff because these can dominate.
                // This is a hack!
                spectrum[0] *= 0.66;

                // convert to energy and calculate total power in spectrum
                spectrum = DataTools.SquareValues(spectrum);
                double sumOfSquares = spectrum.Sum();

                // get combined relative power in the three bins centred on max.
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];

                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[1] + spectrum[2];
                }
                else if (maxIndex >= spectrum.Length - 1)
                {
                    powerAtMax += spectrum[maxIndex - 1] + spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1] + spectrum[maxIndex + 1];
                }

                double relativePower = powerAtMax / sumOfSquares;

                // if the relative power of the max oscillation is large enough,
                // then accumulate its power into the oscillations Vector
                if (relativePower > sensitivity)
                {
                    oscillationsVector[maxIndex] += powerAtMax;
                }
            }

            return LogTransformOscillationVector(oscillationsVector, sampleCount);
        }

        public static double[] GetOscillationArrayUsingWpd(double[,] xCorrByTimeMatrix, double sensitivity, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < sampleCount; e++)
            {
                var autocor = MatrixTools.GetColumn(xCorrByTimeMatrix, e);
                autocor = DataTools.DiffFromMean(autocor);
                var wpd = new WaveletPacketDecomposition(autocor);
                double[] spectrum = wpd.GetWPDEnergySpectrumWithoutDC();

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.66;

                spectrum = DataTools.SquareValues(spectrum);

                // get relative power in the three bins around max.
                double sumOfSquares = spectrum.Sum();

                //double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1];
                }

                if (maxIndex >= spectrum.Length - 1)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex + 1];
                }

                double relativePower1 = powerAtMax / sumOfSquares;

                if (relativePower1 > sensitivity)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        oscillationsVector[maxIndex] += powerAtMax;
                    }
                }
            }

            return LogTransformOscillationVector(oscillationsVector, sampleCount);
        }

        public static double[] GetOscillationArrayUsingCwt(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);

            //int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // loop over the singular values and
            // transfer data from SVD.UMatrix to a single vector of oscilation values
            var oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < 10; e++)
            {
                var autocor = new double[xCorrLength];

                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                {
                    for (int i = 0; i < autocor.Length; i++)
                    {
                        autocor[i] *= -1.0;
                    }
                }

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.5;

                spectrum = DataTools.SquareValues(spectrum);
                double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];

                //double relativePower1 = powerAtMax / sumOfSquares;
                double relativePower2 = powerAtMax / avPower;

                //if (relativePower1 > 0.05)
                if (relativePower2 > 10.0)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        //oscillationsVector[maxIndex] += powerAtMax;
                        oscillationsVector[maxIndex] += relativePower2;
                    }
                }
            }

            for (int i = 0; i < oscillationsVector.Length; i++)
            {
                // NormaliseMatrixValues by sample count
                //oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0)
                {
                    oscillationsVector[i] = 0.0;
                }
                else
                {
                    oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
                }
            }

            return oscillationsVector;
        }

        public static double[] LogTransformOscillationVector(double[] vector, int sampleCount)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                // average the power values over all samples
                vector[i] /= sampleCount;

                // do log transform
                if (vector[i] < 0.1)
                {
                    vector[i] = 0.0;
                }
                else
                {
                    vector[i] = Math.Log10(1 + vector[i]);
                }
            }

            return vector;
        }

        public static double[] GetSpectralIndex_Osc(FileInfo sourceRecording, Dictionary<string, string> configDict)
        {
            var sensitivity = double.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);

            // Sample length i.e. number of frames spanned to calculate oscillations per second
            int sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);

            // set up the default songram config object
            var sonoConfig = new SonogramConfig(configDict)
            {
                WindowSize = int.Parse(configDict[AnalysisKeys.FrameLength]),
            };

            var recordingSegment = new AudioRecording(sourceRecording.FullName);
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);

            // Taking the square-root emphasizes the low amplitude features.
            // Could omit this but it gives fewer detections of oscillations
            sonogram.Data = MatrixTools.SquareRootOfValues(sonogram.Data);

            // remove the DC bin if it has not already been removed.
            // Assume test of divisible by 2 is good enough.
            int binCount = sonogram.Data.GetLength(1);
            if (!binCount.IsEven())
            {
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, binCount - 1);
            }

            var freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, "autocorr-fft");

            //NOTE: Generate an oscillations spectral index for making LDFC spectrograms by taking the max of each column
            var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix);
            return spectralIndex;
        }

        /// <summary>
        /// returns oscillations using the DCT
        /// </summary>
        public static void GetOscillationUsingDct(double[] array, double framesPerSecond, double[,] cosines, out double oscilFreq, out double period, out double intenisty)
        {
            var modifiedArray = DataTools.SubtractMean(array);
            var dctCoeff = MFCCStuff.DCT(modifiedArray, cosines);

            // convert to absolute values because not interested in negative values due to phase.
            for (int i = 0; i < dctCoeff.Length; i++)
            {
                dctCoeff[i] = Math.Abs(dctCoeff[i]);
            }

            // remove low freq oscillations from consideration
            int thresholdIndex = dctCoeff.Length / 5;
            for (int i = 0; i < thresholdIndex; i++)
            {
                dctCoeff[i] = 0.0;
            }

            dctCoeff = DataTools.normalise2UnitLength(dctCoeff);

            //dct = DataTools.NormaliseMatrixValues(dctCoeff); //another option to NormaliseMatrixValues
            int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);

            //recalculate DCT duration in seconds
            double dctDuration = dctCoeff.Length / framesPerSecond;
            oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
            period = 2 * dctCoeff.Length / (double)indexOfMaxValue / framesPerSecond; //convert maxID to period in seconds
            intenisty = dctCoeff[indexOfMaxValue];
        }

        private static Image DrawTitleBarOfOscillationSpectrogram(string algorithmName, int width)
        {
            var longTitle = "Hz * Cycle/s (" + algorithmName + ")";

            var bmp = new Bitmap(width, 20);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            var stringFont = new Font("Arial", 9);
            g.DrawString(longTitle, stringFont, Brushes.Wheat, new PointF(3, 3));
            return bmp;
        }
    }
}

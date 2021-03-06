// <copyright file="PcaWhiteningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System.IO;
    using Accord.Math;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using TestHelpers;
    using Path = System.IO.Path;

    [TestClass]
    public class PcaWhiteningTests
    {

        private DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        /// METHOD TO CHECK IF Default PCA WHITENING IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void PcaWhiteningDefault()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");

            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);
            var recording = new AudioRecording(recordingPath);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // GENERATE AMPLITUDE SPECTROGRAM
            var spectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            spectrogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO RMS NORMALIZATION
            spectrogram.Data = SNR.RmsNormalization(spectrogram.Data);

            // CONVERT NORMALIZED AMPLITUDE SPECTROGRAM TO dB SPECTROGRAM
            var sonogram = new SpectrogramStandard(spectrogram);

            // DO NOISE REDUCTION
            var dataMatrix = PcaWhitening.NoiseReduction(sonogram.Data);
            sonogram.Data = dataMatrix;

            // DO PCA WHITENING
            var whitenedSpectrogram = PcaWhitening.Whitening(true, sonogram.Data);

            // DO UNIT TESTING
            // check if the dimensions of the reverted spectrogram (second output of the pca whitening) is equal to the input matrix
            Assert.AreEqual(whitenedSpectrogram.Reversion.GetLength(0), sonogram.Data.GetLength(0));
            Assert.AreEqual(whitenedSpectrogram.Reversion.GetLength(1), sonogram.Data.GetLength(1));
        }

        /// <summary>
        /// METHOD TO CHECK IF RECONSTRUCTING SPECTROGRAM AFTER APPLYING PATCH SAMPLING AND PCA WHITENING IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void TestPcaWhitening()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "ReconstrcutedSpectrogram.png");

            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);
            var recording = new AudioRecording(recordingPath);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // GENERATE AMPLITUDE SPECTROGRAM
            var spectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            spectrogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO RMS NORMALIZATION
            spectrogram.Data = SNR.RmsNormalization(spectrogram.Data);

            // CONVERT NORMALIZED AMPLITUDE SPECTROGRAM TO dB SPECTROGRAM
            var sonogram = new SpectrogramStandard(spectrogram);

            // DO NOISE REDUCTION
            var dataMatrix = PcaWhitening.NoiseReduction(sonogram.Data);
            sonogram.Data = dataMatrix;

            // Do Patch Sampling
            int rows = sonogram.Data.GetLength(0);
            int columns = sonogram.Data.GetLength(1);
            int patchWidth = columns;
            int patchHeight = 1;
            int numberOfPatches = (rows / patchHeight) * (columns / patchWidth);
            var sequentialPatches = PatchSampling.GetPatches(sonogram.Data, patchWidth, patchHeight, numberOfPatches, PatchSampling.SamplingMethod.Sequential);
            double[,] sequentialPatchMatrix = sequentialPatches.ToMatrix();

            // DO PCA WHITENING
            var whitenedSpectrogram = PcaWhitening.Whitening(true, sequentialPatchMatrix);

            // reconstructing the spectrogram from sequential patches and the projection matrix obtained from random patches
            var projectionMatrix = whitenedSpectrogram.ProjectionMatrix;
            var eigenVectors = whitenedSpectrogram.EigenVectors;
            var numComponents = whitenedSpectrogram.Components;
            double[,] reconstructedSpec = PcaWhitening.ReconstructSpectrogram(projectionMatrix, sequentialPatchMatrix, eigenVectors, numComponents);
            sonogram.Data = PatchSampling.ConvertPatches(reconstructedSpec, patchWidth, patchHeight, columns);
            var reconstructedSpecImage = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "RECONSTRUCTEDSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            reconstructedSpecImage.Save(outputImagePath);

            // DO UNIT TESTING
            Assert.AreEqual(spectrogram.Data.GetLength(0), sonogram.Data.GetLength(0));
            Assert.AreEqual(spectrogram.Data.GetLength(1), sonogram.Data.GetLength(1));
        }
    }
}

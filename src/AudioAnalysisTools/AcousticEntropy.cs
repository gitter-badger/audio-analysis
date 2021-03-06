﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticEntropy.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AcousticEntropy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using StandardSpectrograms;
    using TowseyLibrary;

    public static class AcousticEntropy
    {
        public static double[] CalculateTemporalEntropySpectrum(double[,] spectrogram)
        {
            // int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] tenSp = new double[freqBinCount];      // array of H[t] indices, one for each freq bin

            // for all frequency bins
            for (int j = 0; j < freqBinCount; j++)
            {
                double[] column = MatrixTools.GetColumn(spectrogram, j);

                // ENTROPY of freq bin
                tenSp[j] = DataTools.EntropyNormalised(DataTools.SquareValues(column));
            }

            return tenSp;
        }

        /// <summary>
        /// Calculates three SUMMARY INDICES - three different measures of spectral entropy.
        /// Each of them is derived from the frames of the passed amplitude spectrogram.
        /// 1. the entropy of the average spectrum.
        /// 2. the entropy of the variance spectrum.
        /// 3. the entropy of the Coeff of Variation spectrum.
        /// </summary>
        /// <param name="amplitudeSpectrogram">matrix</param>
        /// <param name="lowerBinBound">lower bin bound to be included in calculation of summary index</param>
        /// <param name="reducedFreqBinCount">total bin count to be included in calculation of summary index</param>
        /// <returns>two doubles</returns>
        public static Tuple<double, double, double> CalculateSpectralEntropies(double[,] amplitudeSpectrogram, int lowerBinBound, int reducedFreqBinCount)
        {
            // iv: ENTROPY OF AVERAGE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            // Entropy is a measure of ENERGY dispersal, therefore must square the amplitude.
            var tuple = SpectrogramTools.CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram(amplitudeSpectrogram);
            double[] averageSpectrum = DataTools.Subarray(tuple.Item1, lowerBinBound, reducedFreqBinCount); // remove low band
            double entropyOfAvSpectrum = DataTools.EntropyNormalised(averageSpectrum); // ENTROPY of spectral averages
            if (double.IsNaN(entropyOfAvSpectrum))
            {
                entropyOfAvSpectrum = 1.0;
            }

            // v: ENTROPY OF VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            double[] varianceSpectrum = DataTools.Subarray(tuple.Item2, lowerBinBound, reducedFreqBinCount); // remove low band
            double entropyOfVarianceSpectrum = DataTools.EntropyNormalised(varianceSpectrum);      // ENTROPY of spectral variances
            if (double.IsNaN(entropyOfVarianceSpectrum))
            {
                entropyOfVarianceSpectrum = 1.0;
            }

            // vi: ENTROPY OF COEFFICIENT OF VARIANCE SPECTRUM
            int covLength = varianceSpectrum.Length;
            double[] coeffOfVarSpectrum = new double[covLength];     // remove low band
            for (int i = 0; i < covLength; i++)
            {
                if (averageSpectrum[i] > 0.0)
                {
                    coeffOfVarSpectrum[i] = varianceSpectrum[i] / averageSpectrum[i];
                }
                else
                {
                    coeffOfVarSpectrum[i] = 1.0;
                }
            }

            double entropyOfCoeffOfVarSpectrum = DataTools.EntropyNormalised(coeffOfVarSpectrum);   // ENTROPY of Coeff Of Variance spectrum
            if (double.IsNaN(entropyOfVarianceSpectrum))
            {
                entropyOfCoeffOfVarSpectrum = 1.0;
            }

            // DataTools.writeBarGraph(indices.varianceSpectrum);
            // Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);

            return Tuple.Create(entropyOfAvSpectrum, entropyOfVarianceSpectrum, entropyOfCoeffOfVarSpectrum);
        } // CalculateSpectralEntropies()

        /// <summary>
        /// CALCULATES THE ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
        /// Only spectral peaks between the lowerBinBound and the upperBinBound will be included in calculation.
        /// </summary>
        public static double CalculateEntropyOfSpectralPeaks(double[,] amplitudeSpectrogram, int lowerBinBound, int upperBinBound)
        {
            //     First extract High band SPECTROGRAM which is now noise reduced
            var midBandSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, upperBinBound - 1);
            var tupleAmplitudePeaks = SpectrogramTools.HistogramOfSpectralPeaks(midBandSpectrogram);
            double entropyOfPeakFreqDistr = DataTools.EntropyNormalised(tupleAmplitudePeaks.Item1);
            if (double.IsNaN(entropyOfPeakFreqDistr))
            {
                entropyOfPeakFreqDistr = 1.0;
            }

            return entropyOfPeakFreqDistr;
        } // CalculateEntropyOfSpectralPeaks()
    } // class AcousticEntropy
}

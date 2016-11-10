﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UperoleiaLithomoda.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// This is a frog recognizer based on the "trill", "ribit" or "washboard" template
    /// It detects trill type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    /// 
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs 
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// 
    /// </summary>
    class UperoleiaLithomoda : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "UperoleiaLithomoda";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration,
            TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            string speciesName = (string) configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string) configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            int minHz = (int) configuration[AnalysisKeys.MinHz];
            int maxHz = (int) configuration[AnalysisKeys.MaxHz];

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds 
            double dctDuration = (double) configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = (double) configuration[AnalysisKeys.DctThreshold];

            // ignore oscillations below this threshold freq
            int minOscilFreq = (int) configuration[AnalysisKeys.MinOscilFreq];

            // ignore oscillations above this threshold freq
            int maxOscilFreq = (int) configuration[AnalysisKeys.MaxOscilFreq];

            // min duration of event in seconds 
            double minDuration = (double) configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds                 
            double maxDuration = (double) configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            double eventThreshold = (double) configuration[AnalysisKeys.EventThreshold];

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            // The default was 512 for Canetoad.
            // Framesize = 128 seems to work for Littoria fallax.
            const int frameSize = 128;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                maxOscilFreq);
            //windowOverlap = 0.75; // previous default

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.1
            };

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            var recordingDuration = recording.Duration();
            //int sr = recording.SampleRate;
            //double freqBinWidth = sr/(double) sonoConfig.WindowSize;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            // double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // This window is used to smooth the score array before extracting events.
            // A short window (e.g. 3) preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 13;
            double[] scores; // predefinition of score array
            List<AcousticEvent> acousticEvents;
            double[,] hits;
            Oscillations2012.Execute(
                (SpectrogramStandard) sonogram,
                minHz,
                maxHz,
                dctDuration,
                minOscilFreq,
                maxOscilFreq,
                dctThreshold,
                eventThreshold,
                minDuration,
                maxDuration,
                scoreSmoothingWindow,
                out scores,
                out acousticEvents,
                out hits);

            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.SegmentDuration = recordingDuration;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot> {plot};


            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plots,
                Events = acousticEvents
            };

        }
    }


    internal class UperoleiaLithomodaConfig
    {
        public string AnalysisName { get; set; }
        public string SpeciesName { get; set; }
        public string AbbreviatedSpeciesName { get; set; }
        public int MinHz { get; set; }
        public int MaxHz { get; set; }
        public double DctDuration { get; set; }
        public double DctThreshold { get; set; }
        public double MinPeriod { get; set; }
        public double MaxPeriod { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
        public double EventThreshold { get; set; }

        internal void ReadConfigFile(dynamic configuration)
        {
            // common properties
            AnalysisName = (string)configuration[AnalysisKeys.AnalysisName] ?? "<no name>";
            SpeciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            AbbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            // frequency band of the call
            MinHz = (int)configuration[AnalysisKeys.MinHz];
            MaxHz = (int)configuration[AnalysisKeys.MaxHz];

            // duration of DCT in seconds 
            DctDuration = (double)configuration[AnalysisKeys.DctDuration];
            // minimum acceptable value of a DCT coefficient
            DctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            MinPeriod = configuration["MinInterval"];
            MaxPeriod = configuration["MaxInterval"];

            // min and max duration of event in seconds 
            MinDuration = (double)configuration[AnalysisKeys.MinDuration];
            MaxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            EventThreshold = (double)configuration[AnalysisKeys.EventThreshold];
        }

    } // Config class



}

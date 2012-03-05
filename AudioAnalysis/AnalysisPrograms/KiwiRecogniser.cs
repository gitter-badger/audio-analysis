﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioTools.AudioUtlity;
using QutSensors.Shared.LogProviders;




//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture

//HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE
//od  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\OD_CaneToad\CaneToad_DetectionParams.txt events.txt
//


namespace AnalysisPrograms
{
    class KiwiRecogniser
    {
        //Following lines are used for the debug command line.
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"


        

        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT        = "FILE_EXT";
        //public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_SEGMENT_DURATION  = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP   = "SEGMENT_OVERLAP";
        
        public static string key_FRAME_LENGTH    = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_MIN_HZ_MALE     = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE     = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE   = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE   = "MAX_HZ_FEMALE";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_DCT_THRESHOLD   = "DCT_THRESHOLD";
        public static string key_MIN_PERIODICITY = "MIN_PERIODICITY";
        public static string key_MAX_PERIODICITY = "MAX_PERIODICITY";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";
        public static string key_REPORT_FORMAT   = "REPORT_FORMAT";


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct KiwiParams
        {
            public int frameLength, minHzMale, maxHzMale, minHzFemale, maxHzFemale;
            public double segmentDuration, segmentOverlap; 
            public double frameOverlap, dctDuration, dctThreshold, minPeriodicity, maxPeriodicity, minDuration, maxDuration, eventThreshold;
            public int DRAW_SONOGRAMS;
            public string reportFormat;

            public KiwiParams(double _segmentDuration, double _segmentOverlap, 
                              int _minHzMale, int _maxHzMale, int _minHzFemale, int _maxHzFemale, int _frameLength, int _frameOverlap, double _dctDuration, double _dctThreshold,
                              double _minPeriodicity, double _maxPeriodicity, double _minDuration, double _maxDuration, double _eventThreshold, 
                              int _DRAW_SONOGRAMS, string _fileFormat)
            {
                segmentDuration = _segmentDuration;
                segmentOverlap  = _segmentOverlap;
                minHzMale = _minHzMale;
                maxHzMale = _maxHzMale;
                minHzFemale = _minHzFemale;
                maxHzFemale = _maxHzFemale;
                frameLength = _frameLength;
                frameOverlap = _frameOverlap;
                dctDuration = _dctDuration;
                dctThreshold = _dctThreshold;
                minPeriodicity = _minPeriodicity;
                maxPeriodicity = _maxPeriodicity;
                minDuration    = _minDuration;
                maxDuration    = _maxDuration;
                eventThreshold = _eventThreshold;
                DRAW_SONOGRAMS = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
                reportFormat   = _fileFormat;
            }
        }





        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO DETECT CALLS OF THE LITTLE SPOTTED KIWI (Apteryx owenii)";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            Log.Verbosity = 1;
            CheckArguments(args);
            string recordingPath = args[0];
            string iniPath   = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.
            string segmentPath = Path.Combine(outputDir, "temp.wav"); //path location/name of extracted recording segment
            Log.WriteIfVerbose("# Output dir: " + outputDir);
                       

            //READ PARAMETER VALUES FROM INI FILE
            KiwiParams kiwiParams = ReadIniFile(iniPath);

            // Get the file time duration
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(recordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(recordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fileInfo.CreationTime.ToLongDateString(), fileInfo.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);

            //SET UP THE REPORT FILE
            string reportfileName = outputDir + "LSKReport_" + Path.GetFileNameWithoutExtension(recordingPath);
            if (kiwiParams.reportFormat.Equals("CSV")) reportfileName += ".csv";
            else reportfileName += ".txt";
            WriteHeaderToFile(reportfileName, kiwiParams.reportFormat);

            // LOOP THROUGH THE FILE
            double startMinutes = 0.0;
            int overlap_ms = (int)Math.Floor(kiwiParams.segmentOverlap * 1000);
            for (int s = 0; s < Int32.MaxValue; s++)
            {
                Console.WriteLine();
                Log.WriteLine("## SAMPLE {0}:-   starts@ {1} minutes", s, startMinutes);

                int resampleRate = 17640;
                int startMilliseconds = (int)(startMinutes * 60000);
                int endMilliseconds = startMilliseconds + (int)(kiwiParams.segmentDuration * 60000) + overlap_ms;
                AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(recordingPath, startMilliseconds, endMilliseconds, resampleRate, segmentPath);
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(recording.GetWavReader().Time.TotalSeconds);
                //Log.WriteLine("Signal Duration: " + segmentDuration);
                int sampleCount = recording.GetWavReader().Samples.Length;
                int minLength = 3 * kiwiParams.frameLength;
                if (sampleCount <= minLength)
                {
                    Log.WriteLine("# WARNING: Recording is less than {0} samples (three frames) long. Will ignore.", sampleCount);
                    //Console.ReadLine();
                    //System.Environment.Exit(666);
                    break;
                }

                //#############################################################################################################################################
                var results = Execute_KiwiDetect(recording, kiwiParams.minHzMale, kiwiParams.maxHzMale, kiwiParams.minHzFemale, kiwiParams.maxHzFemale,
                                                 kiwiParams.frameLength, kiwiParams.frameOverlap,
                                                 kiwiParams.dctDuration, kiwiParams.dctThreshold, kiwiParams.minPeriodicity, kiwiParams.maxPeriodicity,
                                                 kiwiParams.eventThreshold, kiwiParams.minDuration, kiwiParams.maxDuration);
                //#############################################################################################################################################

                recording.Dispose();
                var sonogram = results.Item1;
                var hits = results.Item2;
                var scores = results.Item3;
                //var oscRates = results.Item4;
                var predictedEvents = results.Item5;
                Log.WriteLine("# Event count = " + predictedEvents.Count());

                //write events to results file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = Path.GetFileName(recordingPath);
                int count = predictedEvents.Count;

                StringBuilder sb = KiwiRecogniser.WriteEvents(startMinutes, sigDuration, count, predictedEvents, kiwiParams.reportFormat);
                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 2, 2); //remove the last endLine to prevent line gaps.
                    FileTools.Append2TextFile(reportfileName, sb.ToString());
                }


                //draw images of sonograms
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_" + startMinutes.ToString() + "min.png";
                if ((kiwiParams.DRAW_SONOGRAMS == 2) ||((kiwiParams.DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0)))
                {
                    DrawSonogram(sonogram, imagePath, hits, scores, null, predictedEvents, kiwiParams.eventThreshold);
                }

                startMinutes += kiwiParams.segmentDuration;
            } //end of for loop

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static KiwiParams ReadIniFile(string iniPath)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            KiwiParams kiwiParams; // st
            kiwiParams.segmentDuration = Double.Parse(dict[key_SEGMENT_DURATION]);
            kiwiParams.segmentOverlap  = Double.Parse(dict[key_SEGMENT_OVERLAP]);
            kiwiParams.minHzMale = Int32.Parse(dict[key_MIN_HZ_MALE]);
            kiwiParams.maxHzMale = Int32.Parse(dict[key_MAX_HZ_MALE]);
            kiwiParams.minHzFemale = Int32.Parse(dict[key_MIN_HZ_FEMALE]);
            kiwiParams.maxHzFemale = Int32.Parse(dict[key_MAX_HZ_FEMALE]);
            kiwiParams.frameLength = Int32.Parse(dict[key_FRAME_LENGTH]);
            kiwiParams.frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            kiwiParams.dctDuration = Double.Parse(dict[key_DCT_DURATION]);        //duration of DCT in seconds 
            kiwiParams.dctThreshold = Double.Parse(dict[key_DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            kiwiParams.minPeriodicity = Double.Parse(dict[key_MIN_PERIODICITY]);  //ignore oscillations with period below this threshold
            kiwiParams.maxPeriodicity = Double.Parse(dict[key_MAX_PERIODICITY]);  //ignore oscillations with period above this threshold
            kiwiParams.minDuration = Double.Parse(dict[key_MIN_DURATION]);        //min duration of event in seconds 
            kiwiParams.maxDuration = Double.Parse(dict[key_MAX_DURATION]);        //max duration of event in seconds 
            kiwiParams.eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);  //min score for an acceptable event
            kiwiParams.DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram
            kiwiParams.reportFormat   = dict[key_REPORT_FORMAT];                  //options are TAB or COMMA separator 

            Log.WriteIfVerbose("# PARAMETER SETTINGS:");
            Log.WriteIfVerbose("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", kiwiParams.segmentDuration, kiwiParams.segmentOverlap);
            Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzMale, kiwiParams.maxHzMale);
            Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzFemale, kiwiParams.maxHzFemale);
            Log.WriteIfVerbose("Periodicity bounds: {0:f1}sec - {1:f1}sec", kiwiParams.minPeriodicity, kiwiParams.maxPeriodicity);
            Log.WriteIfVerbose("minAmplitude = " + kiwiParams.dctThreshold);
            Log.WriteIfVerbose("Duration bounds: " + kiwiParams.minDuration + " - " + kiwiParams.maxDuration + " seconds");
            Log.WriteIfVerbose("####################################################################################");
            //Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzMale, maxHzMale, binCount_male);
            //Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzFemale, maxHzFemale, binCount_female);
            //Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            //Log.WriteIfVerbose("Score threshold for oscil events=" + eventThreshold);
            return kiwiParams;
        }




        public static System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>> Execute_KiwiDetect(AudioRecording recording,
            int minHzMale, int maxHzMale, int minHzFemale, int maxHzFemale, int frameLength, double frameOverlap, double dctDuration, double dctThreshold,
            double minPeriodicity, double maxPeriodicity, double eventThreshold, double minDuration, double maxDuration)
        {
            //i: MAKE SONOGRAM
            //Log.WriteLine("Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = frameLength;
            sonoConfig.WindowOverlap  = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //double dB_threshold = 4.0; //threshold for 2D background noise removal
            //var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, dB_threshold);
            //double[,] noiseReducedMatrix = tuple.Item1; 

            //iii: DETECT OSCILLATIONS
            bool normaliseDCT = true;
            double minOscilFreq = 1 / maxPeriodicity;  //convert max period (seconds) to oscilation rate (Herz).
            double maxOscilFreq = 1 / minPeriodicity;  //convert min period (seconds) to oscilation rate (Herz).

            //ii: CHECK FOR MALE KIWIS
            int gapThreshold = 2;                     //merge events that are closer than 2 seconds
            List<AcousticEvent> predictedMaleEvents;  //predefinition of results event list
            Double[,] maleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] maleScores;                      //predefinition of score array
            double[] maleOscRate;
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzMale, maxHzMale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out maleScores, out predictedMaleEvents, out maleHits, out maleOscRate);
            if (predictedMaleEvents.Count > 0)
            {
                ProcessKiwiEvents(predictedMaleEvents, "Male LSK", maleOscRate, minDuration, maxDuration, sonogram.Data);
                //AcousticEvent.MergeAdjacentEvents(predictedMaleEvents, gapThreshold);
            }

            //iii: CHECK FOR FEMALE KIWIS
            Double[,] femaleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] femaleScores;                      //predefinition of score array
            double[] femaleOscRate;
            List<AcousticEvent> predictedFemaleEvents;  //predefinition of results event list
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzFemale, maxHzFemale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out femaleScores, out predictedFemaleEvents, out femaleHits, out femaleOscRate);
            if (predictedFemaleEvents.Count > 0)
            {
                ProcessKiwiEvents(predictedFemaleEvents, "Female LSK", femaleOscRate, minDuration, maxDuration, sonogram.Data);
                //AcousticEvent.MergeAdjacentEvents(predictedFemaleEvents, gapThreshold);
            }


            //iv: MERGE MALE AND FEMALE INFO
            foreach (AcousticEvent ae in predictedFemaleEvents) predictedMaleEvents.Add(ae);
            // Merge the male and female hit matrices. Each hit matrix shows where there is an oscillation of sufficient amplitude in the correct range.
            // Values in the matrix are the oscillation rate. i.e. if OR = 2.0 = 2 oscillations per second. </param>
            Double[,] hits = DataTools.AddMatrices(maleHits, femaleHits);
            //merge the two score arrays
            for (int i = 0; i < maleScores.Length; i++) if (femaleScores[i] > maleScores[i]) maleScores[i] = femaleScores[i];


            return System.Tuple.Create(sonogram, hits, maleScores, maleOscRate, predictedMaleEvents);
        } //end Execute_KiwiDetect()


        public static void ProcessKiwiEvents(List<AcousticEvent> events, string tag, double[] oscRate, double minDuration, double maxDuration, double[,] sonogramMatrix)
        {
            double[] band_dB = new double[sonogramMatrix.GetLength(0)];
            int eventCol1   = events[0].oblong.c1; 
            int eventHeight = events[0].oblong.ColWidth; //assume all events in the list have the same event height
            for (int r = 0; r < band_dB.Length; r++)
            {
                for (int c = 0; c < eventHeight; c++) band_dB[r] += sonogramMatrix[r, eventCol1 + c]; //event dB profile
            }
            for (int r = 0; r < band_dB.Length; r++) band_dB[r] /= eventHeight; //calculate average.
            
            //calculate background noise and SD for the band
            double minDecibels = -110; //min dB to consider
            double noiseThreshold_dB = 20;
            double min_dB, max_dB, modalNoise, SD_Noise;
            SNR.CalculateModalNoise(band_dB, minDecibels, noiseThreshold_dB, out min_dB, out max_dB, out modalNoise, out SD_Noise);

            foreach (AcousticEvent ae in events)
            {
                ae.Name = tag;
                int eventLength = (int)(ae.Duration * ae.FramesPerSecond);
                //int objHt = ae.oblong.RowWidth;
                //Console.WriteLine("{0}    {1} = {2}-{3}", eventLength, objHt, ae.oblong.r2, ae.oblong.r1);

                //1: calculate score for duration. Value lies in [0,1]. Shape the ends.
                double durationScore = 1.0;
                if (ae.Duration < minDuration +  5) durationScore = (ae.Duration - minDuration) / 5;
                else
                if (ae.Duration > maxDuration - 10) durationScore = (maxDuration - ae.Duration) / 10;

                //2:  %hit score = ae.
                double hitScore = ae.Score;//fraction of bins where oscillation was detected

                //3: calculate score for bandwidth of syllables
                double bandWidthScore = CalculateKiwiBandWidthScore(ae, sonogramMatrix);

                //4: get decibels through the event   
                double[] event_dB = new double[eventLength]; //dB profile for event
                for (int r = 0; r < eventLength; r++) event_dB[r] = band_dB[ae.oblong.r1 + r]; //event dB profile
                //DataTools.writeBarGraph(event_dB);

                //5 : calculate score for snr and for change in inter-syllable distance over the event.
                var tuple = KiwiPeakAnalysis(event_dB, modalNoise, SD_Noise, ae.FramesPerSecond);
                double snrScore      = tuple.Item1;
                double sdPeakScore   = tuple.Item2;
                double gapScore      = tuple.Item3;
 
                //6: COMBINE SCORES
                ae.Score = (hitScore * 0.1) + (snrScore * 0.1) + (sdPeakScore * 0.2) + (gapScore * 0.2) + (bandWidthScore * 0.4); //weighted sum
                ae.ScoreNormalised     = ae.Score;
                ae.kiwi_durationScore  = durationScore;
                ae.kiwi_hitScore       = hitScore;
                ae.kiwi_snrScore       = snrScore;
                ae.kiwi_sdPeakScore    = sdPeakScore;
                ae.kiwi_gapScore       = gapScore;
                ae.kiwi_bandWidthScore = bandWidthScore;
            } //foreach (AcousticEvent ae in events)
        }//end method



        /// <summary>
        ///
        /// </summary>
        /// <param name="ae">an acoustic event</param>
        /// <param name="dbArray">The sequence of frame dB over the event</param>
        /// <returns></returns>
        public static System.Tuple<double, double, double> KiwiPeakAnalysis(double[] dbArray, double modalNoise, double SD_Noise, double framesPerSecond)
        {
            int smoothWindow = (int)Math.Ceiling(framesPerSecond * 0.25); //smoothing window of 1/4 second
            if (smoothWindow % 2 == 0) smoothWindow += 1; //make an odd number

            dbArray = DataTools.filterMovingAverage(dbArray, smoothWindow);

            bool[] peaks   = DataTools.GetPeaks(dbArray);

            //remove peaks below the threshold AND make list of peaks > threshold
            double dBThreshold = modalNoise + (2.0 * SD_Noise); //threshold = 2* sd of the background noise in the entire recording
            List<double> peakDB = new List<double>();
            for (int i = 0; i < dbArray.Length; i++)
            {
                if (dbArray[i] < dBThreshold) peaks[i] = false;
                else if (peaks[i]) peakDB.Add(dbArray[i]);
            }
            int peakCount = DataTools.CountTrues(peaks);
            if (peakCount == 0)
                return System.Tuple.Create(0.0, 0.0, 0.0); //no values can be calculated with zero-1 peaks

            //PROCESS PEAK DECIBELS
            double avPeakDB, sdPeakDB;
            NormalDist.AverageAndSD(peakDB.ToArray(), out avPeakDB, out sdPeakDB);
            double snr_dB = avPeakDB - modalNoise;
            //normalise the snr score
            double snrScore = 0.0;
            if (snr_dB > 9.0) snrScore = 1.0; else if (snr_dB > 1.0) snrScore = (snr_dB - 1) * 0.125; //two thresholds - 1 dB and 9 dB 
            //normalise the standard deviation of the peak decibels
            if (peakCount < 2)
                return System.Tuple.Create(snrScore, 0.0, 0.0); //no sd can be calculated with zero-1 peaks
            double sdPeakScore = 3 / sdPeakDB; //set 3 dB as a threshold
            if (sdPeakScore > 1.0) sdPeakScore = 1.0;

            //PROCESS PEAK GAPS
            if (peakCount < 3)
                return System.Tuple.Create(snrScore, sdPeakScore, 0.0); //no gaps can be calculated

            List<int> peakGaps = DataTools.GapLengths(peaks);
            int[] observedFrameGaps = peakGaps.ToArray();
            //double gapDelta = 0;
            //double gapPath = 0;
            double maxDelta_seconds = 0.5; //max permitted change in gap from one peak to next.
            int maxDelta = (int)Math.Ceiling(maxDelta_seconds / framesPerSecond);
            int correctDeltaCount = 0;
            int gapLength = observedFrameGaps[0];
            for (int i = 1; i < observedFrameGaps.Length; i++)
            {
                double gapDelta = (observedFrameGaps[i] - observedFrameGaps[i - 1]);
                if ((gapDelta >= 0) && (gapDelta <= maxDelta)) correctDeltaCount++;
                gapLength += observedFrameGaps[i];
                //gapDelta +=         (observedFrameGaps[i] - observedFrameGaps[i-1]);
                //gapPath  += Math.Abs(observedFrameGaps[i] - observedFrameGaps[i-1]);
            }
            //gapDelta /= (observedFrameGaps.Length - 1);    //convert to average
            //gapPath  /= (observedFrameGaps.Length - 1);    //convert to average
            //double gapScore = gapDelta / gapPath;
            //if (gapDelta <= 0.0) gapScore = 0.0; //gap delta for KIWIs must be positive
            double avGapLength_seconds = (gapLength / (double)observedFrameGaps.Length) / framesPerSecond;


            double gapScore = correctDeltaCount / (double)(observedFrameGaps.Length-2);
            if ((avGapLength_seconds < 0.2) || (avGapLength_seconds > 1.2)) gapScore = 0.0;
            //else gapScore = (gapScore-0.5) * 2; 

            return System.Tuple.Create(snrScore, sdPeakScore, gapScore);
        }


        /// <summary>
        /// Checkes that the passed acoustic event does not have acoustic activity that spills outside the kiwi bandwidth.
        /// </summary>
        /// <param name="ae"></param>
        /// <param name="sonogramMatrix"></param>
        /// <returns></returns>
        public static double CalculateKiwiBandWidthScore(AcousticEvent ae, double[,] sonogramMatrix)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            double[] upper_dB = new double[eventLength]; //dB profile for bandwidth above event
            double[] lower_dB = new double[eventLength]; //dB profile for bandwidth below event
            int eventHt = ae.oblong.ColWidth;
            int halfHt = eventHt / 2;
            int buffer = 20; //avoid this margin around the event
            //get acoustic activity within the event bandwidth and above it.
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHt; c++) event_dB[r] += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
                for (int c = 0; c < halfHt; c++) upper_dB[r]  += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c2 + c + buffer];
                for (int c = 0; c < halfHt; c++) lower_dB[r]  += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c1 - halfHt - buffer + c];
                //for (int c = 0; c < eventHt; c++) noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]     = 20.0; //mark matrix
                //for (int c = 0; c < eventHt; c++) noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c2 + 5 + c] = 40.0; //mark matrix
            }
            for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHt; //calculate average.
            for (int r = 0; r < eventLength; r++) upper_dB[r] /= halfHt;
            for (int r = 0; r < eventLength; r++) lower_dB[r] /= halfHt;

            //event_dB = DataTools.normalise(event_dB);
            //upper_dB = DataTools.normalise(upper_dB);

            double upperCC = DataTools.CorrelationCoefficient(event_dB, upper_dB);
            double lowerCC = DataTools.CorrelationCoefficient(event_dB, lower_dB);
            if (upperCC < 0.0) upperCC = 0.0;
            if (lowerCC < 0.0) lowerCC = 0.0;
            double CCscore = upperCC + lowerCC;
            if (CCscore > 1.0) CCscore = 1.0;
            return 1 - CCscore;
        }



        //##################################################################################################################################################
        //##################################################################################################################################################
        //##################################################################################################################################################
        //NEXT THREE METHODS NOT USED ANYMORE BUT MIGHT SCAVANGE CODE LATER


        /// <summary>
        /// calculates score for change in inter-syllable distance over the KIWI event
        /// </summary>
        /// <param name="ae">an acoustic event</param>
        /// <param name="oscRate"></param>
        /// <returns></returns>
        public static double CalculateKiwiDeltaISDscore(AcousticEvent ae, double[] oscRate)
        {

            //double[] ANDREW_KIWI_GAPS = {0.25,0.27,0.30,0.34,0.38,0.41,0.45,0.47,0.49,0.51,0.54,0.57,0.59,0.62,0.63,0.66,0.68,0.70,0.72,0.75,0.77,0.79,0.79,0.80,0.82,0.83,0.84,0.85};//seconds
            //double[] andrewFrameGaps = new double[ANDREW_KIWI_GAPS.Length];
            //for (int i = 0; i < ANDREW_KIWI_GAPS.Length; i++) andrewFrameGaps[i] = ANDREW_KIWI_GAPS[i] * framesPerSecond;
            //double avAndrewDelta_seconds = 0.0;
            //for (int i = 1; i < ANDREW_KIWI_GAPS.Length; i++) avAndrewDelta_seconds += (ANDREW_KIWI_GAPS[i] - ANDREW_KIWI_GAPS[i - 1]);
            //double avAndrewDelta_Frames = avAndrewDelta_seconds / (ANDREW_KIWI_GAPS.Length-1) * framesPerSecond;


            //double[] array = DataTools.Subarray(oscRate, ae.oblong.r1, ae.oblong.RowWidth);
            //DataTools.writeBarGraph(array);
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            int onetenth = eventLength / 10;
            int onefifth = eventLength / 5;
            int sevenTenth = eventLength * 7 / 10;
            int startOffset = ae.oblong.r1 + onetenth;
            int endOffset = ae.oblong.r1 + sevenTenth;
            double startISD = 0; //Inter-Syllable Distance in seconds
            double endISD = 0; //Inter-Syllable Distance in seconds
            for (int i = 0; i < onefifth; i++)
            {
                startISD += (1 / oscRate[startOffset + i]); //convert oscilation rates to inter-syllable distance i.e. periodicity.
                endISD += (1 / oscRate[endOffset + i]);
            }
            double deltaISD = (endISD - startISD) / onefifth; //get average change in inter-syllable distance
            double deltaScore = 0.0;
            if ((deltaISD >= -0.1) && (deltaISD <= 0.2)) deltaScore = (3.3333 * (deltaISD - 0.1)) + 0.3333;  //y=mx+c where c=0.333 and m=3.333
            else
                if (deltaISD > 0.2) deltaScore = 1.0;
            return deltaScore;
        }

        public static double CalculateKiwiEntropyScore(AcousticEvent ae, double[,] noiseReducedMatrix, double peakThreshold)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            int eventHeight = ae.oblong.ColWidth;
            //int halfHt = eventHt / 2;
            //get acoustic activity within the event bandwidth and above it.
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHeight; c++) event_dB[r] += noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
            }
            for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHeight; //calculate average.

            bool[] peaks = DataTools.GetPeaks(event_dB);
            //int peakCount = DataTools.CountTrues(peaks);
            //DataTools.writeBarGraph(event_dB);

            //for (int r = 0; r < eventLength; r++) if (event_dB[r] < peakThreshold) peaks[r] = false;
            int peakCount2 = DataTools.CountTrues(peaks);
            int expectedPeakCount = (int)(ae.Duration * 0.8); //calculate expected number of peaks given event duration
            //if (peakCount2 == 0) return 1.0;                //assume that energy is dispersed
            if (peakCount2 < expectedPeakCount) return 0.0;   //assume that energy is concentrated

            //set up histogram of peak energies
            double[] histogram = new double[peakCount2];
            int count = 0;
            for (int r = 0; r < eventLength; r++)
            {
                if (peaks[r])
                {
                    histogram[count] = event_dB[r];
                    count++;
                }
            }
            histogram = DataTools.NormaliseProbabilites(histogram);
            double normFactor = Math.Log(histogram.Length) / DataTools.ln2;  //normalize for length of the array
            double entropy = DataTools.Entropy(histogram) / normFactor;
            return entropy;
        }

        public static double CalculateKiwiPeakPeriodicityScore(AcousticEvent ae, double[,] noiseReducedMatrix, double peakThreshold)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            int eventHeight = ae.oblong.ColWidth;
            //get acoustic activity within the event bandwidth
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHeight; c++) event_dB[r] += noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
            }
            //for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHeight; //calculate average.

            event_dB = DataTools.filterMovingAverage(event_dB, 3);
            bool[] peaks = DataTools.GetPeaks(event_dB);


            //DataTools.writeBarGraph(event_dB);

            //for (int r = 0; r < eventLength; r++) if (event_dB[r] < peakThreshold) peaks[r] = false;
            //int peakCount2 = DataTools.CountTrues(peaks);
            //int expectedPeakCount = (int)ae.Duration;         //calculate expected number of peaks given event duration

            var tuple = DataTools.Periodicity_MeanAndSD(event_dB);
            double mean = tuple.Item1;
            double sd   = tuple.Item2;
            int peakCount = tuple.Item3;

            double score = 0.0;
            if (peakCount > (int)Math.Round(ae.Duration*1.2)) return score;

            double ratio = sd / mean;
            if (ratio < 0.333) score = 1.0;
            else if (ratio > 1.0) score = 0.0;
            else score = 1 - (ratio - 0.3) / 0.666;
            return score;
        }
        //ABOVE THREE METHODS NOT USED ANYMORE
        //##################################################################################################################################################
        //##################################################################################################################################################
        //##################################################################################################################################################


        public static void WriteHeaderToFile(string reportfileName, string parmasFile_Separator)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";        
            string line = String.Format("Start{0}Duration{0}__Label__{0}StartMin{0}StartSec{0}DurSec{0}MinHz{0}MaxHz{0}durSc{0}hitSc{0}snrSc{0}sdSc{0}gapSc{0}BWSc{0}WtScore", reportSeparator);
            FileTools.WriteTextFile(reportfileName, line);
        }


        public static StringBuilder WriteEvents(double segmentStart, double segmentDuration, int eventCount, List<AcousticEvent> eventList, string parmasFile_Separator)
        {

            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string duration = DataTools.Time_ConvertSecs2Mins(segmentDuration);
            StringBuilder sb = new StringBuilder();
            if (eventList.Count == 0)
            {
                //string line = String.Format("{1}{0}{2,8:f3}{0}0{0}N/A{0}N/A{0}N/A{0}N/A{0}N/A{0}0{0}0",
                //                     separator, segmentStart, duration);
                //sb.AppendLine(line);
            }
            else
            {
                foreach (AcousticEvent ae in eventList)
                {
                    int startSec = (int)((segmentStart * 60) + ae.StartTime);
                    string line = String.Format("{1}{0}{2,8:f3}{0}{3}{0}{4:f2}{0}{5}{0}{6:f1}{0}{7}{0}{8}{0}{9:f2}{0}{10:f2}{0}{11:f2}{0}{12:f2}{0}{13:f2}{0}{14:f2}{0}{15:f2}",
                                         reportSeparator, segmentStart, duration, ae.Name, ae.StartTime, startSec, ae.Duration, ae.MinFreq, ae.MaxFreq,
                                         ae.kiwi_durationScore, ae.kiwi_hitScore, ae.kiwi_snrScore, ae.kiwi_sdPeakScore, ae.kiwi_gapScore, ae.kiwi_bandWidthScore, ae.ScoreNormalised);
                    sb.AppendLine(line);
                }
            }
            return sb;
        }



        public static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores, double[] oscillationRates,
                                        List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            //Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddTrack(Image_Track.GetScoreTrack(oscillationRates, 0.5, 1.5, 1.0));
                double maxScore = 16.0;
                image.AddSuperimposedMatrix(hits, maxScore);
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        public static void CheckArguments(string[] args)
        {
            if (args.Length != 2)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 2);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("KiwiDetect.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine();
            Console.WriteLine("NOTE: By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }

    } //end class
}
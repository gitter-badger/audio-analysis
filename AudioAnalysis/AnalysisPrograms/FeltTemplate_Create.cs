﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using System.Drawing;


namespace AnalysisPrograms
{



    /// <summary>
    /// This program extracts a template from a recording.
    /// COMMAND LINE ARGUMENTS:
    /// string recordingPath = args[0];   //the recording from which template is to be extracted
    /// string iniPath       = args[1];   //the initialisation file containing parameters for the extraction
    /// string targetName    = args[2];   //prefix of name of the created output files 
    /// 
    /// The program produces four (4) output files:
    ///     string targetPath         = outputDir + targetName + "_target.txt";        //Intensity values (dB) of the marqueed portion of spectrum BEFORE noise reduction
    ///     string targetNoNoisePath  = outputDir + targetName + "_targetNoNoise.txt"; //Intensity values (dB) of the marqueed portion of spectrum AFTER  noise reduction
    ///     string noisePath          = outputDir + targetName + "_noise.txt";         //Intensity of noise (dB) in each frequency bin included in template
    ///     string targetImagePath    = outputDir + targetName + "_target.png";        //Image of noise reduced spectrum
    ///     
    /// The user can then edit the image file to produce a number of templates.
    /// </summary>
    class FeltTemplate_Create
    {
        //GECKO
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  FELT_Gecko1
        //CURRAWONG
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_Currawong_Params.txt  FELT_Currawong2
        //CURLEW
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"              C:\SensorNetworks\Output\FELT_CURLEW2\FELT_CURLEW_Params.txt  FELT_Curlew2
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-213000.wav"        C:\SensorNetworks\Output\FELT_CURLEW3\FELT_CURLEW_Params.txt  FELT_Curlew3
        
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME          = "CALL_NAME";
        public static string key_DO_SEGMENTATION    = "DO_SEGMENTATION";
        public static string key_EVENT_START        = "EVENT_START";
        public static string key_EVENT_END          = "EVENT_END";
        public static string key_MIN_HZ             = "MIN_HZ";
        public static string key_MAX_HZ             = "MAX_HZ";
        public static string key_TEMPLATE_MIN_INTENSITY = "TEMPLATE_MIN_INTENSITY";
        public static string key_TEMPLATE_MAX_INTENSITY = "TEMPLATE_MAX_INTENSITY";
        public static string key_FRAME_OVERLAP      = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW      = "SMOOTH_WINDOW";
        public static string key_SOURCE_RECORDING   = "SOURCE_RECORDING";
        public static string key_MIN_DURATION       = "MIN_DURATION";
        public static string key_DECIBEL_THRESHOLD  = "DECIBEL_THRESHOLD";        // Used when extracting analog template from spectrogram.
        public static string key_TEMPLATE_THRESHOLD = "TEMPLATE_THRESHOLD";       // Value in 0-1. Used when preparing binary, trinary and syntactic templates.
        public static string key_DONT_CARE_NH       = "DONT_CARE_BOUNDARY";       // Used when preparing trinary template.
        public static string key_LINE_LENGTH        = "SPR_LINE_LENGTH";          // Used when preparing syntactic PR template.
        public static string key_DRAW_SONOGRAMS     = "DRAW_SONOGRAMS";




        public static void Dev(string[] args)
        {
            string title = "# EXTRACT AND SAVE ACOUSTIC EVENT.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath       = args[1]; // path of the ini or params file
            string targetName    = args[2]; // prefix of name of created files 

            string outputDir         = Path.GetDirectoryName(iniPath) + "\\";
            string targetPath        = outputDir + targetName + "_target.txt";
            string targetNoNoisePath = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath         = outputDir + targetName + "_noise.txt";
            string targetImagePath   = outputDir + targetName + "_target.png";
            string paramsPath        = outputDir + targetName + "_params.txt";

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;

            double frameOverlap      = Double.Parse(dict[key_FRAME_OVERLAP]);
            double eventStart        = Double.Parse(dict[key_EVENT_START]);
            double eventEnd          = Double.Parse(dict[key_EVENT_END]);            
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            double dBThreshold       = Double.Parse(dict[key_DECIBEL_THRESHOLD]);   //threshold to set MIN DECIBEL BOUND
            int DRAW_SONOGRAMS       = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            // iii: Extract the event as TEMPLATE
            // #############################################################################################################################################
            Log.WriteLine("# Start extracting target event.");
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, dBThreshold);
            var sonogram           = results.Item1;
            var extractedEvent     = results.Item2;
            var template           = results.Item3;  // event's matrix of target values before noise removal
            var noiseSubband       = results.Item4;  // event's array  of noise  values
            var templateMinusNoise = results.Item5;  // event's matrix of target values after noise removal
            Log.WriteLine("# Finished extracting target event.");
            // #############################################################################################################################################

            // iv: SAVE extracted event as matrix of dB intensity values
            FileTools.WriteMatrix2File(template, targetPath);                  // write template values to file PRIOR to noise removal.
            FileTools.WriteMatrix2File(templateMinusNoise, targetNoNoisePath); // write template values to file AFTER to noise removal.
            FileTools.WriteArray2File(noiseSubband, noisePath);

            // v: SAVE image of extracted event in the original sonogram 
            string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            DrawSonogram(sonogram, sonogramImagePath, extractedEvent);

            // vi: SAVE extracted event as noise reduced image 
            // alter matrix dynamic range so user can determine correct dynamic range from image 
            // matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange);       // set event's dynamic range
            var results1    = BaseSonogram.Data2ImageData(templateMinusNoise);
            var targetImage = results1.Item1;
            var min = results1.Item2;
            var max = results1.Item3;
            ImageTools.DrawMatrix(targetImage, 1, 1, targetImagePath);

            // vii: SAVE parameters file
            dict.Add(key_SOURCE_RECORDING, sonogram.Configuration.SourceFName);
            dict.Add(key_TEMPLATE_MIN_INTENSITY, min.ToString());
            dict.Add(key_TEMPLATE_MAX_INTENSITY, max.ToString());
            WriteParamsFile(paramsPath, dict);

            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } // Dev()





        public static System.Tuple<BaseSonogram, AcousticEvent, double[,], double[], double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, double backgroundThreshold)
        {
            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            
            //calculate the modal noise profile
            double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            //extract modal noise values of the required event
            double[] noiseSubband = BaseSonogram.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);
            
            //extract data values of the required event
            double[,] target = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            // create acoustic event with defined boundaries
            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd - eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);

            //truncate noise
            sonogram.Data = SNR.TruncateModalNoise(sonogram.Data, modalNoise);
            sonogram.Data = SNR.RemoveBackgroundNoise(sonogram.Data, backgroundThreshold);

            double[,] targetMinusNoise = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            return System.Tuple.Create(sonogram, ae, target, noiseSubband, targetMinusNoise);
        }//end Execute_Extraction()



        public static void DrawSonogram(BaseSonogram sonogram, string path, AcousticEvent ae)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                var aes = new List<AcousticEvent>();
                aes.Add(ae);
                image.AddEvents(aes);
                image.Save(path);
            }
        } //end DrawSonogram

        public static void WriteParamsFile(string paramsPath, Dictionary<string, string> dict)
        {
            var list = new List<string>();

            list.Add("# FELT TEMPLATE");
            list.Add("\nDATE="+DateTime.Now+"\n");
            list.Add("CALL_NAME="+ dict[key_CALL_NAME]);

            list.Add("FRAME_OVERLAP=" + Double.Parse(dict[key_FRAME_OVERLAP]));
            list.Add("#Do segmentation prior to search.");
            list.Add("DO_SEGMENTATION=false");
            // list.Add("# Window (duration in seconds) for smoothing acoustic intensity before segmentation.");
            // list.Add("SMOOTH_WINDOW=0.333");

            list.Add("EVENT_SOURCE" + dict[key_SOURCE_RECORDING]);
            list.Add("#EVENT BOUNDS");
            list.Add("# Start and end of an event. (Seconds into recording.) Min and max freq. Min and max intensity");
            list.Add("#Time:      " + dict[key_EVENT_START] + " to End = " + dict[key_EVENT_END] + "seconds.");
            list.Add("#Frequency: " + dict[key_MIN_HZ]+" to "+ dict[key_MAX_HZ] + " Herz.");
            list.Add("#Intensity: " + dict[key_TEMPLATE_MIN_INTENSITY] + " to " + dict[key_TEMPLATE_MAX_INTENSITY] + " dB.");
            list.Add("TEMPLATE_MAX_INTENISTY="+dict[key_TEMPLATE_MAX_INTENSITY]);

            list.Add("#DECIBEL THRESHOLD FOR EXTRACTING template FROM SPECTROGRAM - dB above background noise");
            list.Add("DECIBEL_THRESHOLD="+ dict[key_DECIBEL_THRESHOLD]); //threshold to set MIN DECIBEL BOUND
            list.Add("#DON'T CARE BOUNDARY FOR PREPARING TRINARY template");
            list.Add("DONT_CARE_BOUNDARY"+ dict[key_DONT_CARE_NH]);
            list.Add("#LINE LENGTH FOR PREPARING SYNTACTIC template");
            list.Add("SPR_LINE_LENGTH=" + dict[key_LINE_LENGTH]);

            list.Add("# save a sonogram for each recording that contained a hit ");
            list.Add("DRAW_SONOGRAMS=2");

            FileTools.WriteTextFile(paramsPath, list);
        }


    }//class
}
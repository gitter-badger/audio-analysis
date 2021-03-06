// <copyright file="RecordingFetcher.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.WavTools
{
    using System;

    public class RecordingFetcher
    {
        private const string SERVER = "http://sensor.mquter.qut.edu.au/sensors";

        public static byte[] GetRecordingByFileName(string filename)
        {
            Uri address = ConvertToUri(filename);

            System.Net.WebClient webClient = new System.Net.WebClient();
            return webClient.DownloadData(address);
        }

        private static Uri ConvertToUri(string filename)
        {
            ParseFilename(filename, out var sensorName, out var recordingName, out var extension);

            string uriString = $"{SERVER}/{sensorName}/{recordingName}.{extension}";

            return new Uri(uriString);
        }

        private static void ParseFilename(string filename, out string sensorname, out string recordingname, out string extension)
        {
            filename = filename.Replace('/', '_');

            string[] parts = filename.Split('_');
            sensorname = parts[0];
            string fileId = parts[1];

            string[] parts2 = fileId.Split('.');
            recordingname = parts2[0];
            extension = parts2[1];
        }
    }//end class
}//end nmaespace

// <copyright file="AmplitudeSonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using Acoustics.Tools.Wav;

    /// <summary>
    /// This class is designed to produce a full-bandwidth spectrogram of spectral amplitudes
    /// The constructor calls the three argument BaseSonogram constructor.
    /// </summary>
    public class AmplitudeSonogram : BaseSonogram
    {
        public AmplitudeSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav, false)
        {
        }

        /// <summary>
        /// This method does nothing because do not want to change the amplitude sonogram in any way.
        /// Actually the constructor of this class calls the BaseSonogram constructor that does NOT include a call to Make().
        /// Consequently this method should never be called. Just a place filler.
        /// </summary>
        /// <param name="amplitudeM">amplitude sonogram</param>
        public override void Make(double[,] amplitudeM)
        {
        }
    }
}
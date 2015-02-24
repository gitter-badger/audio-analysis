﻿using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Representations;
using Dong.Felt.Configuration;

namespace Dong.Felt.Representations
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using Acoustics.Shared.Extensions;

    public class EventBasedRepresentation : AcousticEvent
    {
        #region Public Properties
        public static Color DEGAULT_BORDER_COLOR = Color.Crimson;
        public static Color DEFAULT_SCORE_COLOR = Color.Black;

        public Point Centroid { get; set; }

        /// <summary>
        /// The unit of Bottom is pixel.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// The unit of Left is pixel.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The unit of Width is pixel.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The unit of Height is pixel.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The unit of Area is pixel.
        /// </summary>
        public int Area { get; set; }

        public int InsideRidgeOrientation { get; set; }

        public double TimeScale { get; set; }

        public double FreqScale { get; set; }

        public int POICount { get; set; }

        #endregion


        #region Public Methods

        public EventBasedRepresentation(double timeScale, double freqScale,
            double maxFrequency, double minFrequency, double startTime, double endTime)
        {
            this.MaxFreq = maxFrequency;
            this.MinFreq = minFrequency;
            this.TimeStart = startTime;
            this.Duration = endTime - startTime;
            this.Bottom = (int)(minFrequency / freqScale) + 1;
            this.Left = (int)(startTime / timeScale) + 1;          

            this.Width = (int)(this.Duration / timeScale) + 1;
            this.Height = (int)(this.FreqRange / freqScale) + 1 + 1;
            this.Centroid = new Point(this.Left + this.Width / 2, this.Bottom + this.Height / 2);
            this.Area = this.Width * this.Height;
        }

        /// <summary>
        /// Take in ridges and form events.
        /// This method aims to detect events from seperated (4 directional) ridges. 
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="ridges"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        //public static List<EventBasedRepresentation> RidgesToAcousticEvents(SpectrogramStandard sonogram,
        //    List<PointOfInterest> ridges, int rows, int cols, CompressSpectrogramConfig compressConfig)
        //{
        //    var result = new List<EventBasedRepresentation>();
        //    // Gaussian blur on ridges 
        //    var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, rows, cols);
        //    var verticalWindowLength = 5;
        //    var horizontalWindowLength = 3;
        //    var sigmaGaussianBlur = 1.0;
        //    var gaussianBlurSize = 3;
        //    var smoothedRidges = ClusterAnalysis.SmoothRidges(ridges, rows, cols,
        //        verticalWindowLength, horizontalWindowLength, sigmaGaussianBlur, gaussianBlurSize);
        //    var smoothedRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(smoothedRidges);
        //    var dividedPOIList = POISelection.POIListDivision(smoothedRidgesList);
        //    var verAcousticEvents = new List<AcousticEvent>();
        //    var horAcousticEvents = new List<AcousticEvent>();
        //    var posAcousticEvents = new List<AcousticEvent>();
        //    var negAcousticEvents = new List<AcousticEvent>();
        //    ClusterAnalysis.SeperateRidgeListToEvent(sonogram, dividedPOIList[0],
        //        dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
        //        rows, cols,
        //        out verAcousticEvents, out horAcousticEvents, out posAcousticEvents, out negAcousticEvents);
        //    foreach (var v in verAcousticEvents)
        //    {
        //        var ve = GetPropertiesFromEvents(sonogram, v, compressConfig);
        //        result.Add(ve);
        //    }
        //    foreach (var h in horAcousticEvents)
        //    {
        //        var he = GetPropertiesFromEvents(sonogram, h, compressConfig);
        //        result.Add(he);
        //    }
        //    foreach (var p in posAcousticEvents)
        //    {
        //        var pe = GetPropertiesFromEvents(sonogram, p, compressConfig);
        //        result.Add(pe);
        //    }
        //    foreach (var n in negAcousticEvents)
        //    {
        //        var ne = GetPropertiesFromEvents(sonogram, n, compressConfig);
        //        result.Add(ne);
        //    }
        //    return result;
        //}

        public static List<EventBasedRepresentation> AcousticEventsToEventBasedRepresentations(SpectrogramStandard sonogram,
            List<AcousticEvent> ae, int orientationType)
        {
            var result = new List<EventBasedRepresentation>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            foreach (var e in ae)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale, 
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = orientationType;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                result.Add(ep);
            }
            return result;
        }

        public static List<List<EventBasedRepresentation>> AcousticEventsToEventBasedRepresentations(SpectrogramStandard sonogram,
           List<List<AcousticEvent>> ae)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            var vAcousticEventList = ae[0];
            var hAcousticEventList = ae[1];
            var pAcousticEventList = ae[2];
            var nAcousticEventList = ae[3];
            var vResult = new List<EventBasedRepresentation>();
            foreach (var e in vAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = 0;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                vResult.Add(ep);
            }
            var hResult = new List<EventBasedRepresentation>();
            foreach (var e in hAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = 1;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                hResult.Add(ep);
            }
            var pResult = new List<EventBasedRepresentation>();
            foreach (var e in pAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = 2;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                pResult.Add(ep);
            }
            var nResult = new List<EventBasedRepresentation>();
            foreach (var e in nAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = 3;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                nResult.Add(ep);
            }
            result.Add(hResult);
            result.Add(vResult);
            result.Add(pResult);
            result.Add(nResult);
            return result;
        }  

        // users might provide the rectangle boundary information of query, so this method aims to detect query 
        public static List<EventBasedRepresentation> ReadQueryAsAcousticEventList(List<EventBasedRepresentation> events,
            Query query)
        {
            var result = new List<EventBasedRepresentation>();

            foreach (var e in events)
            {                
                if (e.Centroid.X > query.LeftInPixel && e.Centroid.X < query.RightInPixel)
                {
                    if (e.Centroid.Y > query.BottomInPixel && e.Centroid.Y < query.TopInPixel)
                    {
                        if (e.Bottom < query.BottomInPixel)
                        {
                            e.Bottom = query.BottomInPixel;
                        }
                        if (e.Bottom + e.Height > query.TopInPixel)
                        {
                            e.Height = query.TopInPixel - e.Bottom;
                        }
                        if (e.Left < query.LeftInPixel)
                        {
                            e.Left = query.LeftInPixel;
                        }
                        if (e.Left + e.Width > query.RightInPixel)
                        {
                            e.Width = query.RightInPixel - e.Left;
                        }                       
                        result.Add(e);
                    }
                }
            }
            return result;
        }
        
        
        public static List<EventBasedRepresentation> SelectEvents(
            List<EventBasedRepresentation> eventList,
            int minFreq,
            int maxFreq,
            int startTime,
            int endTime,
            int maxFrequency,
            int maxFrame)
        {
            var result = new List<EventBasedRepresentation>();            
            if (StatisticalAnalysis.checkBoundary(minFreq, startTime, maxFrequency, maxFrame)
                && StatisticalAnalysis.checkBoundary(maxFreq, endTime, maxFrequency, maxFrame))
            {
                foreach (var c in eventList)
                {
                    if (c.Centroid.X > startTime && c.Centroid.X < endTime)
                    {
                        if (c.Centroid.Y > minFreq && c.Centroid.Y < maxFreq)
                        {
                            if (c.Bottom < minFreq)
                            {
                                c.Bottom = minFreq;
                            }
                            if (c.Bottom + c.Height > maxFreq)
                            {
                                c.Height = maxFreq - c.Height;
                            }
                            if (c.Left < startTime)
                            {
                                c.Left = startTime;
                            }
                            if (c.Left + c.Width > endTime)
                            {
                                c.Width = endTime - c.Left;
                            }
                            c.Area = c.Width * c.Height;
                            result.Add(c);
                        }
                    }
                }
            }
            return result;
        }

        public static List<List<EventBasedRepresentation>> AddSelectedEventLists(List<List<EventBasedRepresentation>> eventList,
            int minFreq,
            int maxFreq,
            int startTime,
            int endTime,
            int maxFrequency,
            int maxFrame)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var vEvents = SelectEvents(eventList[0], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var hEvents = SelectEvents(eventList[1], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var pEvents = SelectEvents(eventList[2], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var nEvents = SelectEvents(eventList[3], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame); 
            result.Add(vEvents);
            result.Add(hEvents);
            result.Add(pEvents);
            result.Add(nEvents);
            return result;
        }

        /// <summary>
        /// This method aims to extract candidate region representation according to the provided
        /// marquee of the queryRepresentation, the frequencyBound are exactly the same as query.
        /// </summary>
        /// <param name="queryRepresentations"></param>
        /// <param name="candidateEventList"></param>
        /// <param name="centroidFreqOffset"> 
        /// </param>
        /// <returns></returns>
        public static List<RegionRepresentation> ExtractFixedAcousticEventList(SpectrogramStandard spectrogram,
            RegionRepresentation queryRepresentations,
            List<EventBasedRepresentation> candidateEventList, string file, Query query)
        {
            var result = new List<RegionRepresentation>();
            var startCentriod = queryRepresentations.MajorEvent.Centroid;

            var maxFrame = spectrogram.FrameCount;
            var maxFreq = spectrogram.Configuration.FreqBinCount;
            var potentialCandidateStart = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList)
            {
                if (c.Centroid.Y <= queryRepresentations.TopInPixel && c.Centroid.Y >= queryRepresentations.BottomInPixel)
                {
                    potentialCandidateStart.Add(c);
                }
            }
            foreach (var pc in potentialCandidateStart)
            {
                var realCandidate = new List<EventBasedRepresentation>();
                var maxFreqPixelIndex = queryRepresentations.topToBottomLeftVertex + pc.Bottom;
                var minFreqPixelIndex = pc.Bottom - queryRepresentations.bottomToBottomLeftVertex;
                var startTimePixelIndex = pc.Left - queryRepresentations.leftToBottomLeftVertex;
                var endTimePixelIndex = queryRepresentations.rightToBottomLeftVertex + pc.Left;

                if (StatisticalAnalysis.checkBoundary(minFreqPixelIndex, startTimePixelIndex, maxFreq, maxFrame)
                    && StatisticalAnalysis.checkBoundary(maxFreqPixelIndex, endTimePixelIndex, maxFreq, maxFrame))
                {
                    foreach (var c in candidateEventList)
                    {
                        if (c.Centroid.X > startTimePixelIndex && c.Centroid.X < endTimePixelIndex)
                        {
                            if (c.Centroid.Y > minFreqPixelIndex && c.Centroid.Y < maxFreqPixelIndex)
                            {
                                if (c.Bottom < minFreqPixelIndex)
                                {
                                    c.Bottom = minFreqPixelIndex;
                                }
                                if (c.Bottom + c.Height > maxFreqPixelIndex)
                                {
                                    c.Height = maxFreqPixelIndex - c.Height;
                                }
                                if (c.Left < startTimePixelIndex)
                                {
                                    c.Left = startTimePixelIndex;
                                }
                                if (c.Left + c.Width > endTimePixelIndex)
                                {
                                    c.Width = endTimePixelIndex - c.Left;
                                }
                                realCandidate.Add(c);
                            }
                        }
                    }
                }
                var candidateRegionRepre = new RegionRepresentation(realCandidate, file, query);
                candidateRegionRepre.bottomToBottomLeftVertex = minFreqPixelIndex;
                candidateRegionRepre.topToBottomLeftVertex = maxFreqPixelIndex;
                candidateRegionRepre.rightToBottomLeftVertex = startTimePixelIndex;
                candidateRegionRepre.leftToBottomLeftVertex = endTimePixelIndex;
                candidateRegionRepre.TopInPixel = maxFreqPixelIndex;
                candidateRegionRepre.BottomInPixel = minFreqPixelIndex;
                candidateRegionRepre.LeftInPixel = startTimePixelIndex;
                candidateRegionRepre.RightInPixel = endTimePixelIndex;
                result.Add(candidateRegionRepre);
            }
            return result;
        }
        
        public static List<EventBasedRepresentation> ExtractPotentialCandidateEvents(           
            List<EventBasedRepresentation> queryRepresentations, List<EventBasedRepresentation> candidateEventList, int centroidFreqOffset)
        {           
            if (queryRepresentations.Count > 0)
            {
                queryRepresentations.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));                 
            }
            var BottomLeftEventInQuery = queryRepresentations[0];           
            var potentialCandidateLocation = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList)
            {
                if (Math.Abs(c.Centroid.Y - BottomLeftEventInQuery.Centroid.Y) <= centroidFreqOffset)
                {
                    potentialCandidateLocation.Add(c);
                }
            }
            return potentialCandidateLocation;
        }
       
        #endregion
    }
}

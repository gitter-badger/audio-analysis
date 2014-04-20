﻿namespace AnalysisBase.StrongAnalyser.ResultBases
{
    public abstract class EventBase : ResultBase
    {
        //AudioAnalysisTools.Keys.EVENT_START_ABS,    //4
        public double? EventStartAbsolute { get; set; }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public double Score { get; set; }

        //AudioAnalysisTools.Keys.EVENT_START_SEC,    //3
        public double EventStartSeconds { get; set; }

        //AudioAnalysisTools.Keys.MIN_HZ
        public double? MinHz { get; set; }

        //AudioAnalysisTools.Keys.EVENT_COUNT,        //1
        public int EventCount { get; set; }

    }
}
// <copyright file="ART.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace NeuralNets
{
    using System;
    using TowseyLibrary;

    internal enum ARTversion
    {
        ART1, ART2v1, ART2v2, ART2a, FuzzyART, ARTMAP2a, FuzzyARTMAP,
    }

    internal enum Tasks
    {
        TRAIN, TEST, TRAINTEST,
    }

    internal enum ARTparams
    {
        a, b, c, d/*F2 output*/, Rho/*vigilance param*/, Theta/*threshold for contrast enhancing*/, Add1, RhoStar,
    }

    internal enum ARTMAPparams
    {
        alpha, beta, Rhoa/*vigilance parameters*/,
        Rhoab, Rhob, RhoTst/*different value of rho used for testing fuzzy ART*/,
        Add1/*used to increment rhoA in Artmap*/, ETP/*error tolerance parameter*/, Add2,
    }

    public sealed class ART
    {
        // {FILE NAMES INFORMATION}
        private const string ARTDir = @"C:\SensorNetworks\ART\";
        private const string configFName = "ART.ini";
        private const string paramsFName = "ARTParameters.txt";

        public static string wtsFname = "artWts";
        public static string wtsFExt = ".txt";
        public static string dataFname = "shapeData";
        public static string dataFExt = ".txt";
        public static bool DEBUG = true;

        //bool printTestResults;    // :boolean;
        //bool printDecisionMatrix; // :boolean;

        public static string configFpath = ARTDir + configFName;
        public static string paramsFpath = ARTDir + paramsFName;
        public static string dataFpath = ARTDir + dataFname + dataFExt;

        //string PrevARTLessonFName         =  "PrevART.les";
        //ARCHITECTURE INFO
        public static int maxF1Size = 100;

        //public static int maxF2Size = 1000;
        //DATA INFORMATION}
        public static bool randomiseTrnSetOrder = true;
        public static int maxIterations = 1000;
        public static int numberOfRepeats = 1;
        public static int maxClassNo = 2;

        private const int noARTVersions = 7;
        private readonly string[] versionNames = { "ART-1", "ART-2v1F&S", "ART-2v2", "ART-2a", "fuzzy-ART", "ARTMAP-2a", "fuzyARTMAP" };
        private readonly string[] taskNames = { "TRAIN net", "TEST net", "TRN&TEST" };

        //string[] paramNames   = {"alpha", "beta", "c", "d"/*F2 output*/,
        //                            "Rho",    //vigilance param*/
        //                            "Theta",  //threshold for contrast enhancing
        //                            "Add1", "RhoStar"
        //                        };
        private readonly string[,] paramNames =
  {
    {
        "A1",    "B1",    "C1",    "D1", " rho ",     "L", " add1", " add2",
    },
    {
        "a",     "b",     "c",     "d", " rho ", "theta", " ETP ", " add2",
    },
    {
        "a",     "b",     "c",     "d", " rho ", "theta", " ETP ", " add2",
    },
    {
        "alpha", " beta", "  c  ", "  d  ", " rho ", "theta", " add1", " rho*",
    },
    {
        "alpha", " beta",      "",      "", " rho ", "rhoTs",      "",      "",
    },
    {
        "alpha", " beta", " rhoa", "rhoab", " rhob", "theta", " add1",      "",
    },
    {
        "alpha", " beta", " rhoa", "rhoab", " rhob", "rhoTs", " add1",      "",
    },
   };

        //char Esc      = 27; //#27;
        //char escape   = 27; //#27;
        //char FormFeed = 12; //#12; {Hex 0C}
        //char FF       = 12; //#12;

//Type
    //PtrToArrayOfInt       = ^ArrayOfInt;
    //ArrayOfInt            =  array [1..MaxF1Size] of integer;
    //PtrToArrayOfPtrsToIntArray= ^ArrayOfPtrsToIntArray;
    //ArrayOfPtrsToIntArray =  array [1..MaxF2Size] of PtrToArrayOfInt;
    //PtrToArrayOfByte      = ^ArrayOfByte;
    //ArrayOfByte           =  array [1..MaxF1Size] of byte;

//Type
    //ART2F1LayerRec        = record
    //   Size              : Integer;
    //   Weight            : PtrToArrayOfPtrsToFloatArray;
    //   w                 : PtrToArrayOfFloat;
    //   x                 : PtrToArrayOfFloat;
    //   v                 : PtrToArrayOfFloat;
    //   u                 : PtrToArrayOfFloat;
    //   uPrev             : PtrToArrayOfFloat;
    //   p                 : PtrToArrayOfFloat;
    //   q                 : PtrToArrayOfFloat;
    //end;

    //ART2F2LayerRec       = record
    //   Size              : integer;
    //   Weight            : PtrToArrayOfPtrsToFloatArray;
    //   inhibit           : PtrToArrayOfInt ;
    //end;

        //CONST
        //  These TWO constants are used only for graphical display of avSig and wts at the end.
        //  Sets the scale for the Y axis graphical display. Since wts are always in range 0-1 therefore set wtsRange = 1;
        //  If using EEG values for input sigs, set avSigRange = 100.
        //  If using normalised inputs, set avSigRange=1. ie depends on data type}
        private const int avSigRange = 1;
        private const int wtsRange = 1;

        //the following two arrays are used only in GetOneSig but they must hold the random numbers and targets of the Training Set}
        //int[] randomArray = new int[maxTrnSetSize];    // : array[1..maxTrnSetSize] of word;
        //byte[] targetArray = new byte[maxTrnSetSize];   // : array[1..maxTrnSetSize] of byte;

        public static void ReadConfigFile()
    {
        //string line, subject, predicate;  // : string[80];
        //int code;  // : integer;
        //assign (F, ConfigFPath);
        //reset (F);
        //repeat
        //    Console.ReadLine(F, line);
        //    if (line[1] = "#") continue;

        //    subject   = copy(Line, 1,               Pos("=",Line)-1 );
        //    predicate = copy(Line, Pos("=",Line)+1, Length(Line)    );
        //    for (k = 1 to length(subject)) subject[k] = upcase(subject[k]);

        //    if (subject = "TASK")         ) val(predicate, task, code)
        //    else
        //    if (subject = "WTSFPATH")     ) WtsFPath = predicate
        //    else
        //    if (subject = "OUTPUTDIR")    ) outputDir = predicate
        //    else
        //    if (subject = "ARTVERSION")   ) val(predicate, versionID, code)
        //    else
        //    if (subject = "F1SIZENETA")   ) val(predicate, F1SizeOfNeta, code)
        //    else
        //    if (subject = "F2SIZENETA")   ) val(predicate, F2SizeOfNeta, code)
        //    else
        //    if (subject = "F1SIZENETB")   ) val(predicate, F1SizeOfNetb, code)
        //    else
        //    if (subject = "F2SIZENETB")   ) val(predicate, F2SizeOfNetb, code)
        //    else
        //    if (subject = "DATADIM")      ) val(predicate, DataDim, code)
        //    else
        //    if (subject = "TRNSETSIZE"   ) val(predicate, TrnSetSize, code)
        //    else
        //    if (subject = "TRNSETFPATH"  ) TrnSetFPath = predicate
        //    else
        //    if (subject = "TRNTARFPATH"  ) TrnTarFpath = predicate
        //    else
        //    if (subject = "TSTSETSIZE"   ) val(predicate, TstSetSize, code)
        //    else
        //    if (subject = "TSTSETFPATH"  ) TstSetFPath = predicate
        //    else
        //    if (subject = "TSTTARFPATH"  ) TstTarFpath = predicate
        //    else
        //    if (subject = "NUMBEROFCLASSES" ) val(predicate, noClasses, code)
        //    else
        //    if (subject == "DISPLAYON")
        //    {
        //        if ((predicate[1]=="N") || (predicate[1]="n")) displayOn= false
        //        else                                           displayOn= true;
        //    }
        //    else
        //    if (subject == "PRINTTESTRESULTS")
        //    {
        //        if ((predicate[1]="N") || (predicate[1]="n")) PrintTestResults = false;
        //        else                                          PrintTestResults = true;
        //    }
        //    else
        //    if (subject == "PRINTDECISIONMATRIX")
        //    {
        //        if ((predicate[1]="N") || (predicate[1]="n")) PrintDecisionMatrix= false;
        //        else                                          PrintDecisionMatrix = true;
        //    }
        //    else
        //    if (subject = "PAUSEAFTERREPEATS")
        //    {
        //        if ((predicate[1]="N") || (predicate[1]="n")) PauseAfterRepeat = false else PauseAfterRepeat = true;
        //    } else
        //    if (subject = "NUMBEROFREPEATS") val(predicate, norepeats, code);
        //    else
        //    if (subject = "MAXITERATIONS") val(predicate, MaxIterations, code);
        //    else
        //    if (subject = "PREPROCESS") val(predicate, preprocess, code);

        //until eoF(F); //end of repeat over all lines in file
        //close (F);
    } //end ReadConfigFile()

    //public static void DisplayConfiguration()  //display on the screen}
//{
//  ClrScr;
//  LoggedConsole.WriteLine; LoggedConsole.WriteLine ("      CURRENT ART CONFIGURATION");
//  LoggedConsole.WriteLine;
//  LoggedConsole.WriteLine ("Task             = ", TaskStr[task]);
//  LoggedConsole.WriteLine ("Wts file to use  = ", WtsFPath);
//  LoggedConsole.WriteLine ("Output directory = ", OutputDir);
//  LoggedConsole.WriteLine ("ART version      = ", versionID," or ",
//                                  versionNames[versionID]);
//  write   ("Size of F1 Net a = ", F1sizeOfNeta:2);
//  LoggedConsole.WriteLine ("        Size of F2 Net a = ", F2sizeOfNeta);
//  write   ("Size of F1 Net b = ", F1sizeOfNetb:2);
//  LoggedConsole.WriteLine ("        Size of F2 Net b = ", F2sizeOfNetb);
//  LoggedConsole.WriteLine ("Dim of input data= ", DataDim);
//  LoggedConsole.WriteLine ("Train set size   = ", trnSetSize);
//  LoggedConsole.WriteLine ("Train set path   = ", trnSetFpath);
//  LoggedConsole.WriteLine ("Training targets = ", trnTarFpath);
//  LoggedConsole.WriteLine ("Test set size    = ", tstSetSize);
//  LoggedConsole.WriteLine ("Test set path    = ", tstSetFpath);
//  LoggedConsole.WriteLine ("Test set targets = ", tstTarFpath);
//  LoggedConsole.WriteLine ("Number of classes= ", NoClasses);
//  LoggedConsole.WriteLine ("Number of repeats= ", norepeats);
//  LoggedConsole.WriteLine ("Max no iterations= ", MaxIterations);
//  write   ("DisplayOn= ",displayOn);
//  LoggedConsole.WriteLine ("  Print Test Results= ", printTestResults);
//  write   ("Print decision matrix= ", printDecisionMatrix);
//  LoggedConsole.WriteLine ("  Pause After Repeats= ", pauseAfterRepeat);
//  LoggedConsole.WriteLine ("Preprocess type  = ", PreprocessStr[preprocess]);
//  LoggedConsole.WriteLine;
//  write   ("                                press any key to continue .... ");
//  repeat until keypressed;
//}  //end;

//    public static void CheckConfiguration(int code)
//    {
////        var
//        int i;
//        char key;

//        if (! PrinterOn)
//        {
//            clrScr;
//            LoggedConsole.WriteLine; LoggedConsole.WriteLine; LoggedConsole.WriteLine;
//            LoggedConsole.WriteLine (" WARNING:- Printer is (! on !!!!");
//            LoggedConsole.WriteLine ("           Turn on-line now if desired. Press any key to continue...");
//            key = Console.ReadKey();
//            //repeat until keypressed;
//            //key = readkey;
//        }  //}  //end;

//        code = 0;    //{default = no error, now generate error codes}
//  if (! FileExists (trnSetFPath)    ) code = 1;
//  if (! FileExists (trnTarFpath)    ) code = 2;
//  if (! FileExists (tstSetFPath)    ) code = 3;
//  if (! FileExists (tstTarFpath)    ) code = 4;
//  if (task == taskTEST)
//    if (! fileExists (WtsFPath)     ) code = 5;

//  if ((task = taskTRAIN) || (task = taskTRAINTEST))
//    if (fileExists (WtsFPath) ) code = 6;
//    else
//    {
//      clrScr; LoggedConsole.WriteLine; LoggedConsole.WriteLine; LoggedConsole.WriteLine;
//      LoggedConsole.WriteLine ("(!E:- Will create a new weights file - ",wtsFPath);
//      LoggedConsole.WriteLine ("       New wts file created for every simulation. ");
//      LoggedConsole.WriteLine ("       Default ext is .wnn where nn is the simulation number");
//      LoggedConsole.WriteLine;
//      LoggedConsole.WriteLine (" Press any key to return to main menu.");
//      key = readkey;
//    }  //end;
//  if (trnSetSize > maxTrnSetSize) ) code = 7;
//  if (F1SizeOfNeta > maxF1size)   ) code = 8;
//  if (F2SizeOfNeta > maxF2size)   ) code = 9;

//  if (code <> 0)   //     {messages concerning fatal errors}
//    {
//    clrScr;
//    LoggedConsole.WriteLine; LoggedConsole.WriteLine ("            CONFIGURATION ERROR!");
//    LoggedConsole.WriteLine; LoggedConsole.WriteLine (" Must edit configuration file in main menu");
//    LoggedConsole.WriteLine;
//    case code of
//    1: LoggedConsole.WriteLine (" Cannot find training set file ",   trnSetFPath);
//    2: LoggedConsole.WriteLine (" Cannot find training target file ",trnTarFPath);
//    3: LoggedConsole.WriteLine (" Cannot find test set file ",       tstSetFPath);
//    4: LoggedConsole.WriteLine (" Cannot find test target file ",    tstTarFPath);
//    5: LoggedConsole.WriteLine (" Cannot find wts file ",            wtsFPath);
//    6: LoggedConsole.WriteLine (" Cannot train over an existing wts file ie ", wtsFPath);
//    7: LoggedConsole.WriteLine (" Train set size exceeds maximum of ",maxTrnSetSize, " declared in source code.");
//    8: LoggedConsole.WriteLine (" F1 layer size exceeds maximum of ",maxF1size, " declared in source code.");
//    9: LoggedConsole.WriteLine (" F2 layer size exceeds maximum of ",maxF2size, " declared in source code.");
//    }  //end; {of case statement}
//    LoggedConsole.WriteLine;
//    LoggedConsole.WriteLine (" Press any key to return to main menu.");
//    key = readkey;
//        }  //}  //end;
//    }  //}  //end;

        public static double[,] ReadParameterValues(string paramFName)
    {
        //ArrayList lines = TowseyLib.DataTools.ReadFile(paramFName);        //prepare parameters file
        //readln (ParamsF, NoSimulationsInRun);
        //noValues = 0;
        //while (! eoln(F))
        //{
        //    read (F, parameters[noValues]);
        //    noValues++;
        //}  //end;
        //readln(F);

        double[,] parameters = new double[1, 8];
        int row = 0;

        //alpha   beta   c     d     rho    theta  add1    rho* not entered in parameters - calculated separately
        //0.1768  0.1    0.1   0.9   0.95   0.05   0.0001
        parameters[row, (int)ARTparams.a] = 0.1768;
        parameters[row, (int)ARTparams.b] = 0.1;
        parameters[row, (int)ARTparams.c] = 0.1;
        parameters[row, (int)ARTparams.d] = 0.9;     //F2 output
        parameters[row, (int)ARTparams.Rho] = 0.95;
        parameters[row, (int)ARTparams.Theta] = 0.05;
        parameters[row, (int)ARTparams.Add1] = 0.0001;

        double sigma = parameters[row, (int)ARTparams.c] * parameters[row, (int)ARTparams.d] / (1 - parameters[row, (int)ARTparams.d]);
        double rhoStar = 0.0;
        double sqr_sigma = sigma * sigma;
        double sqr_1plusSigma = (1 + sigma) * (1 + sigma);
        double sqr_rho = parameters[row, (int)ARTparams.Rho] * parameters[row, (int)ARTparams.Rho];
        if (parameters[row, (int)ARTparams.Rho] > 0.001)
            {
                rhoStar = ((sqr_rho * sqr_1plusSigma) - (1 + sqr_sigma)) / (2 * sigma);
            }

            // the original line of code: RhoStar = (Sqr(parameters[Rho]) * Sqr(1 + Sigma) - (1 + Sqr(Sigma))) / (2 * Sigma);

        parameters[row, (int)ARTparams.RhoStar] = rhoStar;

        return parameters;
    } //end;

    //public static bool ParamValuesOK(int versionID)
    //{
        //var
    //    bool AllOK = true;
    //    File ParamsF;   //    : text;
    //    double[] parameters = new double[MaxARTparamsNo]; // of real;
    //    int noValues;  //   : word;
    //    int simul, pn;  //  : word; // {parameter counter}
    //    char key;      //        : char;
    //    int i;         //:word;

    //    TextBackGround (1); //  {blue background}
    //    ClrScr;
    //    LoggedConsole.WriteLine;
    //    LoggedConsole.WriteLine ("  CHECKING PARAMETER VALUES for Net version ",ARTversion);
    //    LoggedConsole.WriteLine;

    //    assign (ParamsF, ParamsFPath);       // {prepare parameters file}
    //    reset  (ParamsF);
    //    readln (ParamsF, NoSimulationsInRun);

    //    for (int simul= 0; simul < noSimulationsInRun; simul++)
    //    {
    //        ReadParameterValues(noValues, ParamsF, parameters);
    //        LoggedConsole.WriteLine ("  Line "+simul+ " is OK");

    //        //if (noValues != MaxARTparamsNo-1 )
    //        //{
    //        //    for (pn = 1 to maxARTParamsNo-1)    //  {display all 8 parameter values}
    //        //    LoggedConsole.WriteLine ("  (",pn,") ",paramNames[versionID, pn]:10,"  = ",parameters[pn]:7:4);
    //        //    LoggedConsole.WriteLine;
    //        //    LoggedConsole.WriteLine ("  Fault in values for simulation ",simul);
    //        //    LoggedConsole.WriteLine ("  Must have exactly  ",maxARTparamsNo-1, " parameter values.");
    //        //    LoggedConsole.WriteLine ("  Press any key to return to main menu.");
    //        //    key = readkey;
    //        //    ParamvaluesOK = false;
    //        //    exit;
    //        //}  //end;

    //        //{make sure theta is less than 1/sqrM}
    //        //if  ((versionID != verFuzzyART)&& (versionID != verfuzzyARTMAP) )
    //        //    if (parameters[Theta] >= (1/Sqrt(F1SizeofNeta)) )  )
    //        //{
    //        //    parameters[Theta] = (1/Sqrt(F1SizeofNeta) - 0.01);
    //        //    //for (pn = 1 to maxARTParamsNo-1)  //{display first 7 parameter values}
    //        //    //LoggedConsole.WriteLine ("  (",pn,") ",paramNames[versionID, pn]:10,"  = ",parameters[pn]:7:4);
    //        //    LoggedConsole.WriteLine;
    //        //    LoggedConsole.WriteLine ("  Fault in values for simulation " + simul);
    //        //    LoggedConsole.WriteLine ("  Theta must be < 1/sqrtM ie 1/sqrt(F1size of net a)");
    //        //    LoggedConsole.WriteLine ("  Have automatically set  theta = 1/sqrtM - 0.01");
    //        //    LoggedConsole.WriteLine ("  Press any key to continue.");
    //        //    key = readkey;
    //        //}  //end;

    //        //{calc sigma and rho*}
    //        if (versionID == verART2a )
    //        {
    //            Sigma = (Params[c]*ARTParams[d]) / (1-ARTParams[d]);
    //            if (sigma > 1)
    //            {
    //                //for (pn = 1 to maxARTParamsNo-1)  //{display first 7 parameter values}
    //                //LoggedConsole.WriteLine ("  ("+pn+") "+paramNames[versionID, pn]:10+"  = "+parameters[pn]:7:4);
    //                LoggedConsole.WriteLine;
    //                LoggedConsole.WriteLine ("  Fault in values for simulation ",simul);
    //                LoggedConsole.WriteLine ("  Sigma = cd/1-d > 1.0. Choose proper values for c & d");
    //                LoggedConsole.WriteLine ("  Press any key to continue.");
    //                key = readkey;
    //                ParamvaluesOK = false;
    //                exit;
    //            }  //end;
    //        }  //end; {if verionsID = verART2a}

    //    }  //end; {for simul = 1 to noSimulationsInRun do}

    //    Close (ParamsF);
    //    if (AllOK )
    //    {
    //        LoggedConsole.WriteLine; write ("  ALL OK! Press any key to continue ... ");
    //        key = readkey;
    //        clrscr;
    //    }  //end;
    //    ParamvaluesOK = AllOK;
    //}  //end;

    //{This method finds the class which gained max Score for a given F2 unit
    //and assigns that class label to the unit. It also calculates a Class
    //probability Score ie ClassNumber/TotalNumber for each F2unit.}
//    public static void ScoreTrainingResults (noCommitted, noClasses:word; var classLabel:array of word; var classProb:array of real)
//    {
////var
//  uNo, cNo : word; {unit and class counter}
//  ScoreArray : array[1..maxClassNo] of word;
//  index, count{dummy var}, maxValue, total : word;

//for (uNo = 1 to noCommitted{nodes}) //for all units
//  {
//    //{first transfer F2 Scores to Score array. This is to avoid passing
//    // the zero index in the F2ScoreMatrix to the MaxIndex procedure}
//    total = 0;
//    for cNo = 1 to noClasses do
//    {
//      total   = total + F2ScoreMatrix^[uNo][cNo]; {sum over classes}
//      ScoreArray[cNo] = F2ScoreMatrix^[uNo, cNo];
//    }  //end;
//    {MaxIndex returns class with largest count. Random choice if =max}
//    MaxIndex (noClasses, ScoreArray, index, count, maxValue);
//    classLabel[uNo]= index+1; {+1 because array processed as 0 to n-1}

//    if total = 0
//      ) ClassProb[uNo] = 0
//      else ClassProb[uNo] = maxValue/total;
//    classLabel[0] = 0; {zero index will be used to indicate unmatched test sig}
////(***
////for cNo = 1 to noClasses do write (lst, F2ScoreMatrix^[uNo, cNo]:4);
////LoggedConsole.WriteLine (lst);
////LoggedConsole.WriteLine (lst, " max=",maxValue:3, " lbl=",classLabel[uNo]:3, " prob=",ClassProb[uNo]:4:1);
////***)
//  }  //}  //end;  {of all units}

//}  //}  //end; ScoreTrainingResults

//public static void ScoreTestResults
//{

////var
//  oneVote      : array[0..maxClassNo]  of word;
//  repeatsArray : array[1..MaxRepeatNo] of word;
//  count:word; //{(! used. Returns # of first place getters in case tied vote}
//  sigNum, rep, cls :word; {counters}
//  maxVote, maxValue{(! used}, correctClass :word;

//for sigNum = 1 to tstSetSize do {find the winning vote for every signal}
//  {
//    for cls= 0 to noClasses do oneVote[cls] = 0;      {init OneVote}
//    {transfer votes from matrix to OneVote array and ) find the maximum}
//    for rep = 1 to norepeats  do inc(oneVote[DecisionMatrix^[sigNum,rep] ]);
//    maxIndex (noClasses+1, oneVote, maxVote, count, maxValue);
//    decisionMatrix^[sigNum, 0] = maxVote;  {store vote winner in zero index}
//  }  //end; {of all signals}

//  //{summarise results for classified signals and store the test results
//   in test Score matrix}
//  for cls =  0 to noClasses+1 do
//    for rep = 0 to norepeats  do
//      tstScoreMatrix^[cls,rep] = 0;         {initialise test Score matrix}
//  for sigNum = 1 to tstSetSize do
//    for rep = 0 to norepeats do
//    {
//      correctClass = tstSetTargets^[sigNum];
//      if decisionMatrix^[sigNum, rep] = 0 )
//        inc (tstScoreMatrix^[ 0, rep]) //{skipped or no match sigs go in row 0}
//      else
//      if decisionMatrix^[sigNum, rep] = correctClass )
//      {
//        inc (tstScoreMatrix^[ correctClass, rep]);
//        inc (tstScoreMatrix^[ noClasses+1,  rep]);
//      }  //end;
//    }  //end;

//  for cls = 0 to noClasses+1 do
//  {
//   {first transfer rep Scores from matrix to an array which can be passed
//    to the moment procedure which returns the statistics.}
//    for rep = 1 to norepeats do
//          repeatsArray[rep] = tstScoreMatrix^[cls, rep];
//    moment(repeatsArray, norepeats, tstResult^[cls].mean,
//                                    tstResult^[cls].sd,
//                                    tstResult^[cls].min,
//                                    tstResult^[cls].max);
//    tstResult^[cls].vote = tstScoreMatrix^[cls,0]; {transfer vote counts}
//    tstResult^[noClasses+1].tot = tstSetSize;
//  }  //end;
//}  //}  //end; ScoreTestResults

//public static void WriteDecisionMatrix (var F: text)
//{
//    //var
//  sigNum, rep, i  : word; {counter}

//  LoggedConsole.WriteLine(F, "THE DECISION MATRIX");
//  write  (F, "sig#  vote");
//  for rep = 1 to norepeats do write(lst, rep:5);
//  LoggedConsole.WriteLine (F, "   target");
//  for i = 1 to 100 do write (F,"-"); LoggedConsole.WriteLine(F);  {draw horiz line}

//  for sigNum = 1 to tstSetSize do
//  {
//    write (F, sigNum:4,"|");
//    for rep = 0 to norepeats do
//      write (F, decisionMatrix^[sigNum, rep]:5);
//    LoggedConsole.WriteLine (F, "  |", tstSetTargets^[sigNum]:4);
//  }  //end;
//  LoggedConsole.WriteLine (F);
//  LoggedConsole.WriteLine (F, formfeed);
//} // }  //end; WriteDecisionMatrix

//public static void writeTestResults (var F:text)
//{
////var
//  k : word;
//  cls, rep: word;
//  size : word;
//  mean, sd : real;
//  min, max : word;

//  LoggedConsole.WriteLine (F, "TEST RESULTS/Score from file: ",ResultsFPath);
//  printDateAndTime (F);
//  LoggedConsole.WriteLine (F, "ART VERSION = ",versionNames[versionID]);
//  write   (F, "Training Set file = ",TrnSetFPath);
//  LoggedConsole.WriteLine (F, "   Train set size = ",trnSetSize, " signals");
//  LoggedConsole.WriteLine (F, "Train target file = ",TrnTarFPath);
//  write   (F, "Test Set     file = ",TstSetFPath);
//  LoggedConsole.WriteLine (F, "   Test  set size = ",tstSetSize, " signals");
//  LoggedConsole.WriteLine (F, "Test  target file = ",TstTarFPath);
//  LoggedConsole.WriteLine (F, "Weights file      = ",wtsFpath);
//  for k = 1 to MaxARTparamsNo do
//    write (F, paramNames[versionID, k]:9);
//  LoggedConsole.WriteLine (F);
//  for k = 1 to MaxARTparamsNo do write (F, ARTparams[k]:9:4);
//  LoggedConsole.WriteLine (F);
//  LoggedConsole.WriteLine (F);

//  write  (F, "rep number  ");
//  for rep = 1 to norepeats do write(F, rep:6);
//  LoggedConsole.WriteLine (F, "   av");
//  for k = 1 to 80 do write (F,"-"); LoggedConsole.WriteLine(F);  {draw horiz line}
//  write  (F, "categories  ");
//  for rep = 1 to norepeats do write(F, noOfCommittedF2[rep]:6);
//  moment(noOfCommittedF2, norepeats, mean, sd, min, max);
//  LoggedConsole.WriteLine (F, mean:6:1,"+/-",sd:3:1);
//  write  (F, "iter to conv");
//  for rep = 1 to norepeats do write(F, iterToConv[rep]:6);
//  moment(iterToConv, norepeats, mean, sd, min, max);
//  LoggedConsole.WriteLine (F, mean:6:1,"+/-",sd:3:1);
//  write  (F, "# skipped   ");
//  for rep = 1 to norepeats do write(F, SkippedBecauseFull[rep]:6);
//  LoggedConsole.WriteLine (F);
//  LoggedConsole.WriteLine (F);

//  size = tstResult^[noClasses+1].tot; {ie size of the test set or tstSetSize}
//  LoggedConsole.WriteLine(F, "THE TEST Score MATRIX");
//  write  (F, "class   vote");
//  for rep = 1 to norepeats do write(F, rep:6);
//  LoggedConsole.WriteLine (F);
//  for k = 1 to 80 do write (F,"-"); LoggedConsole.WriteLine(F);  {draw horiz line}
//  for cls= 0 to noClasses+1 do
//  {
//    write (F, cls:6);
//    for rep = 0 to norepeats do
//      write (F, tstScoreMatrix^[cls, rep]:6);
//    if cls = 0 ) LoggedConsole.WriteLine (F, " <- no match")
//    else
//    if cls = noClasses+1 ) LoggedConsole.WriteLine (F," <- total correct")
//    else LoggedConsole.WriteLine (F, " <- num correct in class");
//  }  //end; {end of all classes}
//  LoggedConsole.WriteLine (F);

////(****
////  LoggedConsole.WriteLine(lst);
////  LoggedConsole.WriteLine(lst,"Rep No:","":12,"No match   Classes 1 to ",noClasses,"  Total");
////  for rep=1 to norepeats do
////  {
////    write(lst, rep:2,"":9);
////    for cls = 0 to noClasses+1 do write (lst, tstScoreMatrix^[cls, rep]:5);
////    LoggedConsole.WriteLine(lst);
////  }  //end;
////  LoggedConsole.WriteLine(lst); LoggedConsole.WriteLine(lst);
////****)
//  LoggedConsole.WriteLine (F,"No match":21, "Classes 1 to ":30, noClasses, "Total":20);
//  write   (F,"totals  ");
//  for cls= 0 to noClasses+1 do  write (F, tstResult^[cls].tot:14);
//  LoggedConsole.WriteLine (F);

//  write   (F, "mean    ");
//  for cls= 0 to noClasses+1 do
//    write (F, tstResult^[cls].mean:7:1,"(",pc(tstResult^[cls].mean, size):4:1,"%)");
//  LoggedConsole.WriteLine (F);

//  write   (F, "std dev ");
//  for cls= 0 to noClasses+1 do
//    write (F, tstResult^[cls].sd:7:1,  "(",pc(tstResult^[cls].sd,   size):4:1,"%)");
//  LoggedConsole.WriteLine (F);

//  write   (F, "minimum ");
//  for cls= 0 to noClasses+1 do write (F, tstResult^[cls].min:14);
//  LoggedConsole.WriteLine (F);

//  write   (F, "maximum ");
//  for cls= 0 to noClasses+1 do write (F, tstResult^[cls].max:14);
//  LoggedConsole.WriteLine (F);

//  write   (F, "VOTE    ");
//  for cls= 0 to noClasses+1 do
//    write (F, tstResult^[cls].vote:7,"(",pc(tstResult^[cls].vote, size):4:1,"%)");
//  LoggedConsole.WriteLine (F);

//  write (F, formFeed);
//}  //}  //end;  writeTestResults (var F:text)

    //public static void MAINMENU()
    //{
    //    //var
    //    int i;  //      : word;
    //    string FPath; //  : PathStr;
    //    char choice, key;  // : char;
    //    //dirInfo: searchRec;

    //    Repeat
    //        ReadConfigFile;
    //        //TextBackGround (1);   //{blue background}
    //        //ClrScr;
    //        LoggedConsole.WriteLine();
    //        LoggedConsole.WriteLine ("                   ART MAIN MENU");
    //        LoggedConsole.WriteLine ("  All setting up to be entered in CONFIG file.");
    //        LoggedConsole.WriteLine();
    //        LoggedConsole.WriteLine ("  C) edit  CONFIGURATION file:- ",ConfigFpath);
    //        LoggedConsole.WriteLine ("  R) edit  PARAMETERS file   :- ",ParamsFpath);
    //        LoggedConsole.WriteLine ("  P) PRINT configuration file.");
    //        LoggedConsole.WriteLine ("  D) Display contents of OUTPUT directory.");
    //        LoggedConsole.WriteLine ("  T) Print TEXT file.");
    //        LoggedConsole.WriteLine ("  M) Print decision MATRIX.");
    //        LoggedConsole.WriteLine();
    //        LoggedConsole.WriteLine ("  O) OK, continue            Esc) HALT program");

        //    Repeat
        //        Choice = Upcase(readkey);
        //    Until choice in ["O", Esc, "R","C","P","D","M","T"];

        //    Case choice of
        //    "C":{
        //        SwapVectors;
        //        Exec("c:\\DOS\\edit.com", ConfigFPath);
        //        SwapVectors;
        //        If DOSError <> 0 ) LoggedConsole.WriteLine ("DOS Error # ",DosError);
        //        ClrScr;
        //        ReadConfigFile;
        //        DisplayConfiguration;
        //    }  //end;|
        //    "R":{
        //        SwapVectors;
        //        Exec("c:\\DOS\\edit.com", ParamsFPath);
        //        SwapVectors;
        //        If DOSError <> 0 ) LoggedConsole.WriteLine ("DOS Error # ",DosError);
        //        ClrScr;
        //      }  //end;
        //    "P":if (! printerOn ) Printer(!OnMessage
        //        else PrintTextFile(ConfigFName);
        //    "D":{
        //        ClrScr;
        //        LoggedConsole.WriteLine ("List of files in the directory :- ", OUTPUTdir);
        //        LoggedConsole.WriteLine;
        //        FindFirst (OUTPUTdir+"/*.*", AnyFile, DirInfo);
        //        i = 0;
        //        while (DosError = 0)  do
        //        {
        //          inc (i);
        //          write (DirInfo.name:15);
        //          FindNext (DirInfo);
        //          if (i MOD 5 = 0) ) LoggedConsole.WriteLine;
        //        }  //end;
        //        LoggedConsole.WriteLine;
        //        LoggedConsole.WriteLine ("Press <space bar> to return to main menu");
        //        key = readKey;
        //      }  //end;
        //    "T": {
        //        LoggedConsole.WriteLine;
        //        write ("   Enter full path name of text file to print -> ");
        //        readln (FPath);
        //        if (! FileExists(FPath) )
        //        {
        //          LoggedConsole.WriteLine;
        //          write (" File does (! exist!!   Press any key ->");
        //          key = readkey;
        //        end else
        //        if (! printerOn ) Printer(!OnMessage
        //                else printTextFile (FPath);
        //            }  //end;
        //    "M":if (! printerOn ) Printer(!OnMessage
        //        else writeDecisionMatrix (lst);
        //        Esc: HALT;
        //}  //end; {case choice of}

        //Until (choice = "O"{OK}) ;
    //}  //end;  MAINMENU

    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}
    //{***********************************************************************************************************************************}

                       // {*** MAIN PROGRAM ***}

        public static void Main()
    {
        Tasks task = Tasks.TRAIN;
        string wtsFname = "output";
        double[,] trainingData = null;
        int trnSetSize = 0;

        //int tstSetSize = 0;
        int F1Size = 0;
        int F2Size = 0;
        bool[] uncommittedJ = new bool[F2Size];               // : PtrToArrayOfBool;
        int[] noOfCommittedF2 = new int[numberOfRepeats];    // : array[1..MaxRepeatNo] of word;{# committed F2Neta units}
        int[] iterToConv = new int[numberOfRepeats];         // : array[1..MaxRepeatNo] of word;{for training only}

        //char key = '0';        //      : char;
        int code = 0;        //        : word; {used for getting error messages}

        //int Score = 0;       //        : word;

        //double[]  DataArray  = new double[maxDataDim];
        //double[,] dataMatrix = new double[maxTrnSetSize, maxDataDim]; //of PdataArray;
        //int[]     keepScore = new int[maxTrnSetSize]; //stores the assigned class for every input signal. Used to test for stabilisation/convergence}
        //int[] SkippedBecauseFull = new int[numberOfRepeats];  // : array[1..MaxRepeatNo] of word;

        string wtsFpath = "";
        string resultsFPath = "";   //  : PathStr;
        string trnSetFpath = "";

        //string trnTarFpath = ""; //  : pathStr;
        string tstSetFpath = "";

        //string tstTarFpath = ""; //  : pathStr;
        //bool targetFileExists; // : boolean;
        //string ARTVersion;     //   : string[50];
        //int VersionID;         //    : integer;
        //int[] F2classLabel = new int[maxF2Size];       //: array [0..maxF2Size] of word;
        //double[] F2classProb = new double[maxF2Size];  // : array [0..maxF2Size] of real;
        //int KeepScore; //    : PKeepScore;

        //{************************** INITIALISE VALUES *************************}

        //F2ScoreMatrix = new ();   //{used to assign classes to F2 nodes after training}
        //decisionMatrix = new ();  //{matrix: tst sig number x repeats }
        //tstScoreMatrix = new ();  //{matrix: tst sig class  x repeats }
        //tstResult = new ();       //{array of record: class x tst Score results}
        //tstSetTargets = new ();

        //CONFIGURE AND CHECK PARAMETERS
        //ReadConfigFile;
        //code = 0;
        //CheckConfiguration (code);
        //if(code == 0) ParamValuesOK(versionID);

        if (task == Tasks.TRAIN || task == Tasks.TRAINTEST)
        {
            trnSetFpath = dataFpath;
            trainingData = FileTools.ReadDoubles2Matrix(dataFpath);
            trnSetSize = trainingData.GetLength(0);
            F1Size = trainingData.GetLength(1);
            F2Size = trainingData.GetLength(0);
        }

        double[,] parameters = ReadParameterValues(paramsFpath);
        int simulationsCount = parameters.GetLength(0);
        int paramCount = parameters.GetLength(1);

        ART_2A art2a = new ART_2A(F1Size, F2Size);

        if (task == Tasks.TEST) // {load the test file weights}
        {
            //Case versionID of
            //    verART2A      : ReadWtsART2a    (wtsFPath, F1SizeOfNeta, F2SizeOfNeta, F2classLabel, F2classProb, code);
            //    verFuzzyART   : ReadWtsFuzzyART (wtsFPath, F1SizeOfNeta, F2SizeOfNeta, F2classLabel, F2classProb, code);
            //    verARTMAP2a   : { {ReadWtsARTMAP2a;} }  //end;
            //    verFuzzyARTMAP: ReadWtsFuzzyARTMAP(wtsFPath, F2classLabel, F2classProb, code);
            //}  //end; {Case Les.algID of}
            //ReadWtsART2a    (wtsFPath, F1SizeOfNeta, F2SizeOfNeta, F2classLabel, F2classProb, code);
        }

        //if (code != 0)
        //{
        //    LoggedConsole.WriteLine("WARNING!!!!!!!! ERROR READING WTS FILE.");
        //    LoggedConsole.WriteLine(" F1 & F2 sizes in config file and wts file are NOT equal");
        //    LoggedConsole.WriteLine(" Press any key to return to main menu");
        //    key = readkey;
        //    goto TheBeginning;
        //}

        //{Initialise screen for graphics display of F2 weight graphs}
        //InitialiseGraphicsMode;

        //{********** DO SIMULATIONS WITH DIFFERENT PARAMETER VALUES ***********}
        for (int simul = 0; simul < simulationsCount; simul++)
        {
            //pass the eight params for this run of ART2A
            //alpha, beta, rho, theta, rhoStar
            art2a.SetParameterValues(parameters[simul, 0], parameters[simul, 1], parameters[simul, 2], parameters[simul, 3]);

            //set up file name for simulation test results}
            resultsFPath = ARTDir + wtsFname + "s" + simul.ToString("D2") + "_results.txt";

            //init array to count committed F2 nodes
            //int[] noOfCommittedF2 = new int[ART.numberOfRepeats];

            //initialise decision matrix for processing test data}
            //int[,] decisionMatrix = new int[tstSetSize,norepeats];  //{matrix: tst sig number x repeats }

            //{********** DO REPEATS ***********}
            for (int rep = 0; rep < numberOfRepeats; rep++)
            {
                LoggedConsole.WriteLine ("RUN=", simul, " rep=", rep);

                //{********* RUN NET for ONE SET OF PARAMETERS for ALL ITERATIONS *********}
                if (task == Tasks.TRAIN)
                {
                    art2a.InitialiseArrays();
                    code = 0;
                    art2a.TrainNet(trainingData, maxIterations, simul, rep, code);

                    if (code != 0)
                        {
                            break;
                        }

                    noOfCommittedF2[rep] = art2a.CountCommittedF2Nodes();

                    //ScoreTrainingResults (noOfCommittedF2[rep], noClasses, F2classLabel, F2classProb);

                    wtsFpath = ARTDir + ART.wtsFname + "s" + simul + rep + wtsFExt;

                    //art2a.WriteWts(wtsFpath, F2classLabel, F2classProb);
                    if (DEBUG)
                        {
                            LoggedConsole.WriteLine("wts= " + wtsFpath + "  train set= " + trnSetFpath);
                        }
                    }

                if (task == Tasks.TEST)
                {
                    //{wts file was loaded above.
                    //art2a.TestNet(testData, simul, rep, code) ;
                    //if (code != 0) goto EndOfSimulations;
                    //if (DEBUG) LoggedConsole.WriteLine("wts= " + wtsFpath + "  test set= " + tstSetFpath);
                }

                if (task == Tasks.TRAINTEST)
                {
                    //Case versionID of        {initialise the weight arrays}
                    //verART2A      : InitWtsART2a;
                    //verFuzzyART   : InitWtsFuzzyART;
                    //{   verARTMAP2a   : InitWtsARTMAP2a;  }
                    //verFuzzyARTMAP: InitWtsFuzzyARTMAP;
                    //}  //Case Les.algID of}
                    art2a.InitialiseArrays();
                    art2a.TrainNet(trainingData, maxIterations, simul, rep, code);

                    if (code != 0)
                        {
                            break;
                        }

                    noOfCommittedF2[rep] = art2a.CountCommittedF2Nodes();

                    //ScoreTrainingResults (noOfCommittedF2[rep], noClasses, F2classLabel, F2classProb);

                    wtsFpath = ARTDir + ART.wtsFname + "s" + simul + rep + wtsFExt;

                    //Case versionID of
                    //verART2A      : WriteWtsART2a (wtsFPath, F1SizeOfNeta, F2SizeOfNeta, F2classLabel, F2classProb);
                    //verFuzzyART   : WriteWtsFuzzyART(wtsFPath, F1SizeOfNeta, F2SizeOfNeta,F2classLabel, F2classProb);
                    //verFuzzyARTMAP: WriteWtsFuzzyARTMAP(wtsFPath, F2classLabel, F2classProb);
                    //}  //Case Les.algID of}
                    //art2a.WriteWts(wtsFPath, F2classLabel, F2classProb);

                    //art2a.TestNet(testData, simul, rep, code);
                    //if (code != 0) goto EndOfSimulations;
                    if (DEBUG)
                        {
                            LoggedConsole.WriteLine("wts= " + wtsFpath + "  test set= " + tstSetFpath + "  Press any key");
                        }
                    }

                    //{************** DISPLAY RECONSTRUCTED SIGNALS **************}

                    //for F2uNo = 1 to F2SizeOfNeta do   {Calculate the average signal values}
                    //{
                    //  {for j = 0 to noClasses+1 do write (lst, F2ScoreMatrix^[F2uNo][j]:4);
                    //  LoggedConsole.WriteLine (lst);}
                    //  Score = F2ScoreMatrix^[F2uNo][noClasses+1];
                    //  for F1uNo = 1 to F1SizeOfNeta do
                    //    if  Score = 0 )
                    //         avSig^[F2uNo]^[F1uNo] = 0.0
                    //    else avSig^[F2uNo]^[F1uNo] = avSig^[F2uNo]^[F1uNo] /Score;
                    //}  //end;

                    //{*********** FinalScreenDisplay ***********}
                    //if (DEBUG)FinalScreenDisplay(F1sizeOfNeta,F2SizeOfNeta,F2ScoreMatrix,avSig^,WtsNeta^);
                    //{
                    //    InitialiseDisplayOfF2Weights (F1sizeOfNeta, noOfCommittedF2[rep]);
                    //    for (int F2uNo = 0; F2uNo < noOfCommittedF2[rep]; F2uNo++)
                    //    {
                    //        DisplayF2WtsAndSig(F2uNo, F1sizeOfNeta, noOfCommittedF2[rep], F2ScoreMatrix[F2uNo,noClasses+1],
                    //                    avSig[F2uNo], wtsNetA[F2uNo], avSigRange, wtsRange);
                    //    }
                    //} //end; {if display on do final display}
              } //end; {for rep   = 1 to norepeats do}       {***** END OF REPEATS *****}

              //ScoreTestResults;
              //if (printTestResults) writeTestResults(lst); //            {write to printer.....}
              //else   //{...else write to file}
              //{
              //  assign (resultsF, resultsFPath);
              //  rewrite(resultsF);
              //  writeTestResults (resultsF);
              //  close  (resultsF);
              //}  //end;

              //if (printDecisionMatrix) writeDecisionMatrix(lst);
          } //end; {for simul = 1 to noSimulationsInRun do}  {**** END OF SIMULATE *****}

          //if (printerOn) PrintTextFile(ConfigFpath);

          //{LABEL} EndOfSimulations:
          //close  (ParamsF);
          //CloseGraph;
          //Case versionID of
          //  verART2A      : DisposeART2a;
          //  verFuzzyART   : DisposeFuzzyART;
          //{  verARTMAP2a   : DisposeARTMAP2a;}
          //  verFuzzyARTMAP: DisposeFuzzyARTMAP;
          //}  //end; {Case Les.algID of}

          //GoTo TheBeginning;

          //    {****************** DISPOSE OF HEAP VARIABLES ******************}
          //for j = 1 to MaxTrnSetSize do dispose (DataMatrix[j]);
          //dispose (keepScore);
          //dispose (F2ScoreMatrix);   {used to assign classes to F2 nodes}
          //dispose (decisionMatrix);  {matrix: tst sig number x repeats }
          //dispose (tstScoreMatrix);  {matrix: tst sig class  x repeats }
          //dispose (tstResult);       {array of record: class x tst Score results}
          //dispose (TstSetTargets);
        } //END of MAIN METHOD.
    }// end class
}//end namespace

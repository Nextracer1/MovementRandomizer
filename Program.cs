using System;
using System.IO;
using System.Collections;
using System.Security.Claims;
using System.Reflection;


namespace TF2MovementRandomizer
{
    internal class Program
    {
        static Random rnd = new Random();
        static bool debug = false;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "TF2 Movement Randomizer";


            #region setup


            if (!File.Exists("config.txt"))
            {
                WriteError("config.txt doesn't exist in working dir! Make sure it's in " + System.AppDomain.CurrentDomain.BaseDirectory, true);
            }


            Console.Write("Enter titanfall 2 install location (ex. C:\\Program Files (x86)\\Origin Games\\Titanfall2) ~> ");

            string cfgFolder = (Console.ReadLine().Trim()) + @"\r2\cfg";

            if (!Directory.Exists(cfgFolder))
            {
                WriteError("Couldn't find an r2\\cfg folder from the given install location. Make sure install folder is correct, you have access permissions, and if r2\\cfg doesn't exist, create it!", true);
            }

            #endregion



            Console.WriteLine("...");

            List<MovementValue> commandsToWrite = GenerateConvarListFromConfig();
            WriteConvarListToCfg(commandsToWrite, cfgFolder + @"\randomizer.cfg");

            Console.WriteLine("Randomized! Run 'exec randomizer' ingame to play.");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
        }




        public static List<MovementValue> GenerateConvarListFromConfig()
        {
            List<MovementValue> generatedList = new List<MovementValue>();

            try
            {
                int lineNumber = 0;
                //FileStream config = File.OpenRead("config.txt");
                var config = File.ReadLines("config.txt");
                
                foreach (string line in config)
                {
                    if (String.IsNullOrEmpty(line)) continue;
                    DebugLog("processing this line: " + line);


                    MovementValue movementValueInQuestion = new MovementValue();
                    char[] lineChars = line.ToCharArray();
                    int readFrom = 0;
                    int readSecondFrom = 0;
                    string processingStr = "";    // temp use to get ints/floats to convert properly

                    movementValueInQuestion.isInteger = line.StartsWith("!");
                    movementValueInQuestion.isDoubleRange = line.Contains('*');

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (movementValueInQuestion.isInteger)
                    {
                        DebugLog("is integer");
                        // case: value needs to be integer
                        // skip first char

                        for (int i = 1; lineChars[i] != ' '; i++)
                        {
                            movementValueInQuestion.convar += lineChars[i];
                            readFrom = i;
                        }

                    }

                    else
                    {
                        // case: value needs to be float
                        // handle normally 

                        for (int i = 0; lineChars[i] != ' '; i++)    // ^
                        {
                            movementValueInQuestion.convar += lineChars[i];
                            readFrom = i;
                        }
                    }


                    DebugLog("read convar as: " + movementValueInQuestion.convar);
                    processingStr = "";


                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    for (int i = readFrom + 2; lineChars[i] != ' ' && lineChars[i] != '*'; i++)  // do the same for each property of movementValueInQuestion
                    {
                        processingStr += lineChars[i];
                        readSecondFrom = i;
                    }


                    if (movementValueInQuestion.isInteger)
                    {
                        movementValueInQuestion.lowerRange = Convert.ToInt32(processingStr);
                    }

                    else
                    {
                        movementValueInQuestion.lowerRange = Convert.ToSingle(processingStr);
                    }


                    DebugLog("read lower range as: " + movementValueInQuestion.lowerRange);



                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (movementValueInQuestion.isDoubleRange)
                    {
                        DebugLog("is double range");
                        // case: double range 

                        string[] potentialRangesRaw = line.Split('*');
                        string potentialRange1 = potentialRangesRaw[0].Split(' ')[1] + " " + potentialRangesRaw[0].Split(' ')[2];    // ..........................
                        string potentialRange2 = potentialRangesRaw[1].Trim();
                        string rangeToUse;

                        DebugLog("split double range string from\n" + potentialRangesRaw[0] + " ///// " + potentialRangesRaw[1] + "\ninto:\n" + potentialRange1 + " ///// " + potentialRange2);


                        if (CoinFlip()) rangeToUse = potentialRange1;
                        else rangeToUse = potentialRange2;


                        if (movementValueInQuestion.isInteger)
                        {
                            movementValueInQuestion.lowerRange = Convert.ToInt32(rangeToUse.Split(' ')[0]);
                            movementValueInQuestion.upperRange = Convert.ToInt32(rangeToUse.Split(' ')[1]);
                        }

                        else
                        {
                            movementValueInQuestion.lowerRange = Convert.ToSingle(rangeToUse.Split(' ')[0]);
                            movementValueInQuestion.upperRange = Convert.ToSingle(rangeToUse.Split(' ')[1]);
                        }
                    }


                    else
                    {
                        // otherwise, can just read to the end of the line now
                        processingStr = line.Substring(readSecondFrom + 2);

                        if (movementValueInQuestion.isInteger)
                        {
                            movementValueInQuestion.upperRange = Convert.ToInt32(processingStr);
                        }

                        else
                        {
                            movementValueInQuestion.upperRange = Convert.ToSingle(processingStr);
                        }

                        DebugLog("read upper range as: " + movementValueInQuestion.upperRange);
                        processingStr = ""; // just make sure lol
                    }




                    // decide on value

                    if (movementValueInQuestion.isInteger)
                    {
                        movementValueInQuestion.value = Convert.ToInt32(rnd.Next(Convert.ToInt32(movementValueInQuestion.lowerRange),
                                                                                    Convert.ToInt32(movementValueInQuestion.upperRange + 1))); // ugh
                    }

                    else
                    {
                        movementValueInQuestion.value = GetRandomFloat(Convert.ToSingle(movementValueInQuestion.lowerRange), Convert.ToSingle(movementValueInQuestion.upperRange));
                        //movementValueInQuestion.value = Convert.ToSingle();
                    }


                    generatedList.Add(movementValueInQuestion);
                    lineNumber++;

                    DebugLog("added movement value " + movementValueInQuestion.convar + " " + movementValueInQuestion.value + " (" + movementValueInQuestion.lowerRange + " - " + movementValueInQuestion.upperRange + ")\n");
                }

            }

            catch (Exception e)
            {
                WriteError("Failed to get convar list from config!\nException: " + e.Message, true);
            }


            return generatedList;
        }




        public static void WriteConvarListToCfg(List<MovementValue> convarList, string cfgPath)
        {
            DebugLog("\n\n\n----------\n\n\n" + cfgPath + "\n\n\n----------\n\n\n");

            try
            {
                // by this point should have validated that we're in the right folder to be creating this
                if (!File.Exists(cfgPath))
                {
                    Console.WriteLine("randomizer.cfg doesn't exist, creating");
                    Thread.Sleep(1000); // hack
                }

                File.WriteAllText(cfgPath, "sv_cheats 1\n");


                string commandToWrite = "";

                foreach (MovementValue mv in convarList)
                {
                    if (mv.convar.ToLower() == "airacceleration" || mv.convar.ToLower() == "airspeed")
                    {
                        commandToWrite = "ent_fire !self addoutput \"" + mv.convar + " " + mv.value + "\"";
                    }

                    else
                    {
                        commandToWrite = mv.convar + " " + mv.value;
                    }

                    DebugLog("\nwriting line '" + commandToWrite + "' to cfg");

                    File.AppendAllText(cfgPath, commandToWrite + "\n");
                    //Thread.Sleep(10);
                }
            }

            catch (Exception e)
            {
                WriteError("There was an error writing the shuffled covnars to the .cfg!\nException: " + e.Message, true);
            }


            DebugLog("finished writing!");
        }




        #region helpers

        public static void WriteError(string msg, bool exit = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;


            if (exit)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                Environment.Exit(0);
            }
        }


        public static void DebugLog(string msg, bool endl = true)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            if (debug) Console.Write(msg);
            if (debug && endl) Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
        }



        public static bool CoinFlip() => rnd.Next(101) < 50;



        public static float GetRandomFloat(float min, float max)
        {
            return Single.Round(Convert.ToSingle(rnd.NextDouble() * (max - min) + min), 2);
        }


        #endregion
    }



    class MovementValue
    {
        public string convar { get; set; }
        public float lowerRange { get; set; }
        public float upperRange { get; set; }
        public float value { get; set; }
        public bool isInteger { get; set; }
        public bool isDoubleRange { get; set; }


        // todo: make the default convar echo a warning if it eventually gets run since it means something broke
        public MovementValue(string _convar = "", float _lowerRange = 0, float _upperRange = 0, float _value = 0, bool _isInteger = false, bool _isDoubleRange = false)
        {
            this.convar = _convar;
            this.lowerRange = _lowerRange;
            this.upperRange = _upperRange;
            this.value = _value;
            this.isInteger = _isInteger;
            this.isDoubleRange = _isDoubleRange;
        }
    }
}

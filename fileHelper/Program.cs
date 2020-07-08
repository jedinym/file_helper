using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Resources;
using System.Globalization;

namespace fileHelper
{
    class Program
    {
        static int Main(string[] args)
        {
            Stopwatch sw = new Stopwatch(); //for logs purposes
            sw.Start();

            string logPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\fileHelper_Log.txt"; //To avoid problems when running in command line
 

            if (args.Count() == 7)
            {
                if (CanWriteToOutputPath(args[6]))
                {
                    logPath = args[6];
                }
                else
                {
                    Environment.Exit(1007); //cant write logs to specified filepath
                }
            }

            

            if (args.Count() < 6)
            {
                Log(logPath, "ERROR 1001");
                Environment.Exit(1001); //Main() was passed too few arguments
            }
            else if (args.Count() > 7)
            {
                Log(logPath, "ERROR 1002");
                Environment.Exit(1002); //Main() was passed too many arguments
            }

            if (!CanWriteToOutputPath(logPath))
            {
                //Log(logPath, "ERROR 1007");
                Environment.Exit(1007); //cant write logs to specified filepath
            }

            string columnToFind = args[0];

            string pathA = args[1];
            string pathB = args[2];
            string pathC = args[3];
            string pathD = args[4];

            string outputPath = args[5];

            if (GetLogRunCount(logPath) >= 100)
            {
                DeleteOldestLogFromFilePath(logPath);
            }

            Log(logPath, "Inputs: " + columnToFind); //LOG: inputs

            //ErrorLog(logPath, 504);

            int inputPathCode = InputPathsExist(args.Skip(1).Take(4).ToArray()); //skip column identifier, take input paths

            switch (inputPathCode) //make sure this works
            {
                case 0:
                    Log(logPath, "ERROR 1010");
                    Environment.Exit(1010); //first input path doesnt exist
                    break;
                case 1:
                    Log(logPath, "ERROR 1011");
                    Environment.Exit(1011); //second input path does not exist
                    break;
                case 2:
                    Log(logPath, "ERROR 1012");
                    Environment.Exit(1012); //third input path does not exist
                    break;
                case 3:
                    Log(logPath, "ERROR 1013");
                    Environment.Exit(1013); //fourth input path does not exist
                    break;
                default:
                    Log(logPath, "All input paths exist");
                    break;
            }


            if (!CanWriteToOutputPath(outputPath))
            {
                Log(logPath, "ERROR 1006");
                Environment.Exit(1006); //unauthorized access to output file (argument 6)
            }


            Log(logPath, "Lines in file B without header: " + (File.ReadLines(pathB).Count() - 1));

            int? columnNullIdB = GetColumnId(columnToFind, pathB); // if column not found, method returns null
            int? columnNullIdA = GetColumnId(columnToFind, pathA);
            int? columnNullIdC = GetColumnId(columnToFind, pathC);

            if (columnNullIdB == null) //TODO: CHECK IF COLUMN IN ALL FILES
            {
                Log(logPath, "ERROR 1000");
                Environment.Exit(1000); //column not found in file B
            }
            if (columnNullIdA == null)
            {
                Log(logPath, "ERROR 1014");
                Environment.Exit(1014); //column not found in file A
            }
            if (columnNullIdC == null)
            {
                Log(logPath, "Error 1015");
                Environment.Exit(1015); //column not found in file C
            }

            int columnIdB = columnNullIdB.Value;//converted from nullable Int32 to non-nullable Int32
            int columnIdA = columnNullIdA.Value;//converted from nullable Int32 to non-nullable Int32
            int columnIdC = columnNullIdC.Value;//converted from nullable Int32 to non-nullable Int32


            string[] columnArrayA = GetArrayFromFilePath(pathA, columnIdA);
            string[] columnArrayC = GetArrayFromFilePath(pathC, columnIdC);

            Log(logPath, "Total lines in file A without header: " + (columnArrayA.Count() - 1));

            bool rowExistsInA;
            bool rowExistsInC;

            List<string> foundAColumnMatches = new List<string>();
            List<string> foundCColumnMatches = new List<string>();

            StreamWriter outputFile = new StreamWriter(outputPath);
            Log(logPath, "Output file stream opened succesfully");
            outputFile.WriteLine(GetHeaderFromFilePath(pathB)); //copy header from file B to output
            Log(logPath, "Header copied to output file");

            int deletedBasedOnACount = 0;
            int leftBasedOnCCount = 0;


            foreach (string line in File.ReadLines(pathB).Skip(1)) //skip heading of file B
            {
                string column = line.Split(',')[columnIdB];

                rowExistsInA = false;
                rowExistsInC = false;

                if (foundAColumnMatches.Contains(column))
                {
                    rowExistsInA = true;
                }
                else
                {
                    for (int i = 0; i < columnArrayA.Count(); ++i) //skip heading
                    {
                        if (column == columnArrayA[i])
                        {
                            rowExistsInA = true;
                            foundAColumnMatches.Add(column);
                            break;
                        }
                    }
                } //finished scanning A

                if (rowExistsInA)
                {
                    ++deletedBasedOnACount;
                    continue;
                }
                    

                if (foundCColumnMatches.Contains(column))
                {
                    rowExistsInC = true;
                    ++leftBasedOnCCount;
                }
                else
                {
                    for (int i = 0; i < columnArrayC.Count(); ++i) //skip heading
                    {
                        if (column == columnArrayC[i])
                        {
                            rowExistsInC = true;
                            ++leftBasedOnCCount;
                            foundCColumnMatches.Add(column);
                            break;
                        }
                    }
                } //finished scanning C

                if (!rowExistsInA && rowExistsInC)
                {
                    outputFile.WriteLine(line);
                }
            }

            Log(logPath, "Lines deleted from file B based on file A: " + deletedBasedOnACount);
            Log(logPath, "Total lines in file C without header: " + (columnArrayC.Count() - 1));
            Log(logPath, "Lines left in file B based on file C: " + leftBasedOnCCount);


            if (GetHeaderFromFilePath(pathB) == GetHeaderFromFilePath(pathD)) //Only copy fileD to output if headers match
            {
                foreach (string line in File.ReadLines(pathD).Skip(1)) //copy file D to output without heading
                {
                    outputFile.WriteLine(line);
                } //finished writing file D to output
                Log(logPath, "Finished writing to file D");
            }
            else
            {
                Log(logPath, "ERROR 1005");
                Environment.Exit(1005); // headers D and B don't match
            }


            outputFile.Close();
            Log(logPath, "Output file closed");
            sw.Stop();
            Log(logPath, "Operation complete in: " + sw.Elapsed);
            File.AppendAllText(logPath, Environment.NewLine); // end log with line separator

            return 0; // program executed properly
        }

        static int InputPathsExist(string[] _args)
        {
            for (int i = 0; i < _args.Count(); ++i)
            {
                if (!File.Exists(_args[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        static bool CanWriteToOutputPath(string _path)
        {
            try
            {
                using (FileStream fs = new FileStream(_path, FileMode.Append, FileAccess.Write))
                using (StreamWriter test = new StreamWriter(fs))
                {
                    test.Write(string.Empty);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }

        static int? GetColumnId(string _column, string _path) //maybe check if column in every file
        {
            string header = File.ReadLines(_path).Take(1).FirstOrDefault();
            string[] headers = header.Split(',');

            for (int i = 0; i < headers.Count(); ++i)
            {
                if (headers[i] == _column)
                    return i;
            }

            return null; //Column not found in file
        }

        static string GetHeaderFromFilePath(string _path)
        {
            return File.ReadLines(_path).FirstOrDefault();
        }

        static string[] GetArrayFromFilePath(string _path, int columnId, bool _skipHeading = true)
        {
            int skip = 0;
            if (_skipHeading)
                skip = 1;

            int arrayLen = File.ReadLines(_path).Count();

            List<string> columnStringList = new List<string>();

            foreach (string line in File.ReadLines(_path).Skip(skip)) //skip heading
            {
                columnStringList.Add(line.Split(',')[columnId]);
            }

            return columnStringList.ToArray();
        }

        static void Log(string _logPath, string _content)
        {
            _content = _content.TrimStart();

            File.AppendAllText(_logPath, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + _content + Environment.NewLine);
        }

        //static void ErrorLog(string _logPath, int _errorCode) //TODO: figure this out
        //{
        //    ResourceManager rm = new ResourceManager("ErrorCodes", Assembly.GetCallingAssembly());
        //    Assembly v = Assembly.GetCallingAssembly();

        //    string a = rm.GetString("EC1000");


        //    bool f = false;
        //}

        static int GetLogRunCount(string _logPath)
        {
            int emptyLineCount = 0;

            foreach (string line in File.ReadLines(_logPath))
            {
                if (line == string.Empty)
                {
                    ++emptyLineCount;
                }
            }

            return emptyLineCount;
        }

        static void DeleteOldestLogFromFilePath(string _logPath)
        {
            List<string> logList = new List<string>();

            foreach (string line in File.ReadLines(_logPath))
            {
                logList.Add(line);
            }

            bool firstLinePassed = false;
            List<int> toDelete = new List<int>();

            for (int i = 0; i < logList.Count(); ++i)
            {
                // firstLinePassed = logList[i] == string.Empty;

                if (logList[i] == string.Empty)
                    firstLinePassed = true;

                if (!firstLinePassed)
                {
                    logList[i] = null;
                    continue;
                }
                else
                {
                    logList[i] = null; //to delete the blank line as well
                    break; // skip reading rest of the file
                }
            }


            logList.RemoveAll(item => item == null);

            File.WriteAllLines(_logPath, logList);
        }
    }
}

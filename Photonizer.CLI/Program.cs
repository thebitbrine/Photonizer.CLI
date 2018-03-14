using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Photonizer.CLI
{
    class Program
    {

        public static string Source = System.Environment.CurrentDirectory;
        public static string Output = System.Environment.CurrentDirectory + @"\+Sorted+";
        public static bool IsCopy = true;
        public static string Exts = ".jpg";
        //---Thread Ui Updates---
        public static string Log = "";
        public static string[] Info = new string[10];
        public static bool Run = false;
        //---Threaded Output---
        static List<FileInfo> InList = new List<FileInfo>();
        static List<FileInfo> OutList = new List<FileInfo>();

        public static void Main(string[] args)
        {
            Console.Title = "Photonizer - Version 0.2b";
            Console.WriteLine(@"    ___ _           _              _              ");
            Console.WriteLine(@"   / _ \ |__   ___ | |_ ___  _ __ (_)_______ _ __ ");
            Console.WriteLine(@"  / /_)/ '_ \ / _ \| __/ _ \| '_ \| |_  / _ \ '__|");
            Console.WriteLine(@" / ___/| | | | (_) | || (_) | | | | |/ /  __/ |   ");
            Console.WriteLine(@" \/    |_| |_|\___/ \__\___/|_| |_|_/___\___|_|   ");
            Console.WriteLine(@"                                                  ");
            Console.WriteLine(" -Organizing Images Into Directory Trees by Date-  ");
            Console.WriteLine("             -Built By @TheBitBrine-              ");
            Console.WriteLine();

            Console.Write("Source Directory (default: ./): ");
            Source = Console.ReadLine();
            if(IsNullOrWhiteSpace(Source) == true) Source = System.Environment.CurrentDirectory;

            Console.Write("Output Directory Root (default: ./+Sorted+): ");
            Output = Console.ReadLine();
            if (IsNullOrWhiteSpace(Output) == true) Output = System.Environment.CurrentDirectory + @"\+Sorted+";

            Console.Write("File Extension (default: .jpg):");
            Exts = Console.ReadLine().ToLower();
            if (IsNullOrWhiteSpace(Exts) == true) Exts = ".jpg";

            Console.Write("Keep Source Files? (default: Yes):");
            string KSF = Console.ReadLine().ToLower();
            if (KSF.Contains("n"))
                IsCopy = false;
            else
                IsCopy = true;

            Run = true;

            MashIt();

            while (Run == true)
            {
                if (IsNullOrWhiteSpace(Log) == false)
                Console.Write(Log + "                                 \r");
            }

            Console.WriteLine("Organized " + InList.Count + " Files Successfully...");
            Console.Write("Exiting Now...");
            Thread.Sleep(3000);

        }


        public static void MashIt()
        {
            //Discover Files
            DirSearch(Source);
            string[] TempExt = Exts.Replace(" ", "").Split(',');
            for (int i = 0; i < InList.Count; i++)
            {
                Log = "Checking: " + i + "/" + InList.Count;

                for (int x = 0; x < TempExt.Length; x++)
                {
                    if (IsNullOrWhiteSpace(InList[i].Extension) == false && TempExt[x].Contains(InList[i].Extension.ToLower()) == true)
                    {
                        OutList.Add(InList[i]);
                    }
                }
            }

            Console.WriteLine();

            List<List<FileInfo>> MasterList = SplitList(OutList, OutList.Count / 1);
            //for (int i = 0; i < MasterList.Count ; i++)
            //{
            Thread TH = new Thread(() => CopyMove(MasterList[0], 0));
            TH.IsBackground = true;
            TH.Start();
            // }

            //Create Folders & Copy/Move Them

            //Run = false;
        }


        public static void CopyMove(List<FileInfo> Input, int ID)
        {
            for (int i = 0; i < Input.Count; i++)
            {
                Info[ID] = i + "," + Input.Count + "," + true;

                //if (Run == false)
                //break;
                string Year = Input[i].CreationTime.Year + "";
                string Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Input[i].CreationTime.Month);
                string Day = Input[i].CreationTime.Day + "";

                CreateFolder(Output + "\\" + Year);
                CreateFolder(Output + "\\" + Year + "\\" + Month);
                CreateFolder(Output + "\\" + Year + "\\" + Month + "\\" + Day);


                try
                {
                    if (IsCopy == true)
                    {
                        if (File.Exists(Output + "\\" + Year + "\\" + Month + "\\" + Day + "\\" + Year + "." + Input[i].CreationTime.Month + "." + Day + " (" + Input[i].Name.Replace(Input[i].Extension, "") + ")" + Input[i].Extension) == false)
                            File.Copy(Input[i].FullName, Output + "\\" + Year + "\\" + Month + "\\" + Day + "\\" + Year + "." + Input[i].CreationTime.Month + "." + Day + " (" + Input[i].Name.Replace(Input[i].Extension, "") + ")" + Input[i].Extension);
                        Log = "Copying: " + i + "/" + Input.Count;
                    }
                    else
                    {
                        if (File.Exists(Output + "\\" + Year + "\\" + Month + "\\" + Day + "\\" + Input[i].Name) == false)
                            File.Move(Input[i].FullName, Output + "\\" + Year + "\\" + Month + "\\" + Day + "\\" + Input[i].Name);
                        Log = "Moving: " + i + "/" + Input.Count;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }


            Info[ID] = 0 + "," + Input.Count + "," + false;

            Run = false;

        }

        public static List<List<FileInfo>> SplitList(List<FileInfo> locations, int nSize = 30)
        {
            var list = new List<List<FileInfo>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static void DirSearch(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        InList.Add(new FileInfo(f));
                    }
                    DirSearch(d);
                }
                foreach (string f in Directory.GetFiles(sDir))
                {
                    InList.Add(new FileInfo(f));
                }
            }
            catch (System.Exception excpt)
            {
                //List.Add(excpt.Message);
            }

        }


        public static void CreateFolder(string FolderPath)
        {
            if (Directory.Exists(FolderPath) == false)
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}

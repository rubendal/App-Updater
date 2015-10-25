using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 4 || args.Length == 5)
            {
                //Default use:
                //args[0]=> zip or exe
                //args[1]=> Process Name
                //args[2]=>Executable file path
                //args[3]=>New executable file path
                //args[4]=>Zip extracted files directory

                //First check if args[2] and args[3] are valid file paths
                if (!File.Exists(args[2]) && !File.Exists(args[3]))
                {
                    Console.WriteLine("Original executable and/or new executable doesn't exist");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    Thread.Sleep(2000);
                    string path = Path.GetFileNameWithoutExtension(args[2]);
                    //Check if the executable process has finished
                    Console.WriteLine(string.Format("Waiting for {0} process to finish...", args[1]));
                    if (Process.GetProcessesByName(args[1]).Length != 0)
                    {
                        //Wait 1 sec and then kill it if it is still alive...
                        Thread.Sleep(1000);
                        if (Process.GetProcessesByName(args[1]).Length != 0)
                        {
                            Console.WriteLine("Wait time exceeded, killing process...");
                            foreach (var process in Process.GetProcessesByName(args[1]))
                            {
                                process.Kill();
                            }
                        }
                    }
                    
                    if (args[0] == "zip")
                    {
                        //Prepare zip files
                        foreach(string file in Directory.GetFiles(args[4]))
                        {
                            MoveFiles(file.Replace(args[4], "").TrimStart('\\'), args[4]);
                        }
                        foreach(string d in Directory.GetDirectories(args[4]))
                        {
                            string dir = d.Replace(args[4], "").TrimStart('\\');
                            if (!Directory.Exists(Path.Combine(GetCurrentDirectory(), dir)))
                            {
                                Directory.CreateDirectory(Path.Combine(GetCurrentDirectory(), dir));
                            }
                            foreach (string file in Directory.GetFiles(d))
                            {
                                MoveFiles(file.Replace(args[4], "").TrimStart('\\'), args[4]);
                            }
                        }
                        Directory.Delete(args[4],true);
                    }
                    else
                    {
                        //Delete previous exe and rename new one
                        Console.WriteLine("Deleting previous executable...");
                        File.Delete(args[2]);
                        Console.WriteLine("Renaming new executable...");
                        File.Move(args[3], args[2]);
                    }
                    Console.WriteLine("Done!\nPress any key to start " + args[2]);
                    Console.ReadKey();
                    Process.Start(path);
                }
                catch (Exception e)
                {
                    //Some error occurred... notify user
                    Console.WriteLine(e.Message.ToString()  + "\n" + e.StackTrace + "\nPress any key to close...");
                    Console.ReadKey();
                }
                
            }

        }

        static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        static bool PathIsThisUpdater(string path)
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location == path;
        }

        static void MoveFiles(string file, string dir)
        {
            if (!PathIsThisUpdater(Path.GetFileName(file)))
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                Console.WriteLine("Moving " + Path.GetFileName(file));
                File.Move(Path.Combine(dir,file), file);
            }
        }
    }
}

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
            if (args.Length == 3)
            {
                //Default use: args[0]=>Program process name without .exe
                //args[1]=>Executable file path
                //args[2]=>New executable file path

                //First check if args[1] and args[2] are valid file paths
                if (!File.Exists(args[1]) && !File.Exists(args[2]))
                {
                    Console.WriteLine("Original executable and/or new executable doesn't exist");
                    return;
                }
                try
                {
                    Thread.Sleep(2000);
                    string path = Path.GetFileNameWithoutExtension(args[1]);
                    //Check if the executable process has finished
                    Console.WriteLine(string.Format("Waiting for {0} process to finish...", args[0]));
                    if (Process.GetProcessesByName(args[0]).Length != 0)
                    {
                        //Wait 1 sec and then kill it if it is still alive...
                        Thread.Sleep(1000);
                        if (Process.GetProcessesByName(args[0]).Length != 0)
                        {
                            Console.WriteLine("Wait time exceeded, killing process...");
                            foreach (var process in Process.GetProcessesByName(args[0] + ".exe"))
                            {
                                process.Kill();
                            }
                        }
                    }
                    Console.WriteLine("Deleting previous executable...");
                    File.Delete(args[1]);
                    Console.WriteLine("Renaming new executable...");
                    File.Move(args[2], args[1]);
                    Console.WriteLine("Done!\nPress any key to start " + args[0]);
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
    }
}

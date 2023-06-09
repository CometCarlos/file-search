﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileSearch {
    public class Program {
        public static async Task Main(string[] args) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("What directory would you like to search?");
            string path = Console.ReadLine();

            Console.WriteLine("What would you like to search for in the file names?");
            string searchTarget = Console.ReadLine();
            Stopwatch s = new Stopwatch();
            s.Start();

            FileSearcher searcher = new FileSearcher(path, searchTarget);
            searcher.IgnoreCase = true;
            searcher.SetSortBy((path1, path2) => {
                return 0;
            });
            List<string> pathResults = await searcher.Search();

            Console.WriteLine(pathResults.Count + " results found!");
            s.Stop();
            Console.WriteLine("That took " + s.ElapsedMilliseconds + "ms!");

            for (int i = 0; i < pathResults.Count; i++)
                Console.WriteLine(i + "    >>>    " + GetFileInfo(pathResults[i]) + "\n");

            Console.ReadKey(false);
        }

        private static string GetFileInfo(string path) {
            string info = path;
            info += "\nFile Name:" + string.Format("{0, 50}", Path.GetFileName(path));
            info += "\nCreated:  " + string.Format("{0, 50}", File.GetCreationTime(path));

            return info;
        }
    }
}

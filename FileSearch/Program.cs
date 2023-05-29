using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace FileSearch {
	public class Program {
		private const int FileCountPerPage = 10;

		public static void Main(string[] args) {
			//List<int> test = new List<int>();
			//test.Add(3);
			//test.Add(5);
			//test.Add(7);

			//test.Insert(3, 9);
			//for (int i = 0; i < test.Count; i++)
			//	Console.Write(test[i] + " ");

			//Console.ReadKey(false);
			//return;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("What directory would you like to search?");
			string path = Console.ReadLine();
			//Console.WriteLine("The directory that will be searched is:\n" + path);

			Console.WriteLine("What would you like to search for in the file names?");
			string searchTarget = Console.ReadLine();
			Stopwatch s = new Stopwatch();
			s.Start();

			FileSearcher searcher = new FileSearcher(path, searchTarget);
			searcher.IgnoreCase = true;
			searcher.MultiThreaded = true;
			searcher.SetSortBy((path1, path2) => {
				return 0;
			});
			List<string> pathResults = searcher.Search();

			Console.WriteLine(pathResults.Count + " results found!");
			s.Stop();
			Console.WriteLine("That took " + s.ElapsedMilliseconds + "ms!");

			for (int i = 0; i < pathResults.Count; i += FileCountPerPage) {
				for (int j = 0; j < FileCountPerPage; j++) {
					int index = i + j;
					Console.WriteLine("Result " + index + ":\n" + GetFileInfo(pathResults[i]) + "\n");
				}
			}
			//for (int i = 0; i < pathResults.Count; i++) {
			//	Console.WriteLine(pathResults[i]
			//		+ "\n" + File.GetLastAccessTime(pathResults[i])
			//		+ "\n");
			//}
			//Console.WriteLine("... ... ... and now " + pathResults.Count + " results!!");

			Console.ReadKey(false);
		}

		private static string GetFileInfo(string path) {
			string info = path;
			info += "\nFile Name:" + string.Format("{0, 30}", Path.GetFileName(path));
			info += "\nCreated: " + string.Format("{0, 30}", File.GetCreationTime(path));

			return info;
		}
	}
}

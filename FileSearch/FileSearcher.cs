using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace FileSearch {
	public class FileSearcher {
		private string rootPath;
		private string searchTarget;
		private bool ignoreCase;
		private bool multiThreaded;
		private Comparison<string> comparer;

		public string RootPath {
			get { return rootPath; }
			set {
				if (!Directory.Exists(value))
					throw new DirectoryNotFoundException("The directory" + rootPath + " does not exist.");
				rootPath = value;
			}
		}

		public string SearchTarget {
			get { return searchTarget; }
			set { searchTarget = value; }
		}

		public bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public bool MultiThreaded {
			get { return multiThreaded; }
			set { multiThreaded = value; }
		}

		public FileSearcher(string rootPath, string searchTarget) {
			RootPath = rootPath;
			SearchTarget = searchTarget;
		}

		public void SetSortBy(Comparison<string> comparer) {
			this.comparer = comparer;
		}

		public List<string> Search() {
			List<string> pathResults = new List<string>(16);
			string searchTarget = IgnoreCase ? SearchTarget.ToLower() : SearchTarget;
			
			if (MultiThreaded) {
				try {
					Task.Run(() => SearchMultiThreaded(RootPath, pathResults, searchTarget)).Wait();
					Console.WriteLine("FINISHED ----------------------------------------------------");
				} catch (AggregateException e) {
					Console.WriteLine("Something went wrong in the process of setting up the parallel tasks!\n\n" + e.InnerException.Message + "\n\n" + e.InnerException.StackTrace);
				}
			} else {
				Search(RootPath, pathResults, searchTarget);
			}
			if (comparer != null)
				pathResults.Sort(comparer);

			return pathResults;
		}

		private void Search(string currentPath, List<string> pathResults, string searchTarget) {
			try {
				string[] filePaths = Directory.GetFiles(currentPath);

				for (int i = 0; i < filePaths.Length; i++) {
					string currentFileName = Path.GetFileName(filePaths[i]);
					if (IgnoreCase)
						currentFileName = currentFileName.ToLower();

					if (currentFileName.Contains(searchTarget))
						pathResults.Add(filePaths[i]);
				}

				string[] directoryPaths = Directory.GetDirectories(currentPath);
				for (int i = 0; i < directoryPaths.Length; i++)
					Search(directoryPaths[i], pathResults, searchTarget);
			} catch (UnauthorizedAccessException e) {
				//Move on..
			}
		}

		private async Task SearchMultiThreaded(string currentPath, List<string> pathResults, string searchTarget) {
			Task[] tasks = null;
			//await Task.Delay(1000);
			try {
				string[] filePaths = Directory.GetFiles(currentPath);

				for (int i = 0; i < filePaths.Length; i++) {
					string currentFileName = Path.GetFileName(filePaths[i]);
					if (IgnoreCase)
						currentFileName = currentFileName.ToLower();

					if (currentFileName.Contains(searchTarget)) {
						lock (pathResults) {
							pathResults.Add(filePaths[i]);
						}
					}
				}

				string[] directoryPaths = Directory.GetDirectories(currentPath);
				tasks = new Task[directoryPaths.Length];
				for (int i = 0; i < directoryPaths.Length; i++) {
					int threadSafeIndex = i;
					tasks[i] = Task.Run(() => SearchMultiThreaded(directoryPaths[threadSafeIndex], pathResults, searchTarget));
				}
			} catch (UnauthorizedAccessException e) {
				//Move on..
			}
			if (tasks != null) {
				await Task.WhenAll(tasks);
			}
		}
	}
}

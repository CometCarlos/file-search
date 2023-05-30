using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSearch {
    public class FileSearcher {
        private static string SanitizePath(string path) => Regex.Replace(path.Replace('\\', '/'), "/{2,}", "/");

        private string rootPath;
        private string searchTarget;
        private bool ignoreCase;
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

        public FileSearcher(string rootPath, string searchTarget) {
            RootPath = rootPath;
            SearchTarget = searchTarget;
        }

        public void SetSortBy(Comparison<string> comparer) {
            this.comparer = comparer;
        }

        public async Task<List<string>> Search() {
            List<string> pathResults = new List<string>(16);
            string searchTarget = IgnoreCase ? SearchTarget.ToLower() : SearchTarget;

            try {
                await Task.Run(() => SearchInternal(RootPath, pathResults, searchTarget));
                Console.WriteLine("FINISHED ----------------------------------------------------");
            } catch (AggregateException e) {
                Console.WriteLine("Something went wrong in the process of setting up the parallel tasks!\n\n" + e.InnerException.Message + "\n\n" + e.InnerException.StackTrace);
            }
            if (comparer != null)
                pathResults.Sort(comparer);

            return pathResults;
        }

        private async Task SearchInternal(string currentPath, List<string> pathResults, string searchTarget) {
            currentPath = SanitizePath(currentPath);

            Task[] tasks = null;
            try {
                string[] filePaths = Directory.GetFiles(currentPath);

                for (int i = 0; i < filePaths.Length; i++) {
                    string currentFileName = Path.GetFileName(filePaths[i]);
                    if (IgnoreCase)
                        currentFileName = currentFileName.ToLower();

                    if (currentFileName.Contains(searchTarget)) {
                        lock (pathResults) {
                            //NOTE: We're not sanitizing every *file* path, only the ones we add into the list.
                            //      We can change this if this becomes a problem down the line..
                            pathResults.Add(SanitizePath(filePaths[i]));
                        }
                    }
                }

                string[] directoryPaths = Directory.GetDirectories(currentPath);
                tasks = new Task[directoryPaths.Length];
                for (int i = 0; i < directoryPaths.Length; i++) {
                    int threadSafeIndex = i;
                    tasks[i] = Task.Run(() => SearchInternal(directoryPaths[threadSafeIndex], pathResults, searchTarget));
                }
            } catch (UnauthorizedAccessException) {
                //Move on..
            }
            if (tasks != null) {
                await Task.WhenAll(tasks);
            }
        }
    }
}

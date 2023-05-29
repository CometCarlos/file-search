using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTestMain {
	public class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Main started");

			Task<int> task = TestAsync();
			task.Wait(1500);
			int result = task.Result;
			Console.WriteLine("In the Main now after calling TestAsync - result = " + result);
			Console.ReadKey(false);
		}

		public static async Task<int> TestAsync() {
			Console.WriteLine("TestAsync started");
			await Task.Delay(1900);
			Console.WriteLine("Hi from TestAsync 1900ms later");
			return 97;
		}
	}
}

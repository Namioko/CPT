using System;
using System.IO;

namespace Hello
{
	class Hello
	{
		static void Main(string[] args) 
		{
			var line1 = Console.ReadLine();
			var line2 = Console.ReadLine();
			var sr = new StreamReader("t.txt");
			var text = sr.ReadToEnd();
			Console.WriteLine(line1 + "1");
			Console.WriteLine(line2 + "2");
		}
	}
}
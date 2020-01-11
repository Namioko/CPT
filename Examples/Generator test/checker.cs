using System;
using System.IO;
using System.Collections.Generic;

namespace Hello
{
	class Hello
	{
		static void Main(string[] args)
		{
			var sr = new StreamReader(args[1]);
			var line = sr.ReadLine();
			sr.Close();
			var numbers = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			var result = int.Parse(Console.ReadLine());
			int max = Int32.MinValue;
			for(int i = 0; i < numbers.Length; i++)
			{
				var tempNumber = int.Parse(numbers[i]);
				if (tempNumber > max)
				{
					max = tempNumber;
				}
			}
			Console.WriteLine(int.Parse(numbers[result]) == max);
		}

	}
}
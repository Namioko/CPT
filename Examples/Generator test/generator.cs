using System;
using System.IO;
using System.Collections.Generic;

namespace Hello
{
	class Hello
	{
		static void Main(string[] args)
		{
			var sr = new StreamReader(args[0]);
			var line = sr.ReadLine();
			sr.Close();
			var numbers = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int max = Int32.MinValue;
			int maxIndex = -1;
			for(int i = 0; i < numbers.Length; i++)
			{
				var tempNumber = int.Parse(numbers[i]);
				if (tempNumber > max)
				{
					max = tempNumber;
					maxIndex = i;
				}
			}
			Console.WriteLine(maxIndex);
		}

	}
}
using System;
using System.IO;

namespace Hello
{
	class Hello
	{
		static void Main(string[] args)
		{
			var line = Console.ReadLine();
			var numbers = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			int max = Int32.MinValue;
			int indexOfMax = -1;
			for(int i = 0; i < numbers.Length; i++)
			{
				var tempNumber = int.Parse(numbers[i]);
				if (tempNumber > max)
				{
					max = tempNumber;
					indexOfMax = i;
				}
			}
			Console.WriteLine(indexOfMax);
		}

	}
}
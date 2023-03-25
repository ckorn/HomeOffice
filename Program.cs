using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
const string FileName = "homeoffice.csv";
Dictionary<DateTime, bool> homeOfficeDayDictionary = ReadFile();
//foreach(KeyValuePair<DateTime, bool> keyValuePair in homeOfficeDayDictionary)
//{
//	Console.WriteLine($"{keyValuePair.Key}={keyValuePair.Value}");
//}
DateTime today = DateTime.Today;
Console.WriteLine($"{today:d}");
if(!homeOfficeDayDictionary.ContainsKey(today))
{
	string answer=string.Empty;
	do
	{
		Console.WriteLine("HomeOffice? (J/N)");
		answer=Console.ReadLine() ?? string.Empty;
	}while((answer.ToUpper() != "J") && (answer.ToUpper() != "N"));
	bool homeOffice=(answer.ToUpper() == "J");
	AddEntry(today, homeOffice);
}

static Dictionary<DateTime, bool> ReadFile()
{
	Dictionary<DateTime, bool> homeOfficeDayDictionary = new ();
	if (File.Exists(FileName))
	{
		string[] lines = File.ReadAllLines(FileName);
		foreach(string line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
		{
			string[] parts = line.Split(';');
			DateTime dateTime = DateTime.Parse(parts[0]);
			bool homeOffice = (parts[1] == "1");
			homeOfficeDayDictionary[dateTime] = homeOffice;
		}
	}
	return homeOfficeDayDictionary;
}

static void AddEntry(DateTime dateTime, bool homeOffice)
{
	File.AppendAllText(FileName, $"{dateTime:d};{(homeOffice ? "1" : "0")}" + Environment.NewLine);
}

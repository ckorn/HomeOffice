using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
const string FileName = "homeoffice.csv";

Mutex runningMutex = new(true, "Global\\ckorn.HomeOffice", out bool runningMutexCreated);
Mutex? updateMutex = null;
bool updateMutexCreated = false;
const string UpdateMutexNamespace="Global\\ckorn.HomeOffice.update";
if(!runningMutexCreated)
{
	Console.WriteLine("Application already running");
	updateMutex = new(true, UpdateMutexNamespace, out updateMutexCreated);
	if(!updateMutexCreated)
	{
		throw new InvalidOperationException($"{nameof(updateMutexCreated)}=False");
	}
	Console.WriteLine("Waiting for running mutex...");
	runningMutex.WaitOne();
	Console.WriteLine("Got running mutex");
	updateMutex.ReleaseMutex();
	updateMutex.Dispose();
}
Dictionary<DateTime, bool> homeOfficeDayDictionary = ReadFile();
//foreach(KeyValuePair<DateTime, bool> keyValuePair in homeOfficeDayDictionary)
//{
//	Console.WriteLine($"{keyValuePair.Key}={keyValuePair.Value}");
//}
DateTime today = DateTime.Today;
Console.WriteLine($"{today:d}");
if(!homeOfficeDayDictionary.TryGetValue(today, out bool homeOffice))
{
	string answer=string.Empty;
	do
	{
		Console.WriteLine("HomeOffice? (J/N)");
		try
		{
			answer=Reader.ReadLine(5000) ?? string.Empty;
			answer=answer.ToUpper();
		}
		catch (TimeoutException)
		{
			updateMutex = new(false, UpdateMutexNamespace, out updateMutexCreated);
			updateMutex.Dispose();
			if(!updateMutexCreated)
			{
				Console.WriteLine("Release running mutex");
				runningMutex.ReleaseMutex();
				runningMutex.Dispose();
				return;
			}
		}
	}while((answer != "J") && (answer != "N"));
	homeOffice=(answer == "J");
	AddEntry(today, homeOffice);
}
Console.WriteLine($"HomeOffice={homeOffice}");
runningMutex.ReleaseMutex();
runningMutex.Dispose();

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

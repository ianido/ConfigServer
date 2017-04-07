﻿using System;
using System.Text.RegularExpressions;

namespace ConsoleApp3
{

    

    class Program
    {

        public static TimeSpan ParseHuman(string dateTime)
        {
            TimeSpan ts = TimeSpan.Zero;
            string currentString = ""; string currentNumber = "";
            foreach (char ch in dateTime)
            {
                if (ch == ' ') continue;                
                if (Regex.IsMatch(ch.ToString(), @"\d")) { currentNumber += ch; continue; }
                currentString += ch;
                if (Regex.IsMatch(currentString, @"^(days|day|d)", RegexOptions.IgnoreCase)) { ts = ts.Add(TimeSpan.FromDays(int.Parse(currentNumber))); currentString = ""; currentNumber = ""; continue; }
                if (Regex.IsMatch(currentString, @"^(hours|hour|h)", RegexOptions.IgnoreCase)) { ts = ts.Add(TimeSpan.FromHours(int.Parse(currentNumber))); currentString = ""; currentNumber = ""; continue; }
                if (Regex.IsMatch(currentString, @"^(ms)", RegexOptions.IgnoreCase)) { ts = ts.Add(TimeSpan.FromMilliseconds(int.Parse(currentNumber))); currentString = ""; currentNumber = ""; continue; }
                if (Regex.IsMatch(currentString, @"^(mins|min|m)", RegexOptions.IgnoreCase)) { ts = ts.Add(TimeSpan.FromMinutes(int.Parse(currentNumber))); currentString = ""; currentNumber = ""; continue; }
                if (Regex.IsMatch(currentString, @"^(secs|sec|s)", RegexOptions.IgnoreCase)) { ts = ts.Add(TimeSpan.FromSeconds(int.Parse(currentNumber))); currentString = ""; currentNumber = ""; continue; }
            }
            return ts;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("1h " + ParseHuman("1h").ToString());
            Console.WriteLine("1h2s " + ParseHuman("1h2s").ToString());
            Console.WriteLine("13h56m40s " + ParseHuman("13h56m40s").ToString());
            Console.WriteLine("12h 55m " + ParseHuman("12h 55m").ToString());
            Console.WriteLine("11m 20min " + ParseHuman("11m 20min").ToString());
            Console.WriteLine("11d 12h " + ParseHuman("11d 12h").ToString());
            Console.ReadLine();


        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace yupisoft.ConfigServer.Core.Utils
{
    public static class ConverterExtensions
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

        public static string Human(this TimeSpan value)
        {
            string duration = "";

            var totalDays = (int)value.TotalDays;
            if (totalDays >= 1)
            {
                duration = totalDays + " d";
                value = value.Add(TimeSpan.FromDays(-1 * totalDays));
            }

            var totalHours = (int)value.TotalHours;
            if (totalHours >= 1)
            {
                if (!string.IsNullOrEmpty(duration)) duration += " ";
                duration += totalHours + " h";
                value = value.Add(TimeSpan.FromHours(-1 * totalHours));
            }

            var totalMinutes = (int)value.TotalMinutes;
            if (totalMinutes >= 1)
            {
                if (!string.IsNullOrEmpty(duration)) duration += " ";
                duration += totalMinutes + "m";
                value = value.Add(TimeSpan.FromMinutes(-1 * totalMinutes));
            }

            var totalSeconds = (int)value.TotalSeconds;
            if (totalSeconds >= 1)
            {
                if (!string.IsNullOrEmpty(duration)) duration += " ";
                duration += totalSeconds + "s";
            }

            return duration;
        }
    }
}

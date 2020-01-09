#region License
// Copyright (c) 2019 Jake Fowler
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without 
// restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following 
// conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cmdty.TimePeriodValueTypes.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Creating();
                ParsingFormatting();
                Comparing();
                Offsetting();
                OffsetOperators();
                TimePeriodInterface();
                ConvertingGranularity();
                Expanding();
                TimePeriodRanges();
                ExtensionMethods();
                Console.ReadKey();
            }
            else
            {
                switch (args[1])
                {
                    case "creating":
                        Creating();
                        break;
                    case "parsing_formatting":
                        ParsingFormatting();
                        break;
                    case "comparing":
                        Comparing();
                        break;
                    case "offsetting":
                        Offsetting();
                        break;
                    case "offset_operators":
                        OffsetOperators();
                        break;
                    case "converting_granularity":
                        ConvertingGranularity();
                        break;
                    case "expanding":
                        Expanding();
                        break;
                    case "time_period_interface":
                        TimePeriodInterface();
                        break;
                    case "time_period_ranges":
                        TimePeriodRanges();
                        break;
                    case "extension_methods":
                        ExtensionMethods();
                        break;
                }
            }
        }

        private static void Creating()
        {
            #region creating
            var mar19 = new Month(2019, 3);
            Console.WriteLine(mar19);

            var aug19 = Month.CreateAugust(2019);
            Console.WriteLine(aug19);
            var qu119 = Quarter.CreateQuarter1(2019);
            Console.WriteLine(qu119);

            Day christmas2020 = Day.FromDateTime(new DateTime(2020, 12, 25));
            Console.WriteLine(christmas2020);
            Console.WriteLine();
            #endregion
        }

        private static void ParsingFormatting()
        {
            #region parsing_formatting
            Month dec19Original = Month.CreateDecember(2019);
            string dec19Text = dec19Original.ToString();

            Month dec19FromText = Month.Parse(dec19Text);
            Console.WriteLine("Dec-19 parsed from text: " + dec19FromText);

            var hour = new Hour(2019, 9, 11, 12);
            Console.WriteLine(hour.ToString("dd-MMM-yyyy hh:mm:ss", CultureInfo.InvariantCulture));
            Console.WriteLine();
            #endregion
        }

        private static void Comparing()
        {
            #region comparing
            
            var qu119 = new Quarter(2019, 1);
            var qu219 = Quarter.CreateQuarter2(2019);

            // IComparable<T>.CompareTo
            Console.WriteLine(qu119.CompareTo(qu219));
            Console.WriteLine();

            // IEquatable<T>.Equals
            Console.WriteLine(qu119.Equals(qu219));
            Console.WriteLine();

            // Comparison operators
            Console.WriteLine(qu119 < qu219);
            Console.WriteLine(qu119 <= qu219);
            Console.WriteLine(qu119 == qu219);
            Console.WriteLine(qu119 != qu219);
            Console.WriteLine(qu119 > qu219);
            Console.WriteLine(qu119 >= qu219);
            Console.WriteLine();
            #endregion
        }

        private static void Offsetting()
        {
            #region offsetting
            var tenAm = new Hour(2019, 8, 30, 10);
            Hour midday = tenAm.Offset(2);
            Console.WriteLine(midday);

            int numHours = midday.OffsetFrom(tenAm);
            Console.WriteLine(numHours);
            Console.WriteLine();
            #endregion offsetting
        }

        private static void OffsetOperators()
        {
            #region offset_operators
            var calYear19 = new CalendarYear(2019);

            CalendarYear calYear22 = calYear19 + 3;
            Console.WriteLine(calYear22);

            int yearsDifference = calYear22 - calYear19;
            Console.WriteLine(yearsDifference);

            // The increment ++, and decrement --, operators can also be used
            var halfHour = new HalfHour(2019, 8, 30, 22, 0);

            Console.WriteLine();
            Console.WriteLine("Incrementing Half Hour");
            Console.WriteLine(halfHour);
            halfHour++;
            Console.WriteLine(halfHour);
            halfHour++;
            Console.WriteLine(halfHour);

            Console.WriteLine();
            Console.WriteLine("Decrementing Half Hour");
            halfHour--;
            Console.WriteLine(halfHour);
            halfHour--;
            Console.WriteLine(halfHour);
            halfHour--;
            Console.WriteLine(halfHour);
            Console.WriteLine();
            #endregion offset_operators
        }

        private static void TimePeriodInterface()
        {
            #region time_period_interface

            void PrintFirstAndLastPeriods<T, T2>(T timePeriod)
                where T : ITimePeriod<T>
                where T2 : ITimePeriod<T2>
            {
                string periodType = typeof(T).Name;
                string period2Type = typeof(T2).Name;

                T2 firstHour = timePeriod.First<T2>();
                T2 lastHour = timePeriod.Last<T2>();
                Console.WriteLine($"The {periodType} period '{timePeriod}' has first {period2Type} '{firstHour.ToString()}' and last {period2Type} '{lastHour.ToString()}'");
            }

            var dec20 = new Month(2020, 12);
            PrintFirstAndLastPeriods<Month, Hour>(dec20);
            Console.WriteLine();
            #endregion
        }

        private static void ConvertingGranularity()
        {
            #region converting_granularity
            var qu119 = Quarter.CreateQuarter1(2019);
            Console.WriteLine("The first month in Q1-19 is " + qu119.First<Month>());
            Console.WriteLine("The last month in Q1-19 is " + qu119.Last<Month>());
            Console.WriteLine();
            #endregion converting_granularity
        }

        private static void Expanding()
        {
            #region expanding
            var qu219 = Quarter.CreateQuarter2(2019);
            IEnumerable<Month> allMonthsInQ119 = qu219.Expand<Month>();
            Console.WriteLine("All the months in Q2-19:");
            foreach (Month month in allMonthsInQ119)
            {
                Console.WriteLine(month);
            }
            Console.WriteLine();
            #endregion expanding
        }

        private static void TimePeriodRanges()
        {
            #region time_period_ranges
            Console.WriteLine("Minimum Day: " + Day.MinDay);
            Console.WriteLine("Maximum Day: " + Day.MaxDay);
            Console.WriteLine();
            #endregion time_period_ranges
        }

        private static void ExtensionMethods()
        {
            #region extension_methods
            var quarterStart = Quarter.CreateQuarter3(2020);
            var quarterEnd = Quarter.CreateQuarter2(2021);
            Console.WriteLine($"All the quarters from {quarterStart} to {quarterEnd}");
            foreach (Quarter quarter in quarterStart.EnumerateTo(quarterEnd))
            {
                Console.WriteLine(quarter);
            }

            Console.WriteLine();
            var dayStart = new Day(2019, 8, 30);
            var dayEnd = new Day(2019, 9, 4);
            Console.WriteLine($"All week days from {dayStart} to {dayEnd}");
            foreach (Day weekday in dayStart.EnumerateWeekdays(dayEnd))
            {
                Console.WriteLine(weekday);
            }
            Console.WriteLine();
            #endregion extension_methods
        }

    }
}

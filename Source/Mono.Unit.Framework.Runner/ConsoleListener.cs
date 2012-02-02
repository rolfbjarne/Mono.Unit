// ConsoleListener.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

#if !MOBILE
using System;
using System.Collections.Generic;
using System.Text;

using Mono.Unit.Framework;
using Mono.Unit.Framework.Runner;

using NUnit.Framework;
using NUnitLite;
using NUnitLite.Runner;

namespace Mono.Unit.Framework.Runner {
	public class ConsoleListener : TestListener, MonoTestListener {
		int indent;
		int errors;
		int failures;
		int successes;
		int notrun;
		
		public int Verbosity { get; set; }
		public bool Colors { get; set; }
		
		public void TestStarted (ITest test)
		{
			AsyncTestSuite ats = test as AsyncTestSuite;
			AsyncTestFixture atf;
			
			if (ats != null) {
				Console.WriteLine (ats.Name);
			} else if ((atf = test as AsyncTestFixture) != null) {
				Console.WriteLine ("{0}{1}", new string (' ', indent * 4), atf.Name);
			} else {
				Console.Write ("{0}{1} ", new string (' ', indent * 4), test.FullName);
			}
			indent++;
		}
		
		ConsoleColor GetColor (TestResult result)
		{
			switch (result.ResultState) {
			case ResultState.Error:
			case ResultState.Failure:
				return ConsoleColor.DarkRed;
			case ResultState.Success:
				return ConsoleColor.DarkGreen;
			case ResultState.NotRun:
				return ConsoleColor.Yellow;
			}
			
			return ConsoleColor.DarkBlue;
		}

		public void TestFinished (TestResult result)
		{
			AsyncTestSuite ats = result.Test as AsyncTestSuite;
			
			if (ats != null) {
				if (Colors)
					Console.ForegroundColor = GetColor (result);
				Console.WriteLine ("Test run {0}. {4} tests: {1} succeeded, {2} failed, {3} not run.",
					errors + failures > 0 ? "failed" : "passed", successes, failures + errors, notrun, successes + failures + errors + notrun);
				if (Colors)
					Console.ResetColor ();
			} else if (result.Test as AsyncTestFixture != null) {
				// nothing to print here
			} else {
				if (Colors)
					Console.ForegroundColor = GetColor (result);
				Console.WriteLine (result.ResultState.ToString ());
				if (Colors)
					Console.ResetColor ();
				switch (result.ResultState) {
				case ResultState.Error:
					if (Colors)
						Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine ("{0}{1}", new string (' ', indent), result.Message);
					if (!string.IsNullOrEmpty (result.StackTrace))
						Console.WriteLine (result.StackTrace);
					if (Colors)
						Console.ResetColor ();
					errors++;
					break;
				case ResultState.Failure:
					failures++;
					break;
				case ResultState.Success:
					successes++;
					break;
				case ResultState.NotRun:
					notrun++;
					break;
				}
			}
			indent--;
		}

		#region MonoTestListener implementation
		void MonoTestListener.AddOptions (Mono.Options.OptionSet options)
		{
			options.Add ("color|colors", "Use colors", v => Colors = v != null);
			options.Add ("v|verbose", "Increase verbosity", v => Verbosity += (v == null ? -1 : 1));

		}
		#endregion
	}
}

#endif


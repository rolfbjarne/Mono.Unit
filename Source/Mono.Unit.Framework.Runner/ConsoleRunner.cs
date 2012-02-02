// ConsoleRunner.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

#if !MOBILE

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using Mono.Options;
using Mono.Unit.Framework;

namespace Mono.Unit.Framework.Runner {
	public class ConsoleRunner {
		public static void ShowHelp (OptionSet options)
		{
			string exe = Assembly.GetEntryAssembly ().GetName ().Name;

			Console.WriteLine ("{0}", exe);
			Console.WriteLine ("Copyright 2012, Xamarin Inc.");
			Console.WriteLine ("Usage is: {0} [options]", exe);
		
			options.WriteOptionDescriptions (Console.Out);
		}
		
		public static int Run (string [] args, Assembly assembly)
		{
			bool help = false;
			var fixtures = new List<string> ();
			List<string> extra_args;
			MonoTestListener listener;
			
			listener = new ConsoleListener ();
			
			var options = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => help = true },
				{ "f|fixture=", "Select a fixture to run. It is also possible to select an entire namespace to run. This option can be specified multiple times.", v => fixtures.Add (v) },
			};
			listener.AddOptions (options);
			
			try {
				extra_args = options.Parse (args);
				if (extra_args != null && extra_args.Count > 0) {
					Console.Error.WriteLine ("{0}: Invalid options passed: {1}", Assembly.GetExecutingAssembly ().GetName ().Name, string.Join (" ", extra_args.ToArray ()));
					Console.Error.WriteLine ("See '{0} --help' for more information", Assembly.GetExecutingAssembly ().GetName ().Name);
					return 1;
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("{0}: {1}", Assembly.GetExecutingAssembly ().GetName ().Name, e.Message);
				Console.Error.WriteLine ("See '{0} --help' for more information", Assembly.GetExecutingAssembly ().GetName ().Name);
				return 1;
			}
			
			if (help) {
				ShowHelp (options);
				return 0;
			}
			
			var runner = new AsyncTestRunner (listener);
			return runner.Execute (assembly, fixtures.ToArray ());
		}
	}
}

#endif

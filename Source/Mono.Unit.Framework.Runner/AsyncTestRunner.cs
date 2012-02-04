// AsyncTestRunner.cs:
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
using System.Threading;
using System.Reflection;

using NUnitLite;
using NUnitLite.Runner;

namespace Mono.Unit.Framework.Runner {
	public class AsyncTestRunner : TestRunner {
		bool quit;
		List<InvokeData> callbacks = new List<InvokeData> ();
		AutoResetEvent callback_event = new AutoResetEvent (false);
		TestListener listener;
		
		public AsyncTestRunner (TestListener listener)
		{
			this.listener = listener;
		}
		
		public int Execute (Assembly assembly, params string[] tests)
		{
			TestResult result;
			result = Run (assembly, tests);
			return result.IsSuccess ? 0 : 1;
		}
		
		public override TestResult Run (Assembly assembly)
		{
			return Run (AsyncTestLoader.Load (assembly));
		}
		
		public override TestResult Run (Assembly assembly, string[] tests)
		{
			if (tests == null || tests.Length == 0)
				return Run (assembly);
			
			throw new NotImplementedException ();
		}
		
		public override TestResult Run (ITest test)
		{
			return Run ((AsyncTestSuite) test);
		}
		
		TestResult Run (AsyncTestSuite suite)
		{
			suite.BeginInvokeHandler = BeginInvoke;
			suite.CompletedEvent += (sender, e) => 
			{
				quit = true;
				callback_event.Set ();
			};
			suite.RunAsync (listener);
			MainLoop ();
			return suite.Result;
		}
		
		void BeginInvoke (InvokeHandler action, TimeSpan interval)
		{
			DateTime dt = DateTime.UtcNow.Add (interval);
			lock (callbacks) {
				bool inserted = false;
				var data = new InvokeData () { Action = action, Time = dt };
				for (int i = 0; i < callbacks.Count; i++) {
					if (dt > callbacks [i].Time) {
						callbacks.Insert (i, data);
						inserted = true;
						break;
					}
				}
				if (!inserted)
					callbacks.Add (data);
				callback_event.Set ();
			}
		}
		
		void MainLoop ()
		{
			InvokeData data;
			TimeSpan timeout = TimeSpan.Zero;
			bool more;
			
			while (!quit) {
				if (timeout < TimeSpan.Zero)
					timeout = TimeSpan.Zero;
				callback_event.WaitOne (timeout);
				
				if (quit)
					break;
				
				more = false;
				do {
					lock (callbacks) {
						if (callbacks.Count == 0)
							continue;
						data = callbacks [callbacks.Count - 1];
						timeout = data.Time - DateTime.UtcNow;
						if (timeout.Ticks <= 0) {
							callbacks.RemoveAt (callbacks.Count - 1);
							more = callbacks.Count > 0;
						} else {
							break;
						} 
					}
					
					if (data != null) {
						data.Action ();
					} else {
						break;
					}
				} while (more);
			}
		}
		
		class InvokeData {
			public InvokeHandler Action;
			public DateTime Time;
		}
	}
}
#endif

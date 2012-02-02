// AsyncTestSuite.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

using System;
using System.Collections.Generic;

using NUnitLite;
using Mono.Unit.Framework.Runner;

namespace Mono.Unit {
	public class AsyncTestSuite : TestSuite {
		Queue<AsyncTestFixture> queue;
		AsyncTestFixture current;
		TestListener listener;
		
		public int Failures { get; private set; }
		public int Errors { get; private set; }

		public AsyncTestSuite (string name)
			: base (name)
		{
		}
		
		internal void BeginInvoke (InvokeHandler action, TimeSpan timeout)
		{
#if MONOTOUCH
			MonoTouch.Foundation.NSTimer timer = null;
			timer = MonoTouch.Foundation.NSTimer.CreateScheduledTimer (timeout, () =>
			{
				try {
					action ();
				} finally {
					timer.Dispose ();
				}
			});
#else
			BeginInvokeHandler (action, timeout);
#endif
		}

		public BeginInvokeHandler BeginInvokeHandler;

		internal void BeginInvoke (InvokeHandler action)
		{
			BeginInvoke (action, TimeSpan.FromMilliseconds (1));
		}

		public override void RunAsync (TestListener listener)
		{
			this.listener = listener;

			queue = new Queue<AsyncTestFixture> (Tests.Count);
			Result = new TestResult (this);

			OnStarted (null);
			listener.TestStarted (this);

			foreach (AsyncTestFixture fixture in Tests)
				queue.Enqueue (fixture);

			RunIterate ();
		}
		
		void RunIterate ()
		{
			if (current != null) {
				if (!current.RunIterate ()) {
					BeginInvoke (RunIterate);
					return;
				}
				Result.AddResult (current.Result);
			}

			while (queue.Count > 0) {
				current = queue.Dequeue ();
				current.RunAsync (listener);
				if (!current.RunIterate ()) {
					BeginInvoke (RunIterate);
					return;
				}
				Result.AddResult (current.Result);
			}

			RunEnd ();
		}

		void RunEnd ()
		{
			if (TestCaseCount == 0)
				Result.NotRun ("Suite has no tests");
			else if (Errors > 0 || Failures > 0)
				Result.Failure ("One or more component tests failed");
			else
				Result.Success ();
			
			listener.TestFinished (Result);
			OnCompleted (null);
			current = null;
		}
	}
}

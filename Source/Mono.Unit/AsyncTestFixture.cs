// AsyncTestFixture.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

using System;
using System.Collections.Generic;
using System.Reflection;

using NUnitLite;
using Mono.Unit.Framework.Runner;

namespace Mono.Unit {
	public class AsyncTestFixture : TestSuite {
		Queue<AsyncTestCase> queue;
		AsyncTestCase current;
		TestListener listener;
		
		public AsyncTestSuite Suite { get; set; }
		public int Failures { get; private set; }
		public int Errors { get; private set; }
		
		public AsyncTestFixture (AsyncTestSuite suite, Type type)
			: base (type)
		{
			this.Suite = suite;
			
			foreach (AsyncTestCase c in Tests)
				c.Suite = Suite;
		}
		
		protected override TestCase CreateTestCase (MethodInfo method)
		{
			return new AsyncTestCase (method);
		}
		
		public override void RunAsync (TestListener listener)
		{
			this.listener = listener;
			
			OnStarted (null);
			listener.TestStarted (this);
			Result = new TestResult (this);

			queue = new Queue<AsyncTestCase> (Tests.Count);

			switch (RunState) {
			case RunState.NotRunnable:
				Result.Error (IgnoreReason);
				break;

			case RunState.Ignored:
				Result.NotRun (IgnoreReason);
				break;

			case RunState.Runnable:
				foreach (AsyncTestCase @case in Tests)
					queue.Enqueue (@case);
				break;
			}
		}

		internal bool RunIterate ()
		{
			if (current != null) {
				if (!current.RunIterate ())
					return false;
				CurrentCompleted ();
			}

			while (queue.Count > 0) {
				current = queue.Dequeue ();
				current.RunAsync (listener);
				if (!current.RunIterate ())
					return false;
				CurrentCompleted ();
			}

			RunEnd ();
			return true;
		}
		
		void CurrentCompleted ()
		{
			Result.AddResult (current.Result);
			Errors += current.Errors;
			Failures += current.Failures;
		}

		void RunEnd ()
		{
			if (TestCaseCount == 0)
				Result.NotRun ("Class has no tests");
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

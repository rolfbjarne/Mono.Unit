// AsyncTestCase.cs:
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
using NUnit.Framework;

using Mono.Unit.Framework;
using Mono.Unit.Framework.Runner;

namespace Mono.Unit {
	public class AsyncTestCase : TestCase {
		TestListener listener;
		bool is_async;
		Queue<ConditionalHandler> tasks;
		
		public static AsyncTestCase Current;
		
		public AsyncTestSuite Suite { get; set; }
		public int Failures { get; private set; }
		public int Errors { get; private set; }
		
		public AsyncTestCase (MethodInfo method)
			: base (method)
		{
		}
		
		public override void RunAsync (TestListener listener)
		{
			IgnoreAttribute ignore = (IgnoreAttribute) Reflect.GetAttribute (Method, typeof (IgnoreAttribute));
			
			this.listener = listener;
			tasks = new Queue<ConditionalHandler> ();
			Result = new TestResult (this);
			
			OnStarted (null);
			listener.TestStarted (this);

			is_async = Method.IsDefined (typeof (AsynchronousAttribute), false);
			Current = this;

			if (RunState == RunState.NotRunnable) {
				Result.Failure (IgnoreReason);
			} else if (ignore != null) {
				Result.NotRun (ignore.Reason);
			} else {
				SetUp ();
				try {
					RunTest ();

					if (is_async) {
						Suite.BeginInvoke (ProcessTasks);
					} else {
						Result.Success ();
						RunEnd ();
					}
				} catch (NUnitLiteException nex) {
					Result.RecordException (nex.InnerException);
					RunEnd ();
				} catch (Exception ex) {
					Result.RecordException (ex);
					RunEnd ();
				}
			}
		}

		internal bool RunIterate ()
		{
			if (!is_async)
				return true;

			if (Current != this)
				return true;

			return false;
		}

		void RunEnd ()
		{
			switch (Result.ResultState) {
			case ResultState.Error:
				++Errors;
				break;
			case ResultState.Failure:
				++Failures;
				break;
			case ResultState.NotRun:
				Result.Success ();
				break;
			default:
				break;
			}
			listener.TestFinished (Result);
			OnCompleted (null);

			// clear everything for the next test
			tasks.Clear ();
			Current = null;
			
		}
		
		/// <summary>
		/// Add a task to execute. The task must return true if has completed (and if it
		/// returns false it will be invoked again after a little while, until it
		/// eventually returns true)
		/// </summary>
		/// <param name="action"></param>
		public void AddTask (ConditionalHandler action)
		{
			if (!is_async)
				throw new Exception ("Cannot add async tasks to a sync test.");

			tasks.Enqueue (action);
		}
		
		void ProcessTasks ()
		{
			if (tasks.Count == 0)
				return;

			ConditionalHandler task = tasks.Peek ();
			try {
				if (task ()) {
					if (tasks.Contains (task))
						tasks.Dequeue ();
				}

				if (tasks.Count > 0)
					Suite.BeginInvoke (ProcessTasks);
			} catch (Exception ex) {
				SetTestException (ex);
				RunEnd ();
			}
		}
		
		internal void SetTestException (Exception ex)
		{
			Result.RecordException (ex);
		}

		internal void SetTestCompleted ()
		{
			RunEnd ();
		}
	}
}

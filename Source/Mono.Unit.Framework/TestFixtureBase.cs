// TestFixtureBase.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using Mono.Unit;
using Mono.Unit.Framework;
using Mono.Unit.Framework.Runner;

namespace Mono.Unit.Framework {
	[TestFixture ()]
	public class TestFixtureBase {
		public void Enqueue (InvokeHandler action)
		{
			AsyncTestCase.Current.AddTask (() => { action (); return true; });
		}

		public void EnqueueConditional (ConditionalHandler condition)
		{
			AsyncTestCase.Current.AddTask (condition);
		}

		public void EnqueueTestComplete ()
		{
			AsyncTestCase.Current.AddTask (() => { AsyncTestCase.Current.SetTestCompleted (); return true; } );
		}
	}
}


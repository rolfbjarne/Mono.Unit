// AsyncTestLoader.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

using System;
using System.Reflection;

using NUnitLite;
using NUnitLite.Runner;
using NUnit.Framework;

namespace Mono.Unit.Framework.Runner
{
	public static class AsyncTestLoader
	{
		public static TestSuite Load (Assembly assembly)
		{
			AsyncTestSuite suite = new AsyncTestSuite (new AssemblyName (assembly.FullName).Name);
			
			foreach (Type type in assembly.GetTypes ()) {
				if (IsTestFixture (type))
					suite.AddTest (new AsyncTestFixture (suite, type));
			}
			
			return suite;
		}

		private static bool IsTestFixture (Type type)
		{
			return Reflect.HasAttribute (type, typeof (TestFixtureAttribute));
		}

		public static TestSuite LoadAsSuite (Type type)
		{
			PropertyInfo suiteProperty = Reflect.GetSuiteProperty (type);
			if (suiteProperty != null)
				return (TestSuite) suiteProperty.GetValue (null, Reflect.EmptyTypes);
			
			return null;
		}
	}
}

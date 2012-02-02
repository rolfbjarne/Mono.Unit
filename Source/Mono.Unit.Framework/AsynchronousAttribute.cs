// AsynchronousAttribute.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

using System;
using System.Collections.Generic;

namespace Mono.Unit.Framework {
	[AttributeUsage (AttributeTargets.Method)]
	public class AsynchronousAttribute : Attribute
	{
	}
}


// MonoTestListener.cs:
//
// Authors:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved
//

#if !MOBILE
using System;

using NUnitLite;

using Mono.Options;
using Mono.Unit;
using Mono.Unit.Framework;
using Mono.Unit.Framework.Runner;

namespace Mono.Unit.Framework.Runner {
	public interface MonoTestListener : TestListener {
		void AddOptions (OptionSet options);
	}
}

#endif


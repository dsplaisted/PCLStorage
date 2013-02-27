using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCLStorage.TestFramework
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public class TestMethodAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class TestClassAttribute : Attribute
	{

	}
}

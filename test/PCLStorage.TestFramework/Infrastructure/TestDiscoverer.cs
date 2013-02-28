using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PCLStorage.TestFramework.Infrastructure
{
	public class TestDiscoverer
	{

		public IEnumerable<Test> DiscoverTests(Assembly assembly)
		{
			List<Test> ret = new List<Test>();

			foreach (Type type in assembly.GetExportedTypes())
			{
				ret.AddRange(DiscoverTests(type));
			}

			return ret;
		}

		public IEnumerable<Test> DiscoverTests(Type type)
		{
			List<Test> ret = new List<Test>();
			foreach (MethodInfo method in type.GetMethods())
			{
				if (!method.IsStatic)
				{
					continue;
				}

				if (method.GetCustomAttributes(typeof(TestMethodAttribute), false).Any())
				{
					var test = new Test(method);
					ret.Add(test);
				}

			}
			return ret;
		}

	}
}

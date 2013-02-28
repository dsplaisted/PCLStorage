using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage.TestFramework.Infrastructure
{
	public class TestRunner
	{
		List<Test> _tests;
		StringBuilder _log;

		public TestRunner(IEnumerable<Test> tests)
		{
			_tests = tests.ToList();
			_log = new StringBuilder();
		}

		public TestRunner(params Assembly[] assemblies)
			: this(assemblies.SelectMany(a => new TestDiscoverer().DiscoverTests(a)))
		{
		}

		public int TestCount { get { return _tests.Count; } }
		public int PassCount { get; private set; }
		public int FailCount { get; private set; }
		public int SkipCount { get; private set; }

		public string Log { get { return _log.ToString(); } }

		public async Task RunTestsAsync()
		{
			PassCount = 0;
			FailCount = 0;
			SkipCount = 0;

			_log.Length = 0;

			foreach (var test in _tests)
			{
				await test.RunAsync();
				if (test.TestState == TestState.Passed)
				{
					PassCount++;
				}
				else if (test.TestState == TestState.Skipped)
				{
					SkipCount++;
					LogLine("Skipped: " + test.FullName);
				}
				else if (test.TestState == TestState.Failed)
				{
					FailCount++;
					LogLine("Failed: " + test.FullName);
					LogLine(test.FailureException.ToString());
					LogLine("");
				}
				else
				{
					throw new InvalidOperationException("Unexpected test state: " + test.TestState);
				}
			}

			LogLine("");
			LogLine(PassCount.ToString() + " passed, " + FailCount + " failed, " + SkipCount + " skipped, " + TestCount + " total");
		}

		void LogLine(string s)
		{
			Debug.WriteLine(s);
			_log.AppendLine(s);
		}

		
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage.TestFramework.Infrastructure
{
	public enum TestState
	{
		NotRun,
		Passed,
		Failed,
		Skipped,
	}

	public class Test
	{
		MethodInfo _method;

		public Test(MethodInfo method)
		{
			_method = method;
			TestState = TestState.NotRun;
			FailureException = null;
		}

		public string FullName
		{
			get
			{
				return _method.DeclaringType.FullName + "." + _method.Name;
			}
		}

		public TestState TestState { get; private set; }

		public Exception FailureException { get; private set; }

		public async Task RunAsync()
		{
			TestState = TestState.NotRun;
			FailureException = null;
			try
			{
				//	Use compiled lambdas so that exceptions won't be wrapped in a TargetInvocationException
                if (_method.IsStatic)
                {
                    if (_method.ReturnType == typeof(void))
                    {
                        var callExpression = Expression.Call(_method);
                        var lambda = Expression.Lambda<Action>(callExpression);
                        lambda.Compile()();
                    }
                    else
                    {
                        var callExpression = Expression.Call(_method);
                        var lambda = Expression.Lambda<Func<Task>>(callExpression);
                        await lambda.Compile()();
                    }
                }
                else
                {
                    object testClassInstance = Activator.CreateInstance(_method.ReflectedType);
                    if (_method.ReturnType == typeof(void))
                    {
                        var callExpression = Expression.Call(Expression.Constant(testClassInstance, _method.ReflectedType), _method);
                        var lambda = Expression.Lambda<Action>(callExpression);
                        lambda.Compile()();
                    }
                    else
                    {
                        var callExpression = Expression.Call(Expression.Constant(testClassInstance, _method.ReflectedType), _method);
                        var lambda = Expression.Lambda<Func<Task>>(callExpression);
                        await lambda.Compile()();
                    }
                }
				TestState = TestState.Passed;
			}
			catch (Exception ex)
			{
				TestState = TestState.Failed;
				FailureException = ex;
			}			
		}
	}
}

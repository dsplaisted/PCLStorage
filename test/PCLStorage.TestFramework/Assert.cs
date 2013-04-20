using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage.TestFramework
{
	public class AssertFailedException : Exception
	{
		public AssertFailedException(string message)
			: base(message)
		{

		}

		public AssertFailedException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}
#if !NETFX_CORE && !DESKTOP
	public static class Assert
	{
		public static void AreEqual(object expected, object actual, string message = null)
		{
			bool equal;
			if (expected == null)
			{
				equal = (actual == null);
			}
			else
			{
				equal = expected.Equals(actual);
			}

			if (!equal)
			{
				string failMessage = string.Format("Expected: {0} Actual: {1}", expected, actual);
				HandleFail("AreEqual", failMessage, message);
			}
		}

		public static void IsTrue(bool condition, string message = null)
		{
			if (!condition)
			{
				HandleFail("IsTrue", null, message);
			}
		}

		public static void IsFalse(bool condition, string message = null)
		{
			if (condition)
			{
				HandleFail("IsFalse", null, message);
			}
		}

        public static void IsNull(object obj, string message = null)
        {
            if (!object.ReferenceEquals(obj, null))
            {
                HandleFail("IsNull", null, message);
            }
        }

        public static void IsNotNull(object obj, string message = null)
        {
            if (object.ReferenceEquals(obj, null))
            {
                HandleFail("IsNotNull", null, message);
            }
        }

		

        static void HandleFail(string assertName, string failMessage, string message, Exception innerException = null)
        {
			string finalMessage = "Assert." + assertName + " failed.";
			if (!string.IsNullOrEmpty(failMessage))
			{
				finalMessage += "  " + failMessage;
			}
            if (!string.IsNullOrEmpty(message))
            {
                finalMessage += "  " + message;
            }

            if (innerException == null)
            {
                throw new AssertFailedException(finalMessage);
            }
            else
            {
                throw new AssertFailedException(finalMessage, innerException);
            }
        }
    }
#endif

    public static class ExceptionAssert
    {
		public static T Throws<T>(Action action, string message = null) where T : Exception
		{
			string failMessage;
			try
			{
				action();
			}
			catch (T ex)
			{
				return ex;
			}
			catch (Exception ex)
			{
				failMessage = string.Format("Expected exception of type {0}, but caught exception of type {1}", typeof(T).FullName, ex.GetType().FullName);
				HandleFail("Throws", failMessage, message, ex);
				return null;
			}

			failMessage = string.Format("Expected exception of type {0}, but no exception was caught", typeof(T).FullName);
			HandleFail("Throws", failMessage, message);

			return null;
		}

		public static async Task<T> ThrowsAsync<T>(Func<Task> action, string message = null) where T : Exception
		{
			string failMessage;
			try
			{
				await action();
			}
			catch (T ex)
			{
				return ex;
			}
			catch (Exception ex)
			{
				failMessage = string.Format("Expected exception of type {0}, but caught exception of type {1}", typeof(T).FullName, ex.GetType().FullName);
				HandleFail("ThrowsAsync", failMessage, message, ex);
				return null;
			}

			failMessage = string.Format("Expected exception of type {0}, but no exception was caught", typeof(T).FullName);
			HandleFail("ThrowsAsync", failMessage, message);

			return null;
		}

        static void HandleFail(string assertName, string failMessage, string message, Exception innerException = null)
        {
            string finalMessage = "ExceptionAssert." + assertName + " failed.  " + failMessage;
            if (!string.IsNullOrEmpty(message))
            {
                message += "  " + message;
            }

            if (innerException == null)
            {
                throw new AssertFailedException(finalMessage);
            }
            else
            {
                throw new AssertFailedException(finalMessage, innerException);
            }
        }
	}
}

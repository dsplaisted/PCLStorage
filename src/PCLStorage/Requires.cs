//-----------------------------------------------------------------------
// <copyright file="Requires.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
// This file is a derivation of:
// https://github.com/AArnott/Validation/blob/master/src/Validation/Requires.cs
// Which is released under the MS-PL license.
//-----------------------------------------------------------------------

namespace PCLStorage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Common runtime checks that throw ArgumentExceptions upon failure.
    /// </summary>
    internal static class Requires
    {
        private const string Argument_EmptyString = "'{0}' cannot be an empty string (\"\") or start with the null character.";

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough]
        public static T NotNull<T>(T value, string parameterName)
            where T : class // ensures value-types aren't passed to a null checking method
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null or empty.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>null</c> or empty.</exception>
        [DebuggerStepThrough]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            // To the guy that is doing random code cleaning: 
            // Consider the perfomance when changing the code to delegate to NotNull.
            // In general do not chain call to another function, check first and return as earlier as possible. 
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Length == 0 || value[0] == '\0')
            {
                throw new ArgumentException(Format(Argument_EmptyString, parameterName), parameterName);
            }
        }

        /// <summary>
        /// Helper method that formats string arguments.
        /// </summary>
        /// <returns>The formatted string.</returns>
        private static string Format(string format, params object[] arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, format, arguments);
        }
    }
}


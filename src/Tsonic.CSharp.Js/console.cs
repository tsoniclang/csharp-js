/**
 * JavaScript console object implementation
 */

using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Console logging functions (lowercase class name to match JavaScript)
    /// </summary>
    public static class console
    {
        /// <summary>
        /// Log message to console
        /// </summary>
        public static void log(params object[] data)
        {
            Console.WriteLine(string.Join(" ", data));
        }

        /// <summary>
        /// Log error message to stderr
        /// </summary>
        public static void error(params object[] data)
        {
            Console.Error.WriteLine(string.Join(" ", data));
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void warn(params object[] data)
        {
            Console.WriteLine("WARN: " + string.Join(" ", data));
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public static void info(params object[] data)
        {
            Console.WriteLine(string.Join(" ", data));
        }

        /// <summary>
        /// Log debug message
        /// </summary>
        public static void debug(params object[] data)
        {
            Console.WriteLine("DEBUG: " + string.Join(" ", data));
        }

        /// <summary>
        /// Log stack trace
        /// </summary>
        public static void trace(params object[] data)
        {
            Console.WriteLine(string.Join(" ", data));
            Console.WriteLine(new System.Diagnostics.StackTrace(true).ToString());
        }

        /// <summary>
        /// Assert condition and log if false
        /// </summary>
        public static void assert(bool condition = false, params object[] data)
        {
            if (!condition)
            {
                if (data.Length == 0)
                {
                    error("Assertion failed");
                    return;
                }

                var message = new object[data.Length + 1];
                message[0] = "Assertion failed:";
                System.Array.Copy(data, 0, message, 1, data.Length);
                error(message);
            }
        }

        /// <summary>
        /// Display data in tabular format
        /// </summary>
        public static void table(object? tabularData = null, object? properties = null)
        {
            log(properties == null ? new object[] { tabularData! } : new object[] { tabularData!, properties });
        }

        // Timing
        private static readonly Dictionary<string, long> _timers = new Dictionary<string, long>();

        /// <summary>
        /// Start a timer
        /// </summary>
        public static void time(string label = "default")
        {
            _timers[label] = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// End a timer and log elapsed time
        /// </summary>
        public static void timeEnd(string label = "default")
        {
            if (_timers.TryGetValue(label, out long startTime))
            {
                long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - startTime;
                double ms = elapsed * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                log($"{label}: {ms:F3}ms");
                _timers.Remove(label);
            }
        }

        /// <summary>
        /// Log elapsed time without ending timer
        /// </summary>
        public static void timeLog(string label = "default", params object[] data)
        {
            if (_timers.TryGetValue(label, out long startTime))
            {
                long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - startTime;
                double ms = elapsed * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                log($"{label}: {ms:F3}ms", data);
            }
        }

        /// <summary>
        /// Record a timestamp marker
        /// </summary>
        public static void timeStamp(string label = "default")
        {
            log(label);
        }

        // Counting
        private static readonly Dictionary<string, int> _counters = new Dictionary<string, int>();

        /// <summary>
        /// Increment and log counter
        /// </summary>
        public static void count(string label = "default")
        {
            if (!_counters.ContainsKey(label))
            {
                _counters[label] = 0;
            }
            _counters[label]++;
            log($"{label}: {_counters[label]}");
        }

        /// <summary>
        /// Reset counter
        /// </summary>
        public static void countReset(string label = "default")
        {
            _counters[label] = 0;
        }

        public static void profile(string label = "default")
        {
            _ = label;
        }

        public static void profileEnd(string label = "default")
        {
            _ = label;
        }

        // Grouping
        private static int _groupIndent = 0;

        /// <summary>
        /// Start a log group
        /// </summary>
        public static void group(params object[] data)
        {
            log(data);
            _groupIndent++;
        }

        /// <summary>
        /// Start a collapsed log group
        /// </summary>
        public static void groupCollapsed(params object[] data)
        {
            log(data);
            _groupIndent++;
        }

        /// <summary>
        /// End a log group
        /// </summary>
        public static void groupEnd()
        {
            if (_groupIndent > 0)
            {
                _groupIndent--;
            }
        }

        /// <summary>
        /// Clear console
        /// </summary>
        public static void clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// Display object properties
        /// </summary>
        public static void dir(object? item = null, object? options = null)
        {
            log(options == null ? new object[] { item! } : new object[] { item!, options });
        }

        /// <summary>
        /// Display XML/HTML element
        /// </summary>
        public static void dirxml(params object[] data)
        {
            log(data);
        }
    }
}

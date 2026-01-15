using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Core;
using PolicyDrivenSingleton.Tests.Editor.Doubles;

namespace PolicyDrivenSingleton.Tests.Editor.Logging
{
    [TestFixture]
    public class SingletonLoggerEditModeTests
    {
        private static Regex BuildTypedLogRegex<T>(string message)
        {
            string typeName = typeof(T).FullName ?? typeof(T).Name;
            string pattern = @"^\[" + Regex.Escape(str: typeName) + @"\]\s*" + Regex.Escape(str: message) + @"\s*$";
            return new Regex(pattern: pattern, options: RegexOptions.Singleline);
        }

        [Test]
        public void Log_WithTypeParameter_FormatsCorrectly()
        {
            // Expect the log to avoid test failure
            LogAssert.Expect(type: LogType.Log, message: BuildTypedLogRegex<TestPersistentSingletonForEditMode>(message: "Test info message"));

            SingletonLogger.Log<TestPersistentSingletonForEditMode>(message: "Test info message");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LogWarning_WithTypeParameter_FormatsCorrectly()
        {
            // Expect the warning log to avoid test failure
            LogAssert.Expect(type: LogType.Warning, message: BuildTypedLogRegex<TestPersistentSingletonForEditMode>(message: "Test warning message"));

            SingletonLogger.LogWarning<TestPersistentSingletonForEditMode>(message: "Test warning message");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LogError_WithTypeParameter_FormatsCorrectly()
        {
            // Expect the error log to avoid test failure
            LogAssert.Expect(type: LogType.Error, message: BuildTypedLogRegex<TestPersistentSingletonForEditMode>(message: "Test error message"));

            SingletonLogger.LogError<TestPersistentSingletonForEditMode>(message: "Test error message");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ThrowInvalidOperation_ThrowsWithCorrectMessage()
        {
            var ex = Assert.Throws<System.InvalidOperationException>(code: () =>
            {
                SingletonLogger.ThrowInvalidOperation<TestPersistentSingletonForEditMode>(message: "Test exception");
            });

            Assert.IsTrue(condition: ex.Message.Contains(value: "TestPersistentSingletonForEditMode"), message: "Exception should contain type name");
            Assert.IsTrue(condition: ex.Message.Contains(value: "Test exception"), message: "Exception should contain custom message");
        }
    }
}

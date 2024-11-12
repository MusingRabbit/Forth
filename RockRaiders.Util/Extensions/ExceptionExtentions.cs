using System;
using System.Diagnostics;

namespace RockRaiders.Util.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// This method provides the name of the method that the try/catch is written 
        /// NOT where the exception occurred.
        /// This is for populating the logs with a generated title.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>The name of the method.</returns>
        public static string GetThisMethodName(this Exception exception)
        {
            var stackTrace = new StackTrace(exception);
            if (stackTrace.FrameCount > 1)
            {
                return stackTrace.GetFrame(stackTrace.FrameCount - 1).GetMethod().Name;
            }
            else
            {
                return exception.TargetSite.Name;
            }
        }
    }
}

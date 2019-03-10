using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLeh.Console
{
    public static class ExceptionExtensions
    {
        public static Exception GetInnermostException<T>(this T e)
            where T : Exception
        {
            Exception innermost = e;
            while (e.InnerException != null)
                innermost = e.InnerException;
            return innermost;
        }
    }
}

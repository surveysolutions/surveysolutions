using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public interface IRequesProcessor
    {
        T Process<T>(string url, T defaultValue) where T : struct;
        T Process<T>(string url, string method, T defaultValue) where T : struct;
        T Process<T>(string url, string method, bool includeCookies, T defaultValue) where T : struct;
    }
}

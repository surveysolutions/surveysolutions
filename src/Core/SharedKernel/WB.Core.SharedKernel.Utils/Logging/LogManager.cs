﻿using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.SharedKernel.Logger;

namespace WB.Core.SharedKernel.Utils.Logging
{
    /// <summary>
    /// A manager class to use to get a logger for a certain type.
    /// </summary>
    public static class LogManager
    {
        private static readonly ReaderWriterLockSlim _cacheLocker = new ReaderWriterLockSlim();

        //private static bool _isLog4NetAvailable = false;
        private static Dictionary<Type, ILog> _loggerCache = new Dictionary<Type, ILog>();

        static LogManager()
        {
            /*try
            {
                Assembly.Load("log4net");
                _isLog4NetAvailable = true;
            }
            catch (FileNotFoundException)
            {
                _isLog4NetAvailable = false;
            }*/
        }

        public static ILog GetLogger(Type type)
        {
            ILog logger;
            _cacheLocker.EnterReadLock();

            try
            {
                if (_loggerCache.TryGetValue(type, out logger))
                {
                    return logger;
                }
            }
            finally
            {
                _cacheLocker.ExitReadLock();
            }
            
            _cacheLocker.EnterWriteLock();
            try
            {
                // double check, as while the read-lock was released, the dictionary could have been modified
                if (_loggerCache.TryGetValue(type, out logger))
                {
                    return logger;
                }

                logger = CreateLoggerForType(type);
                _loggerCache.Add(type, logger);
                return logger;
            }
            finally
            {
                _cacheLocker.ExitWriteLock();
            }
        }

        private static ILog CreateLoggerForType(Type type)
        {
            return new NLogLogger(global::NLog.LogManager.GetLogger(type.FullName));

            /*if (_isLog4NetAvailable)
            {
                return (ILog)Activator.CreateInstance(typeof(Log4NetLogger), new object[] { type });
            }
            else
            {
                return new TraceLogger();
            }*/
        }
    }
}
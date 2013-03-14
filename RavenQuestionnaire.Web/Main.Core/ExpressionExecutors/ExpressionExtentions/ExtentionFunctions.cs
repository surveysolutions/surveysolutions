// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtentionFunctions.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The extension functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.ExpressionExecutors.ExpressionExtentions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using NCalc;

    /// <summary>
    /// The extension functions.
    /// </summary>
    public static class ExtensionFunctions
    {
        private static Type[] CommonTypes = new[]
                                                {
                                                    typeof(Int64), typeof(Double), typeof(Boolean), typeof(String),
                                                    typeof(Decimal)
                                                };

        /// <summary>
        /// Gets the the most precise type.
        /// </summary>
        /// <param name="a">Type a.</param>
        /// <param name="b">Type b.</param>
        /// <returns></returns>
        private static Type GetMostPreciseType(Type a, Type b)
        {
            foreach (Type t in CommonTypes)
            {
                if (a == t || b == t)
                {
                    return t;
                }
            }

            return a;
        }

        public static int CompareUsingMostPreciseType(object a, object b)
        {
            if (a == null || b == null) return -1;
            Type mpt = GetMostPreciseType(a.GetType(), b.GetType());
            return Comparer.Default.Compare(Convert.ChangeType(a, mpt), Convert.ChangeType(b, mpt));
        }

        public static void EvaluateFunctionContains(string name, FunctionArgs args)
        {
            if (name == "contains")
            {
                if (args.Parameters.Length != 2) throw new ArgumentException("contains() takes 2 arguments");

                object parameter = args.Parameters[0].Evaluate();
                object argument = args.Parameters[1].Evaluate();

                bool evaluation = false;

                var values = parameter as IEnumerable<object>;

                if (values != null)
                {
                    ////multiselect
                    var enumerator = values.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (ExtensionFunctions.CompareUsingMostPreciseType(argument, enumerator.Current) == 0)
                        {
                            evaluation = true;
                            break;
                        }
                    }
                }
                else
                {
                    ////singleoption
                    if (ExtensionFunctions.CompareUsingMostPreciseType(argument, parameter) == 0)
                    {
                        evaluation = true;
                    }
                }

                args.Result = evaluation;
            }
        }

        public static Func<T, bool> AndAlso<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) && predicate2(arg);
        }

        public static Func<T, bool> OrElse<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) || predicate2(arg);
        }
    }
}

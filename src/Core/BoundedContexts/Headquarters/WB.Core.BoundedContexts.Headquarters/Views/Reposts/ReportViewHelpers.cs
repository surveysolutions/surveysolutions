using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public static class ReportViewHelpers
    {
        public static string AsColumnName(this CategoricalOption a)
        {
            // to be possible to use as unique id
            return "col_" + a.Value.ToString().Replace("-", "_");
        }

        public static ReportView ApplyOrderAndPaging(this ReportView report,
            IEnumerable<OrderRequestItem> orders, int page, int pageSize)
        {
            var result = new ReportView
            {
                Columns = report.Columns,
                Warnings = report.Warnings,
                Headers = report.Headers,
                TotalCount = report.Data.Length,
                Totals = report.Totals
            };

            bool thenBy = false;
            var data = report.Data.OrderBy(d => true);

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    var field = order.Field.Trim();

                    var index = Array.IndexOf(report.Columns, field);
                    if (index >= 0)
                    {
                        data = OrderBy(d => d[index]);
                    }

                    IOrderedEnumerable<object[]> OrderBy<T>(Func<object[], T> a)
                    {
                        if (thenBy)
                        {
                            return order.Direction == OrderDirection.Asc
                                ? data.ThenBy(a)
                                : data.ThenByDescending(a);
                        }

                        thenBy = true;
                        return order.Direction == OrderDirection.Asc
                            ? data.OrderBy(a)
                            : data.OrderByDescending(a);
                    }
                }
            }

            result.Data = data.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
            return result;
        }

        /// <summary>
        /// Set value to all items of array starting from index till arraLength
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Array to fill with value</param>
        /// <param name="value">Value to use as fill value</param>
        /// <param name="index">From which index to start with</param>
        /// <param name="arrayLength">Length of array. Default value = is length of <c>array</c></param>
        public static void SetDefault<T>(this object[] array, T value = default(T), int index = 0, int? arrayLength = null)
        {
            for (int i = index; i < (arrayLength ?? array.Length); i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Merge two Report Views into one by common index columns
        /// For example
        /// <c>RV1: (TL, TM, A, B, C)              RV2: (TL, TM, E, D)</c>
        /// <c>Merge(RV1, RV2, [TL, TM]) => (TL, TM, A, B, C, E, D) </c>
        /// </summary>
        /// <param name="a">First report view</param>
        /// <param name="b">Second report to merge</param>
        /// <param name="indexColumns">Which columns to use as index. Should be unique</param>
        /// <returns>Merged reports</returns>
        public static ReportView Merge(this ReportView a, ReportView b, params string[] indexColumns)
        {
            var report = new ReportView();

            IEnumerable<(string column, string header, object total)> GetColumnStructure()
            {
                for (int i = 0; i < a.Columns.Length; i++)
                {
                    yield return a.GetAt(i);
                }

                for (int i = 0; i < b.Columns.Length; i++)
                {
                    if (indexColumns.Contains(b.Columns[i])) continue;
                    yield return b.GetAt(i);
                }
            }

            var newStructure = GetColumnStructure().ToArray();

            report.Columns = newStructure.Select(c => c.column).ToArray();
            report.Headers = newStructure.Select(c => c.header).ToArray();
            report.Totals = newStructure.Select(c => c.total).ToArray();

            var reportBIndex = b.GetIndex(indexColumns);
            var reportAIndex = a.GetIndex(indexColumns);

            var reportData = new List<object[]>();

            foreach (var key in reportAIndex.Keys.Union(reportBIndex.Keys))
            {
                reportAIndex.TryGetValue(key, out var rowsA);
                reportBIndex.TryGetValue(key, out var rowsB);

                object[] result = new object[report.Columns.Length];
                reportData.Add(result);

                int index = 0;
                foreach (var col in report.Columns)
                {
                    var value = a.GetAt(rowsA, col) ?? b.GetAt(rowsB, col);
                    result[index] = value ?? 0L;
                    index++;
                }
            }

            report.Data = reportData.ToArray();
            report.Warnings = new HashSet<ReportWarnings>(a.Warnings.Union(b.Warnings));
            return report;
        }

        public static ReportView SelectColumns(this ReportView a, params string[] columns)
        {
            if (columns == null || columns.Length == 0) return a;

            if (columns.SequenceEqual(a.Columns)) return a;

            var report = new ReportView();

            IEnumerable<(string column, string header, object total)> GetColumnStructure()
            {
                for (int i = 0; i < a.Columns.Length; i++)
                {
                    if (columns.Contains(a.Columns[i]))
                    {
                        yield return a.GetAt(i);
                    }
                }
            }

            var newStructure = GetColumnStructure().ToArray();

            report.Columns = newStructure.Select(c => c.column).ToArray();
            report.Headers = newStructure.Select(c => c.header).ToArray();
            report.Totals = newStructure.Select(c => c.total).ToArray();
            report.Warnings = a.Warnings;

            var reportData = new List<object[]>();

            foreach (var row in a.Data)
            {
                object[] result = new object[report.Columns.Length];
                reportData.Add(result);

                int index = 0;
                foreach (var col in report.Columns)
                {
                    var value = a.GetAt(row, col);
                    result[index] = value;
                    index++;
                }
            }

            report.Data = reportData.ToArray();
            return report;
        }

        private static (string column, string header, object total) GetAt(this ReportView report, int index)
        {
            return (report.Columns[index], report.Headers[index], report.Totals[index]);
        }

        private static object GetAt(this ReportView report, object[] row, string columnName)
        {
            if (row == null) return null;
            var index = Array.IndexOf(report.Columns, columnName);

            if (index < 0) return null;

            return row[index];
        }

        private static string GetIndexValue(this ReportView report, object[] row, string[] columnNames)
        {
            var res = string.Empty;
            foreach (var column in columnNames)
            {
                var value = report.GetAt(row, column);
                if (value == null) res += "_null_";
                else res += value.ToString();
            }
            return res;
        }

        private static Dictionary<string, object[]> GetIndex(this ReportView report, string[] indexColumns)
        {
            return report.Data.ToDictionary(d => report.GetIndexValue(d, indexColumns));
        }
    }
}

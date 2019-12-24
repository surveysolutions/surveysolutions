using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Models.Api.DataTable
{
    class DataTablesRequestModelBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return Task.Factory.StartNew(() =>
            {
                BindModel(bindingContext);
            });
        }

        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext"></param>
        private void BindModel(ModelBindingContext bindingContext)
        {
            var valueProvider = bindingContext.ValueProvider;

            int draw, start, length;

            var valueResultProvider = valueProvider.GetValue("draw");
            if (valueProvider == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }
            TryParse<int>(valueResultProvider, out draw);
            TryParse<int>(valueProvider.GetValue("start"), out start);
            TryParse<int>(valueProvider.GetValue("length"), out length);

            var instance = Activator.CreateInstance(bindingContext.ModelType);
            DataTableRequest result = (DataTableRequest) instance;
            result.Draw = draw;
            result.Start = start;
            result.Length = length;
            result.Search = TryGetSearch(valueProvider);
            result.Order = TryGetOrders(valueProvider);
            result.Columns = TryGetColumns(valueProvider);
            
            bindingContext.Result = ModelBindingResult.Success(result);
        }

        /// <summary>
        /// Gets the search part of query
        /// </summary>
        /// <returns></returns>
        private DataTableRequest.SearchInfo TryGetSearch(IValueProvider valueProvider)
        {
            string searchValue;
            if (TryParse<string>(valueProvider.GetValue("search[value]"), out searchValue) &&
                !string.IsNullOrEmpty(searchValue))
            {
                bool regex = false;
                TryParse<bool>(valueProvider.GetValue("search[regex]"), out regex);
                return new DataTableRequest.SearchInfo()
                {
                    Regex = regex,
                    Value = searchValue
                };
            }
            return null;
        }

        /// <summary>
        /// Gets the list of columns in request
        /// </summary>
        /// <param name="valueProvider"></param>
        /// <returns></returns>
        private List<DataTableRequest.ColumnInfo> TryGetColumns(IValueProvider valueProvider)
        {
            //columns[0][data]:name
            //columns[0][name]:
            //columns[0][searchable]:true
            //columns[0][orderable]:true
            //columns[0][search][value]:
            //columns[0][search][regex]:false
            int index = 0;
            List<DataTableRequest.ColumnInfo> columns = new List<DataTableRequest.ColumnInfo>();

            // Count number of column
            do
            {
                if (valueProvider.GetValue($"columns[{index}][data]").FirstValue != null)
                {
                    string data, name, searchValue;
                    bool searchable, orderable, searchRegEx;
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][data]"), out data);
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][name]"), out name);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][searchable]"), out searchable);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][orderable]"), out orderable);
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][search][value]"), out searchValue);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][search][regex]"), out searchRegEx);

                    columns.Add(new DataTableRequest.ColumnInfo()
                    {
                        Name = name,
                        Searchable = searchable,
                        Orderable = orderable,
                        Search = new DataTableRequest.SearchInfo()
                        {
                            Regex = searchRegEx,
                            Value = searchValue,
                        },
                    });
                    index++;
                }
                else
                {
                    break;
                }
            } while (true);

            return columns;
        }

        /// <summary>
        /// Gets the list of order columns in request
        /// </summary>
        /// <param name="valueProvider"></param>
        /// <returns></returns>
        private List<DataTableRequest.SortOrder> TryGetOrders(IValueProvider valueProvider)
        {
            //order[0][column]:0
            //order[0][dir]:asc
            int index = 0;
            List<DataTableRequest.SortOrder> orders = new List<DataTableRequest.SortOrder>();

            do
            {
                if (valueProvider.GetValue($"order[{index}][column]").FirstValue != null)
                {
                    int column;
                    OrderDirection dir;
                    string name;
                    TryParse<int>(valueProvider.GetValue($"order[{index}][column]"), out column);
                    TryParse<OrderDirection>(valueProvider.GetValue($"order[{index}][dir]"), out dir);
                    TryParse<string>(valueProvider.GetValue($"order[{index}][name]"), out name);

                    orders.Add(new DataTableRequest.SortOrder() {
                        Column = column,
                        Dir = dir,
                        Name = name,
                    });
                    index++;
                }
                else
                {
                    break;
                }
            } while (true);

            return orders;
        }

        /// <summary>
        /// Try to gets the first value in the request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryParse<T>(ValueProviderResult value, out T result)
        {
            result = default(T);
            if (value == null || value.FirstValue == null) return false;

            try
            {
                result = (T)Convert.ChangeType(value.FirstValue, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

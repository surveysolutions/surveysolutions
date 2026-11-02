using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Models.Api.DataTable
{
    public class DataTablesRequestModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (typeof(DataTableRequest).IsAssignableFrom(context.Metadata.ModelType))
            {
                return new DataTablesRequestModelBinder(
                    new ComplexObjectModelBinderProvider().GetBinder(context));
            }

            return null;
        }
    }

    class DataTablesRequestModelBinder : IModelBinder
    {
        private readonly IModelBinder complexObjectModelBinder;

        public DataTablesRequestModelBinder(IModelBinder complexObjectModelBinder)
        {
            this.complexObjectModelBinder = complexObjectModelBinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await complexObjectModelBinder.BindModelAsync(bindingContext);

            if (bindingContext.Result.Model is not DataTableRequest request)
                return;

            request.Search = TryGetSearch(bindingContext.ValueProvider);
            request.Order = TryGetOrders(bindingContext.ValueProvider);

            var columns = TryGetColumns(bindingContext.ValueProvider);
            request._C = columns;
            request.Columns = columns;
        }

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
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][data]"), out _);
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][name]"), out var name);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][searchable]"), out var searchable);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][orderable]"), out var orderable);
                    TryParse<string>(valueProvider.GetValue($"columns[{index}][search][value]"), out var searchValue);
                    TryParse<bool>(valueProvider.GetValue($"columns[{index}][search][regex]"), out var searchRegEx);

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
                    TryParse<int>(valueProvider.GetValue($"order[{index}][column]"), out var column);

                    string orderDirection = valueProvider.GetValue($"order[{index}][dir]").FirstValue;
                    Enum.TryParse(orderDirection, true, out OrderDirection dir);

                    TryParse<string>(valueProvider.GetValue($"order[{index}][name]"), out var name);

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

        private bool TryParse<T>(ValueProviderResult value, out T result)
        {
            result = default(T);
            if (value.FirstValue == null) return false;

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WB.UI.Headquarters.Filters
{
    public class FormMultipartEncodedMediaTypeFormatter : MediaTypeFormatter
    {
        private const string SupportedMediaType = "multipart/form-data";

        public FormMultipartEncodedMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(SupportedMediaType));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);

            //need add boundary
            //(if add when fill SupportedMediaTypes collection in class constructor then receive post with another boundary will not work - Unsupported Media Type exception will thrown)
            if (headers.ContentType == null)
                headers.ContentType = new MediaTypeHeaderValue(SupportedMediaType);

            if (!String.Equals(headers.ContentType.MediaType, SupportedMediaType, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Not a Multipart Content");

            if (headers.ContentType.Parameters.All(m => m.Name != "boundary"))
                headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "MultipartDataMediaFormatterBoundary1q2w3e"));
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
                                                               IFormatterLogger formatterLogger)
        {
            var httpContentToFormDataConverter = new HttpContentToFormDataConverter();
            FormData multipartFormData = await httpContentToFormDataConverter.Convert(content);

            IFormDataConverterLogger logger;
            if (formatterLogger != null)
                logger = new FormatterLoggerAdapter(formatterLogger);
            else
                logger = new FormDataConverterLogger();

            var dataToObjectConverter = new FormDataToObjectConverter(multipartFormData, logger);
            object result = dataToObjectConverter.Convert(type);

            logger.EnsureNoErrors();

            return result;
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                TransportContext transportContext)
        {
            if (!content.IsMimeMultipartContent())
                throw new Exception("Not a Multipart Content");

            var boudaryParameter = content.Headers.ContentType.Parameters.FirstOrDefault(m => m.Name == "boundary" && !String.IsNullOrWhiteSpace(m.Value));
            if (boudaryParameter == null)
                throw new Exception("multipart boundary not found");

            var objectToMultipartDataByteArrayConverter = new ObjectToMultipartDataByteArrayConverter();
            byte[] multipartData = objectToMultipartDataByteArrayConverter.Convert(value, boudaryParameter.Value);

            await writeStream.WriteAsync(multipartData, 0, multipartData.Length);

            content.Headers.ContentLength = multipartData.Length;
        }
    }

    public class FormDataToObjectConverter
    {
        private readonly FormData SourceData;
        private readonly IFormDataConverterLogger Logger;

        public FormDataToObjectConverter(FormData sourceData, IFormDataConverterLogger logger)
        {
            if (sourceData == null)
                throw new ArgumentNullException("sourceData");
            if (logger == null)
                throw new ArgumentNullException("logger");

            SourceData = sourceData;
            Logger = logger;
        }

        public object Convert(Type destinitionType)
        {
            if (destinitionType == null)
                throw new ArgumentNullException("destinitionType");

            if (destinitionType == typeof(FormData))
                return SourceData;

            var objResult = CreateObject(destinitionType);
            return objResult;
        }

        private object CreateObject(Type destinitionType, string propertyName = "")
        {
            object propValue = null;

            object buf;
            if (TryGetFromFormData(destinitionType, propertyName, out buf)
                || TryGetAsGenericDictionary(destinitionType, propertyName, out buf)
                || TryGetAsGenericListOrArray(destinitionType, propertyName, out buf)
                || TryGetAsCustomType(destinitionType, propertyName, out buf))
            {
                propValue = buf;
            }
            else if (!IsFileOrConvertableFromString(destinitionType))
            {
                Logger.LogError(propertyName, String.Format("Cannot parse type \"{0}\".", destinitionType.FullName));
            }

            return propValue;
        }

        private bool TryGetFromFormData(Type destinitionType, string propertyName, out object propValue)
        {
            bool existsInFormData = false;
            propValue = null;

            if (destinitionType == typeof(HttpFile))
            {
                HttpFile httpFile;
                if (SourceData.TryGetValue(propertyName, out httpFile))
                {
                    propValue = httpFile;
                    existsInFormData = true;
                }
            }
            else
            {
                string val;
                if (SourceData.TryGetValue(propertyName, out val))
                {
                    existsInFormData = true;
                    var typeConverter = destinitionType.GetFromStringConverter();
                    if (typeConverter == null)
                    {
                        Logger.LogError(propertyName, "Cannot find type converter for field - " + propertyName);
                    }
                    else
                    {
                        try
                        {
                            propValue = typeConverter.ConvertFromString(null, CultureInfo.CurrentCulture, val);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(propertyName, String.Format("Error parsing field \"{0}\": {1}", propertyName, ex.Message));
                        }
                    }
                }
            }

            return existsInFormData;
        }

        private bool TryGetAsGenericDictionary(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            Type keyType, valueType;
            bool isGenericDictionary = IsGenericDictionary(destinitionType, out keyType, out valueType);
            if (isGenericDictionary)
            {
                var dictType = typeof(Dictionary<,>).MakeGenericType(new[] { keyType, valueType });
                var add = dictType.GetMethod("Add");

                var pValue = Activator.CreateInstance(dictType);

                int index = 0;
                string origPropName = propertyName;
                bool isFilled = false;
                while (true)
                {
                    string propertyKeyName = String.Format("{0}[{1}].Key", origPropName, index);
                    var objKey = CreateObject(keyType, propertyKeyName);
                    if (objKey != null)
                    {
                        string propertyValueName = String.Format("{0}[{1}].Value", origPropName, index);
                        var objValue = CreateObject(valueType, propertyValueName);

                        if (objValue != null)
                        {
                            add.Invoke(pValue, new[] { objKey, objValue });
                            isFilled = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                    index++;
                }

                if (isFilled)
                {
                    propValue = pValue;
                }
            }

            return isGenericDictionary;
        }

        private bool TryGetAsGenericListOrArray(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            Type genericListItemType;
            bool isGenericList = IsGenericListOrArray(destinitionType, out genericListItemType);
            if (isGenericList)
            {
                var listType = typeof(List<>).MakeGenericType(genericListItemType);

                var add = listType.GetMethod("Add");
                var pValue = Activator.CreateInstance(listType);

                int index = 0;
                string origPropName = propertyName;
                bool isFilled = false;
                while (true)
                {
                    propertyName = String.Format("{0}[{1}]", origPropName, index);
                    var objValue = CreateObject(genericListItemType, propertyName);
                    if (objValue != null)
                    {
                        add.Invoke(pValue, new[] { objValue });
                        isFilled = true;
                    }
                    else
                    {
                        break;
                    }

                    index++;
                }

                if (isFilled)
                {
                    if (destinitionType.IsArray)
                    {
                        var toArrayMethod = listType.GetMethod("ToArray");
                        propValue = toArrayMethod.Invoke(pValue, new object[0]);
                    }
                    else
                    {
                        propValue = pValue;
                    }
                }
            }

            return isGenericList;
        }

        private bool TryGetAsCustomType(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            bool isCustomNonEnumerableType = destinitionType.IsCustomNonEnumerableType();
            if (isCustomNonEnumerableType)
            {
                if (String.IsNullOrWhiteSpace(propertyName)
                    || SourceData.AllKeys().Any(m => m.StartsWith(propertyName + ".", StringComparison.CurrentCultureIgnoreCase)))
                {
                    var obj = Activator.CreateInstance(destinitionType);
                    bool isFilled = false;
                    foreach (PropertyInfo propertyInfo in destinitionType.GetPublicAccessibleProperties())
                    {
                        var propName = (!String.IsNullOrEmpty(propertyName) ? propertyName + "." : "") + propertyInfo.Name;
                        var objValue = CreateObject(propertyInfo.PropertyType, propName);
                        if (objValue != null)
                        {
                            propertyInfo.SetValue(obj, objValue);
                            isFilled = true;
                        }
                    }
                    if (isFilled)
                    {
                        propValue = obj;
                    }
                }
            }
            return isCustomNonEnumerableType;
        }


        private bool IsGenericDictionary(Type type, out Type keyType, out Type valueType)
        {
            Type iDictType = type.GetInterface(typeof(IDictionary<,>).Name);
            if (iDictType != null)
            {
                var types = iDictType.GetGenericArguments();
                if (types.Length == 2)
                {
                    keyType = types[0];
                    valueType = types[1];
                    return true;
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }

        private bool IsGenericListOrArray(Type type, out Type itemType)
        {
            if (type.GetInterface(typeof(IDictionary<,>).Name) == null) //not a dictionary
            {
                if (type.IsArray)
                {
                    itemType = type.GetElementType();
                    return true;
                }

                Type iListType = type.GetInterface(typeof(ICollection<>).Name);
                if (iListType != null)
                {
                    Type[] genericArguments = iListType.GetGenericArguments();
                    if (genericArguments.Length == 1)
                    {
                        itemType = genericArguments[0];
                        return true;
                    }
                }
            }

            itemType = null;
            return false;
        }

        private bool IsFileOrConvertableFromString(Type type)
        {
            if (type == typeof(HttpFile))
                return true;

            return type.GetFromStringConverter() != null;
        }
    }

    public class HttpContentToFormDataConverter
    {
        public async Task<FormData> Convert(HttpContent content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            if (!content.IsMimeMultipartContent())
            {
                throw new Exception("Unsupported Media Type");
            }

            MultipartMemoryStreamProvider multipartProvider = await content.ReadAsMultipartAsync();

            var multipartFormData = await Convert(multipartProvider);
            return multipartFormData;
        }

        public async Task<FormData> Convert(MultipartMemoryStreamProvider multipartProvider)
        {
            var multipartFormData = new FormData();

            foreach (var file in multipartProvider.Contents.Where(x => IsFile(x.Headers.ContentDisposition)))
            {
                var name = UnquoteToken(file.Headers.ContentDisposition.Name);
                string fileName = FixFilename(file.Headers.ContentDisposition.FileName);
                string mediaType = file.Headers.ContentType.MediaType;

                using (var stream = await file.ReadAsStreamAsync())
                {
                    multipartFormData.Add(name, new HttpFile(fileName, mediaType, stream));
                }
            }

            foreach (var part in multipartProvider.Contents.Where(x => x.Headers.ContentDisposition.DispositionType == "form-data"
                                                                  && !IsFile(x.Headers.ContentDisposition)))
            {
                var name = UnquoteToken(part.Headers.ContentDisposition.Name);
                var data = await part.ReadAsStringAsync();
                multipartFormData.Add(name, data);
            }

            return multipartFormData;
        }

        private bool IsFile(ContentDispositionHeaderValue disposition)
        {
            return !string.IsNullOrEmpty(disposition.FileName);
        }

        /// <summary>
        /// Remove bounding quotes on a token if present
        /// </summary>
        private static string UnquoteToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }

        /// <summary>
        /// Amend filenames to remove surrounding quotes and remove path from IE
        /// </summary>
        private static string FixFilename(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return string.Empty;

            var result = originalFileName.Trim();

            // remove leading and trailing quotes
            result = result.Trim('"');

            // remove full path versions
            if (result.Contains("\\"))
                result = Path.GetFileName(result);

            return result;
        }
    }

    public class ObjectToMultipartDataByteArrayConverter
    {
        public byte[] Convert(object value, string boundary)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (String.IsNullOrWhiteSpace(boundary))
                throw new ArgumentNullException("boundary");

            List<KeyValuePair<string, object>> propertiesList = ConvertObjectToFlatPropertiesList(value);
            byte[] buffer = GetMultipartFormDataBytes(propertiesList, boundary);
            return buffer;
        }

        private List<KeyValuePair<string, object>> ConvertObjectToFlatPropertiesList(object value)
        {
            var propertiesList = new List<KeyValuePair<string, object>>();
            if (value is FormData)
            {
                FillFlatPropertiesListFromFormData((FormData)value, propertiesList);
            }
            else
            {
                FillFlatPropertiesListFromObject(value, "", propertiesList);
            }

            return propertiesList;
        }

        private void FillFlatPropertiesListFromFormData(FormData formData, List<KeyValuePair<string, object>> propertiesList)
        {
            foreach (var field in formData.Fields)
            {
                propertiesList.Add(new KeyValuePair<string, object>(field.Name, field.Value));
            }
            foreach (var field in formData.Files)
            {
                propertiesList.Add(new KeyValuePair<string, object>(field.Name, field.Value));
            }
        }

        private void FillFlatPropertiesListFromObject(object obj, string prefix, List<KeyValuePair<string, object>> propertiesList)
        {
            if (obj != null)
            {
                Type type = obj.GetType();

                if (obj is IDictionary)
                {
                    var dict = obj as IDictionary;
                    int index = 0;
                    foreach (var key in dict.Keys)
                    {
                        string indexedKeyPropName = String.Format("{0}[{1}].Key", prefix, index);
                        FillFlatPropertiesListFromObject(key, indexedKeyPropName, propertiesList);

                        string indexedValuePropName = String.Format("{0}[{1}].Value", prefix, index);
                        FillFlatPropertiesListFromObject(dict[key], indexedValuePropName, propertiesList);

                        index++;
                    }
                }
                else if (obj is ICollection)
                {
                    var list = obj as ICollection;
                    int index = 0;
                    foreach (var indexedPropValue in list)
                    {
                        string indexedPropName = String.Format("{0}[{1}]", prefix, index);
                        FillFlatPropertiesListFromObject(indexedPropValue, indexedPropName, propertiesList);

                        index++;
                    }
                }
                else if (type.IsCustomNonEnumerableType())
                {
                    foreach (var propertyInfo in type.GetPublicAccessibleProperties())
                    {
                        string propName = String.IsNullOrWhiteSpace(prefix)
                                              ? propertyInfo.Name
                                              : String.Format("{0}.{1}", prefix, propertyInfo.Name);
                        object propValue = propertyInfo.GetValue(obj);

                        FillFlatPropertiesListFromObject(propValue, propName, propertiesList);
                    }
                }
                else
                {
                    propertiesList.Add(new KeyValuePair<string, object>(prefix, obj));
                }
            }
        }

        private byte[] GetMultipartFormDataBytes(List<KeyValuePair<string, object>> postParameters, string boundary)
        {
            Encoding encoding = Encoding.UTF8;

            using (var formDataStream = new System.IO.MemoryStream())
            {
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    // Add a CRLF to allow multiple parameters to be added.
                    // Skip it on the first parameter, add it to subsequent parameters.
                    if (needsCLRF)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    needsCLRF = true;

                    if (param.Value is HttpFile)
                    {
                        HttpFile httpFileToUpload = (HttpFile)param.Value;

                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header =
                            string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                                boundary,
                                param.Key,
                                httpFileToUpload.FileName ?? param.Key,
                                httpFileToUpload.MediaType ?? "application/octet-stream");

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        httpFileToUpload.FileStream = formDataStream;
                    }
                    else
                    {
                        string objString = "";
                        if (param.Value != null)
                        {
                            var typeConverter = param.Value.GetType().GetToStringConverter();
                            if (typeConverter != null)
                            {
                                objString = typeConverter.ConvertToString(null, CultureInfo.CurrentCulture, param.Value);
                            }
                            else
                            {
                                throw new Exception(String.Format("Type \"{0}\" cannot be converted to string", param.Value.GetType().FullName));
                            }
                        }

                        string postData =
                            string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                          boundary,
                                          param.Key,
                                          objString);
                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                byte[] formData = formDataStream.ToArray();

                return formData;
            }
        }
    }

    public class HttpFile
    {
        public string FileName { get; set; }
        public string MediaType { get; set; }
        public Stream FileStream { get; set; }

        public HttpFile() { }

        public HttpFile(string fileName, string mediaType, Stream fileStream)
        {
            FileName = fileName;
            MediaType = mediaType;
            this.FileStream = fileStream;
        }
    }

    public class FormData
    {
        private List<ValueFile> _Files;
        private List<ValueString> _Fields;

        public List<ValueFile> Files
        {
            get
            {
                if (_Files == null)
                    _Files = new List<ValueFile>();
                return _Files;
            }
            set
            {
                _Files = value;
            }
        }

        public List<ValueString> Fields
        {
            get
            {
                if (_Fields == null)
                    _Fields = new List<ValueString>();
                return _Fields;
            }
            set
            {
                _Fields = value;
            }
        }

        public IEnumerable<string> AllKeys()
        {
            return Fields.Select(m => m.Name).Union(Files.Select(m => m.Name));
        }

        public void Add(string name, string value)
        {
            Fields.Add(new ValueString() { Name = name, Value = value });
        }

        public void Add(string name, HttpFile value)
        {
            Files.Add(new ValueFile() { Name = name, Value = value });
        }

        public bool TryGetValue(string name, out string value)
        {
            var field = Fields.FirstOrDefault(m => String.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (field != null)
            {
                value = field.Value;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetValue(string name, out HttpFile value)
        {
            var field = Files.FirstOrDefault(m => String.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (field != null)
            {
                value = field.Value;
                return true;
            }
            value = null;
            return false;
        }

        public class ValueString
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class ValueFile
        {
            public string Name { get; set; }
            public HttpFile Value { get; set; }
        }
    }

    internal class DateTimeConverterISO8601 : DateTimeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && value is DateTime && destinationType == typeof(string))
            {
                return ((DateTime)value).ToString("O"); // ISO 8601
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class FormDataConverterLogger : IFormDataConverterLogger
    {
        private Dictionary<string, List<LogErrorInfo>> Errors { get; set; }

        public FormDataConverterLogger()
        {
            Errors = new Dictionary<string, List<LogErrorInfo>>();
        }

        public void LogError(string errorPath, Exception exception)
        {
            AddError(errorPath, new LogErrorInfo(exception));
        }

        public void LogError(string errorPath, string errorMessage)
        {
            AddError(errorPath, new LogErrorInfo(errorMessage));
        }

        public List<LogItem> GetErrors()
        {
            return Errors.Select(m => new LogItem()
            {
                ErrorPath = m.Key,
                Errors = m.Value.Select(t => t).ToList()
            }).ToList();
        }

        public void EnsureNoErrors()
        {
            if (Errors.Any())
            {
                var errors = Errors
                    .SelectMany(m => m.Value)
                    .Select(m => (m.ErrorMessage ?? (m.Exception != null ? m.Exception.Message : "")))
                    .ToList();

                string errorMessage = String.Join(" ", errors);

                throw new Exception(errorMessage);
            }
        }

        private void AddError(string errorPath, LogErrorInfo info)
        {
            List<LogErrorInfo> listErrors;
            if (!Errors.TryGetValue(errorPath, out listErrors))
            {
                listErrors = new List<LogErrorInfo>();
                Errors.Add(errorPath, listErrors);
            }
            listErrors.Add(info);
        }

        public class LogItem
        {
            public string ErrorPath { get; set; }
            public List<LogErrorInfo> Errors { get; set; }
        }

        public class LogErrorInfo
        {
            public string ErrorMessage { get; private set; }
            public Exception Exception { get; private set; }
            public bool IsException { get; private set; }

            public LogErrorInfo(string errorMessage)
            {
                ErrorMessage = errorMessage;
                IsException = false;
            }

            public LogErrorInfo(Exception exception)
            {
                Exception = exception;
                IsException = true;
            }
        }
    }

    internal class FormatterLoggerAdapter : IFormDataConverterLogger
    {
        private IFormatterLogger FormatterLogger { get; set; }

        public FormatterLoggerAdapter(IFormatterLogger formatterLogger)
        {
            if (formatterLogger == null)
                throw new ArgumentNullException("formatterLogger");
            FormatterLogger = formatterLogger;
        }

        public void LogError(string errorPath, Exception exception)
        {
            FormatterLogger.LogError(errorPath, exception);
        }

        public void LogError(string errorPath, string errorMessage)
        {
            FormatterLogger.LogError(errorPath, errorMessage);
        }

        public void EnsureNoErrors()
        {
            //nothing to do
        }
    }

    public interface IFormDataConverterLogger
    {
        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="errorPath">The path to the member for which the error is being logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        void LogError(string errorPath, Exception exception);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="errorPath">The path to the member for which the error is being logged.</param>
        /// <param name="errorMessage">The error message to be logged.</param>
        void LogError(string errorPath, string errorMessage);

        /// <summary>
        /// throw exception if errors found
        /// </summary>
        void EnsureNoErrors();
    }

    internal static class TypeExtensions
    {
        internal static TypeConverter GetFromStringConverter(this Type type)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
            if (typeConverter != null && !typeConverter.CanConvertFrom(typeof(String)))
            {
                typeConverter = null;
            }
            return typeConverter;
        }

        internal static TypeConverter GetToStringConverter(this Type type)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
            if (typeConverter is DateTimeConverter)
            {
                //replace default datetime converter for serializing datetime in ISO 8601 format
                typeConverter = new DateTimeConverterISO8601();
            }
            if (typeConverter != null && !typeConverter.CanConvertTo(typeof(String)))
            {
                typeConverter = null;
            }
            return typeConverter;
        }

        internal static IEnumerable<PropertyInfo> GetPublicAccessibleProperties(this Type type)
        {
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                if (!propertyInfo.CanRead || !propertyInfo.CanWrite || propertyInfo.SetMethod == null || propertyInfo.SetMethod.IsPrivate)
                    continue;
                yield return propertyInfo;
            }
        }

        internal static bool IsCustomNonEnumerableType(this Type type)
        {
            var nullType = Nullable.GetUnderlyingType(type);
            if (nullType != null)
            {
                type = nullType;
            }
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            return type != typeof(object)
                   && Type.GetTypeCode(type) == TypeCode.Object
                   && type != typeof(HttpFile)
                   && type != typeof(Guid)
                   && type.GetInterface(typeof(IEnumerable).Name) == null;
        }
    }
}
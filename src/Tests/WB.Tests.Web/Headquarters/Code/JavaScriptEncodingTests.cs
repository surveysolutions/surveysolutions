using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Web.Headquarters.Code
{
    [TestFixture]
    public class JavaScriptEncodingTests
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new JsonJavaScriptEncodeConverter() }
        };

        private static string Serialize(object model) => JsonConvert.SerializeObject(model, Settings);

        private static string SafeMessage(string source)
        {
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(x => x.Raw(It.IsAny<string>())).Returns<string>(s => new HtmlString(s));
            return helper.Object.ToSafeJavascriptMessage(source).ToString();
        }

        [TestCase("hello world", "hello world")]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("it's", "it\\u0027s")]
        [TestCase("say \"hi\"", "say \\u0022hi\\u0022")]
        [TestCase("back\\slash", "back\\\\slash")]
        [TestCase("line1\nline2\r\ttab", "line1\\nline2\\r\\ttab")]
        [TestCase("</script>", "\\u003C/script\\u003E")]
        [TestCase("<img src=x onerror=alert(1)>", "\\u003Cimg src=x onerror=alert(1)\\u003E")]
        [TestCase("a&b", "a\\u0026b")]
        public void ToSafeJavascriptMessage_should_escape_dangerous_characters(string source, string expected)
        {
            SafeMessage(source).Should().Be(expected);
        }

        [Test]
        public void ToSafeJavascriptMessage_should_escape_line_separators_and_non_ascii()
        {
            var result = SafeMessage("\u2028\u2029é中😀");

            result.Should().NotContain("\u2028").And.NotContain("\u2029");
            result.Should().Be("\\u2028\\u2029\\u00E9\\u4E2D\\uD83D\\uDE00");
        }

        [TestCase("hello", "\"hello\"")]
        [TestCase("", "\"\"")]
        [TestCase("it's", "\"it\\u0027s\"")]
        [TestCase("say \"hi\"", "\"say \\u0022hi\\u0022\"")]
        [TestCase("</script><script>alert(1)</script>", "\"\\u003C/script\\u003E\\u003Cscript\\u003Ealert(1)\\u003C/script\\u003E\"")]
        [TestCase("a\\b\nc", "\"a\\\\b\\nc\"")]
        public void Converter_should_write_quoted_escaped_string(string value, string expected)
        {
            Serialize(value).Should().Be(expected);
        }

        [Test]
        public void Converter_should_produce_valid_json_that_roundtrips_original_values()
        {
            var values = new[]
            {
                "plain", "", "it's \"quoted\"", "</script>", "<!--", "a&b", "back\\slash",
                "\u2028\u2029", "é中😀", "tab\tnew\nline\r", "\0", "\u0001\u001f", "{\"a\":1}", "' + alert(1) + '"
            };

            foreach (var value in values)
            {
                var json = Serialize(new { Value = value });

                JsonConvert.DeserializeObject<Dictionary<string, string>>(json)["value"].Should().Be(value);
            }
        }

        [Test]
        public void Converter_should_not_leak_html_sensitive_characters_into_output()
        {
            var json = Serialize(new { Text = "</script><script>alert('x')</script><!-- & -->" });

            json.Should().NotContain("<").And.NotContain(">").And.NotContain("&").And.NotContain("'");
        }

        [Test]
        public void Converter_should_handle_nested_objects_collections_and_camel_case()
        {
            var json = Serialize(new
            {
                Title = "</script>",
                Items = new[] { "a'b", "c" },
                Child = new { InnerText = "x\"y" },
                Number = 5
            });

            json.Should().Be("{\"title\":\"\\u003C/script\\u003E\",\"items\":[\"a\\u0027b\",\"c\"]," +
                             "\"child\":{\"innerText\":\"x\\u0022y\"},\"number\":5}");
        }

        [Test]
        public void Converter_should_serialize_null_string_property_without_failing()
        {
            var json = Serialize(new { Text = (string)null });

            json.Should().BeOneOf("{\"text\":null}", "{\"text\":\"\"}");
        }

        [Test]
        public void Converter_should_only_convert_strings()
        {
            var converter = new JsonJavaScriptEncodeConverter();

            converter.CanConvert(typeof(string)).Should().BeTrue();
            converter.CanConvert(typeof(int)).Should().BeFalse();
            converter.CanConvert(typeof(object)).Should().BeFalse();
        }

        [Test]
        public void Converter_should_read_plain_string_values()
        {
            JsonConvert.DeserializeObject<string>("\"abc\"", Settings).Should().Be("abc");
        }

        [Test]
        public void RenderHqConfig_should_escape_title_and_model_inside_script_block()
        {
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(x => x.Raw(It.IsAny<string>())).Returns<string>(s => new HtmlString(s));

            var html = helper.Object.RenderHqConfig(new { Name = "</script><b>" }, "ti\"tle</script>").ToString();

            html.Should().Be("<script>window.CONFIG.title=\"ti\\u0022tle\\u003C/script\\u003E\";" +
                             "window.CONFIG.model={\"name\":\"\\u003C/script\\u003E\\u003Cb\\u003E\"}</script>");
        }
    }
}

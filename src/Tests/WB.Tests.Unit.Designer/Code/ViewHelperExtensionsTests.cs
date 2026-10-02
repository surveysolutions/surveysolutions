using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Designer.BootstrapSupport;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    public class ViewHelperExtensionsTests
    {
        private class SampleController { public void Index() { } }

        private class WithKey
        {
            public string Name { get; set; } = "";
            [Key] public int Custom { get; set; }
        }

        private class WithId { public int Id { get; set; } public string Title { get; set; } = ""; }

        private class WithTypeId { public int WithTypeIdId { get; set; } }

        private class WithoutId { public string Title { get; set; } = ""; }

        private class Ordered
        {
            [Display(Order = 2)] public string Second { get; set; } = "";
            [Display(Order = 1)] public string First { get; set; } = "";
            public string Unordered { get; set; } = "";
        }

        [DisplayName("Custom label")]
        private class Labeled { }

        private class SomeEntityName { }

        [Test]
        public void GetControllerName_should_strip_suffix()
        {
            typeof(SampleController).GetControllerName().Should().Be("Sample");
        }

        [Test]
        public void GetActionName_should_return_called_method_name()
        {
            Expression<Action<SampleController>> expr = c => c.Index();
            expr.GetActionName().Should().Be("Index");
        }

        [Test]
        public void IdentifierPropertyName_should_prefer_key_attribute()
        {
            new WithKey().IdentifierPropertyName().Should().Be("Custom");
        }

        [Test]
        public void IdentifierPropertyName_should_fall_back_to_id_then_typename_id()
        {
            typeof(WithId).IdentifierPropertyName().Should().Be("Id");
            typeof(WithTypeId).IdentifierPropertyName().Should().Be("WithTypeIdId");
            typeof(WithoutId).IdentifierPropertyName().Should().BeEmpty();
        }

        [Test]
        public void VisibleProperties_should_exclude_identifier()
        {
            new WithId().VisibleProperties().Select(p => p.Name).Should().Equal("Title");
        }

        [Test]
        public void OrderedByDisplayAttr_should_use_order()
        {
            typeof(Ordered).GetProperties().OrderedByDisplayAttr().Select(p => p.Name)
                .Should().Equal("Unordered", "First", "Second");
        }

        [Test]
        public void GetIdValue_should_return_route_values_with_identifier()
        {
            var values = new WithId { Id = 7 }.GetIdValue();

            values["Id"].Should().Be(7);
        }

        [Test]
        public void AddArea_should_return_copy_with_area()
        {
            var original = new WithId { Id = 7 }.GetIdValue();
            var withArea = original.AddArea("Admin");

            withArea["area"].Should().Be("Admin");
            original.ContainsKey("area").Should().BeFalse();
        }

        [Test]
        public void IsDateTimePropertyValue_should_detect_datetime()
        {
            typeof(DateHolder).GetProperty(nameof(DateHolder.When))!.IsDateTimePropertyValue().Should().BeTrue();
            typeof(DateHolder).GetProperty(nameof(DateHolder.Text))!.IsDateTimePropertyValue().Should().BeFalse();
        }

        private class DateHolder { public DateTime When { get; set; } public string Text { get; set; } = ""; }

        [Test]
        public void ToSeparatedWords_should_split_pascal_case()
        {
            "SomeEntityName".ToSeparatedWords().Should().Be("Some Entity Name");
        }

        [Test]
        public void AttributeExists_and_GetAttribute_should_work_for_properties_and_types()
        {
            var keyProp = typeof(WithKey).GetProperty(nameof(WithKey.Custom))!;
            var plainProp = typeof(WithKey).GetProperty(nameof(WithKey.Name))!;

            keyProp.AttributeExists<KeyAttribute>().Should().BeTrue();
            plainProp.AttributeExists<KeyAttribute>().Should().BeFalse();
            keyProp.GetAttribute<KeyAttribute>().Should().NotBeNull();
            plainProp.GetAttribute<KeyAttribute>().Should().BeNull();
            typeof(Labeled).AttributeExists<DisplayNameAttribute>().Should().BeTrue();
            typeof(WithKey).GetAttribute<DisplayNameAttribute>().Should().BeNull();
        }

        [Test]
        public void LabelFromType_should_use_display_name_or_separated_type_name()
        {
            PropertyInfoExtensions.LabelFromType(typeof(Labeled)).Should().Be("Custom label");
            PropertyInfoExtensions.LabelFromType(typeof(SomeEntityName)).Should().Be("Some Entity Name");
            new SomeEntityName().GetLabel().Should().Be("Some Entity Name");
        }

        [Test]
        public void GetLabel_for_collection_should_use_element_type()
        {
            new[] { new SomeEntityName() }.GetLabel().Should().Be("Some Entity Name");
            new System.Collections.Generic.List<Labeled>().GetLabel().Should().Be("Custom label");
        }
    }
}


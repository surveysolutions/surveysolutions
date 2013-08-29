using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Status;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;

namespace Core.Supervisor.Tests.Views
{
    [TestFixture]
    public class StatusViewTests
    {
        [Test]
        public void Ctor_When_Grid_is_empty_header_is_not_empty_Then_final_header_is_still_not_empty()
        {
            // arrange
            var header = new List<TemplateLight>() {new TemplateLight(Guid.NewGuid(), "test")};
            var items = Enumerable.Empty<StatusViewItem>();

            // act
            StatusView target = CreateStatusViewWithHeaderAndItems(header, items);

            // assert
            Assert.That(target.Headers.Count, Is.EqualTo(header.Count));
        }

        [Test]
        public void Ctor_When_item_is_not_presented_in_header_Then_final_header_is_still_not_empty_item_is_present_in_grid()
        {
            // arrange
            var header = new List<TemplateLight>() { new TemplateLight(Guid.NewGuid(), "test") };
            var items = new StatusViewItem[]
                {
                    new StatusViewItem(new UserLight(Guid.NewGuid(), "user"),
                                       new Dictionary<Guid, int>() {{Guid.NewGuid(), 33}})
                };

            // act
            StatusView target = CreateStatusViewWithHeaderAndItems(header, items);

            // assert
            Assert.That(target.Headers.Count, Is.EqualTo(header.Count));
        }

        [Test]
        public void Ctor_When_item_is_presented_in_header_Then_header_is_not_empty_one_item_is_present()
        {
            // arrange
            var templateId = Guid.NewGuid();
            var questionnarieCount = 33;
            var header = new List<TemplateLight>() { new TemplateLight(templateId, "test") };
            var items = new StatusViewItem[]
                {
                    new StatusViewItem(new UserLight(Guid.NewGuid(), "user"),
                                       new Dictionary<Guid, int>() {{templateId,questionnarieCount}})
                };

            // act
            StatusView target = CreateStatusViewWithHeaderAndItems(header, items);

            // assert

            //check header values
            Assert.That(target.Headers.Count, Is.EqualTo(1));
            Assert.That(target.Headers[0].TemplateId, Is.EqualTo(templateId));

            //check item value
            Assert.That(target.Items.Count, Is.EqualTo(1));
            Assert.That(target.Items[0].GetCount(templateId), Is.EqualTo(questionnarieCount));

        }

        private static StatusView CreateStatusViewWithHeaderAndItems(List<TemplateLight> header, IEnumerable<StatusViewItem> items)
        {
            return new StatusView(0, 10, SurveyStatus.Initial,
                                  header,
                                  items);
        }
    }
}

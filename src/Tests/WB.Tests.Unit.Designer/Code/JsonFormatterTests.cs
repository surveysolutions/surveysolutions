using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    public class JsonFormatterTests
    {
        [Test]
        public async Task WriteToStreamAsync_when_HQ_version_is_5_22_and_work_with_moved_classes()
        {
            var typeToSerialize = new PagedQuestionnaireCommunicationPackage()
            {
                TotalCount = 11,
                Items = new []
                {
                    new QuestionnaireListItem()
                    {
                        OwnerName = "owner1",
                        Version = new QuestionnaireVersion() { Major = 1, Minor = 1, Patch = 1}
                    },
                    new QuestionnaireListItem()
                    {
                        OwnerName = "owner2",
                        Version = new QuestionnaireVersion() { Major = 2, Minor = 2, Patch = 2}
                    }, 
                }
            };
            var memoryStream = new MemoryStream();
            JsonFormatter jsonFormatter = Create.JsonFormatter(new Version(5, 22, 0));

            await jsonFormatter.WriteToStreamAsync(typeToSerialize.GetType(), typeToSerialize, memoryStream, null, null);

            var array = memoryStream.ToArray();
            string text = System.Text.Encoding.UTF8.GetString(array);
            Assert.AreEqual(text, "{\"$type\":\"PagedQuestionnaireCommunicationPackage\",\"Items\":[{\"$type\":\"QuestionnaireListItem\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"OwnerName\":\"owner1\",\"Version\":{\"$type\":\"QuestionnaireVersion\",\"Major\":1,\"Minor\":1,\"Patch\":1}},{\"$type\":\"QuestionnaireListItem\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"OwnerName\":\"owner2\",\"Version\":{\"$type\":\"QuestionnaireVersion\",\"Major\":2,\"Minor\":2,\"Patch\":2}}],\"TotalCount\":11}");
        }

        [Test]
        public async Task WriteToStreamAsync_when_HQ_version_is_5_21_and_work_with_moved_classes()
        {
            var typeToSerialize = new PagedQuestionnaireCommunicationPackage()
            {
                TotalCount = 11,
                Items = new[]
                {
                    new QuestionnaireListItem()
                    {
                        OwnerName = "owner1",
                        Version = new QuestionnaireVersion() { Major = 1, Minor = 1, Patch = 1}
                    },
                    new QuestionnaireListItem()
                    {
                        OwnerName = "owner2",
                        Version = new QuestionnaireVersion() { Major = 2, Minor = 2, Patch = 2}
                    },
                }
            };
            var memoryStream = new MemoryStream();
            JsonFormatter jsonFormatter = Create.JsonFormatter(new Version(5, 21, 0));

            await jsonFormatter.WriteToStreamAsync(typeToSerialize.GetType(), typeToSerialize, memoryStream, null, null);

            var array = memoryStream.ToArray();
            string text = System.Text.Encoding.UTF8.GetString(array);
            Assert.AreEqual(text, "{\"$type\":\"WB.Core.SharedKernel.Structures.Synchronization.Designer.PagedQuestionnaireCommunicationPackage, WB.Core.SharedKernel.Structures\",\"Items\":[{\"$type\":\"WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireListItem, WB.Core.SharedKernel.Structures\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"OwnerName\":\"owner1\",\"Version\":{\"$type\":\"WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion, WB.Core.SharedKernel.Structures\",\"Major\":1,\"Minor\":1,\"Patch\":1}},{\"$type\":\"WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireListItem, WB.Core.SharedKernel.Structures\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"OwnerName\":\"owner2\",\"Version\":{\"$type\":\"WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion, WB.Core.SharedKernel.Structures\",\"Major\":2,\"Minor\":2,\"Patch\":2}}],\"TotalCount\":11}");
        }

        [Test]
        public async Task ReadFromStreamAsync_when_HQ_version_is_5_21_and_work_with_moved_classes()
        {
            var serializedType = @"{""$type"":""PagedQuestionnaireCommunicationPackage"",""TotalCount"":11}";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedType));
            JsonFormatter jsonFormatter = Create.JsonFormatter(new Version(21, 0));

            var result = await jsonFormatter.ReadFromStreamAsync(typeof(PagedQuestionnaireCommunicationPackage), memoryStream, null, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(PagedQuestionnaireCommunicationPackage));

            var package = (PagedQuestionnaireCommunicationPackage) result;
            Assert.AreEqual(package.TotalCount, 11);
        }

        [Test]
        public async Task when_WriteToStreamAsync_then_stream_should_not_be_closed()
        {
            var typeToSerialize = "string to serialize";

            var memoryStream = new MemoryStream();
            JsonFormatter jsonFormatter = Create.JsonFormatter(new Version(5, 21, 0));

            await jsonFormatter.WriteToStreamAsync(typeToSerialize.GetType(), typeToSerialize, memoryStream, null, null);

            Assert.DoesNotThrow(() => memoryStream.ReadByte());
        }
    }
}
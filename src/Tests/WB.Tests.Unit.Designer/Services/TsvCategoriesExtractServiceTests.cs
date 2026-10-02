#nullable enable
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(TsvCategoriesExtractService))]
    public class TsvCategoriesExtractServiceTests
    {
        private static Stream ToStream(string text) => new MemoryStream(Encoding.UTF8.GetBytes(text));

        private static TsvCategoriesExtractService CreateService(Mock<ICategoriesVerifier>? verifier = null)
            => new TsvCategoriesExtractService((verifier ?? new Mock<ICategoriesVerifier>()).Object);

        [Test]
        public void Extract_should_read_rows_using_header_positions()
        {
            var rows = CreateService().Extract(ToStream("value\ttitle\n1\tOne\n2\tTwo\n"));

            rows.Select(x => (x.Id, x.Text)).Should().Equal(("1", "One"), ("2", "Two"));
        }

        [Test]
        public void Extract_should_support_old_header_names_and_parent()
        {
            var rows = CreateService().Extract(ToStream("id\ttext\tparentid\n1\tOne\t10\n"));

            rows.Should().ContainSingle();
            rows[0].ParentId.Should().Be("10");
        }

        [Test]
        public void Extract_should_skip_empty_rows()
        {
            var rows = CreateService().Extract(ToStream("value\ttitle\n1\tOne\n\t\n"));

            rows.Should().HaveCount(1);
        }

        [Test]
        public void Extract_when_header_is_unknown_should_throw_invalid_file()
        {
            var act = () => CreateService().Extract(ToStream("foo\tbar\n1\tOne\n"));

            act.Should().Throw<InvalidFileException>().Which.FoundErrors.Should().NotBeEmpty();
        }

        [Test]
        public void Extract_when_value_column_is_missing_should_throw_invalid_file()
        {
            var act = () => CreateService().Extract(ToStream("title\nOne\n"));

            act.Should().Throw<InvalidFileException>();
        }

        [Test]
        public void Extract_when_row_verification_fails_should_throw_with_errors()
        {
            var verifier = new Mock<ICategoriesVerifier>();
            verifier.Setup(x => x.Verify(It.IsAny<CategoriesRow>(), It.IsAny<CategoriesHeaderMap>()))
                .Returns(new ImportValidationError { Message = "bad row" });

            var act = () => CreateService(verifier).Extract(ToStream("value\ttitle\n1\tOne\n"));

            act.Should().Throw<InvalidFileException>()
                .Which.FoundErrors!.Select(x => x.Message).Should().Contain("bad row");
        }

        [Test]
        public void Extract_should_verify_all_rows_at_the_end()
        {
            var verifier = new Mock<ICategoriesVerifier>();

            CreateService(verifier).Extract(ToStream("value\ttitle\n1\tOne\n"));

            verifier.Verify(x => x.VerifyAll(It.IsAny<System.Collections.Generic.List<CategoriesRow>>(), It.IsAny<CategoriesHeaderMap>()), Times.Once);
        }

        [Test]
        public void GetAsFile_should_write_header_and_items()
        {
            var bytes = CreateService().GetAsFile(new System.Collections.Generic.List<CategoriesItem>
            {
                new CategoriesItem { Id = 1, Text = "One" }
            }, isCascading: false, hqImport: false);

            var lines = Encoding.UTF8.GetString(bytes).Split('\n').Select(x => x.TrimEnd('\r')).Where(x => x != "").ToArray();
            lines[0].Should().StartWith("value\ttitle");
            lines[1].Should().StartWith("1\tOne");
        }

        [Test]
        public void GetTemplateFile_for_cascading_should_include_parent_column()
        {
            var text = Encoding.UTF8.GetString(CreateService().GetTemplateFile(isCascading: true));

            text.Should().Contain("parentvalue");
        }

        [Test]
        public void GetTemplateFile_for_plain_should_not_include_parent_column()
        {
            var text = Encoding.UTF8.GetString(CreateService().GetTemplateFile(isCascading: false));

            text.Should().NotContain("parentvalue");
        }
    }
}


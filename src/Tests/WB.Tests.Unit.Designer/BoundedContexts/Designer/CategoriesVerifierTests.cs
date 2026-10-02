#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(CategoriesVerifier))]
    internal class CategoriesVerifierTests
    {
        private static readonly CategoriesHeaderMap Headers = new CategoriesHeaderMap
        {
            IdIndex = "A", ParentIdIndex = "B", TextIndex = "C", AttachmentNameIndex = "D"
        };

        private static CategoriesRow Row(string? id, string text, string? parentId = null, int rowId = 1)
            => new CategoriesRow { Id = id, Text = text, ParentId = parentId, RowId = rowId };

        private readonly CategoriesVerifier verifier = new CategoriesVerifier();

        [Test]
        public void Verify_should_throw_when_row_is_null()
        {
            Action act = () => verifier.Verify(null!, Headers);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Verify_should_throw_when_row_is_completely_empty()
        {
            Action act = () => verifier.Verify(Row(null, ""), Headers);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Verify_should_return_null_for_valid_row()
        {
            verifier.Verify(Row("1", "Yes"), Headers).Should().BeNull();
        }

        [Test]
        public void Verify_should_return_null_for_valid_row_with_parent()
        {
            verifier.Verify(Row("1", "Yes", "2"), Headers).Should().BeNull();
        }

        [Test]
        public void Verify_should_return_error_when_id_empty()
        {
            var error = verifier.Verify(Row("", "Yes", "2", 5), Headers);

            error.Should().NotBeNull();
            error!.ErrorAddress.Should().Contain("A").And.Contain("5");
        }

        [Test]
        public void Verify_should_return_error_when_id_is_not_integer()
        {
            var error = verifier.Verify(Row("abc", "Yes"), Headers);

            error.Should().NotBeNull();
            error!.ErrorAddress.Should().Contain("A");
        }

        [Test]
        public void Verify_should_return_error_when_parent_id_is_not_integer()
        {
            var error = verifier.Verify(Row("1", "Yes", "x"), Headers);

            error.Should().NotBeNull();
            error!.ErrorAddress.Should().Contain("B");
        }

        [Test]
        public void Verify_should_return_error_when_text_empty()
        {
            var error = verifier.Verify(Row("1", ""), Headers);

            error.Should().NotBeNull();
            error!.ErrorAddress.Should().Contain("C");
        }

        [Test]
        public void VerifyAll_should_throw_when_no_rows()
        {
            Action act = () => verifier.VerifyAll(new List<CategoriesRow>(), Headers);
            act.Should().Throw<InvalidFileException>();
        }

        [Test]
        public void VerifyAll_should_throw_when_single_row()
        {
            Action act = () => verifier.VerifyAll(new List<CategoriesRow> { Row("1", "a") }, Headers);
            act.Should().Throw<InvalidFileException>();
        }

        [Test]
        public void VerifyAll_should_not_throw_for_valid_rows()
        {
            var rows = new List<CategoriesRow> { Row("1", "a", rowId: 2), Row("2", "b", rowId: 3) };
            Action act = () => verifier.VerifyAll(rows, Headers);
            act.Should().NotThrow();
        }

        [Test]
        public void VerifyAll_should_throw_when_text_is_too_long()
        {
            var rows = new List<CategoriesRow>
            {
                Row("1", new string('a', 251), rowId: 2),
                Row("2", "b", rowId: 3)
            };

            var ex = Assert.Throws<InvalidFileException>(() => verifier.VerifyAll(rows, Headers))!;

            ex.FoundErrors.Should().HaveCount(1);
            ex.FoundErrors.Single().ErrorAddress.Should().Be("C2");
        }

        [Test]
        public void VerifyAll_should_throw_when_only_some_rows_have_parent_id()
        {
            var rows = new List<CategoriesRow>
            {
                Row("1", "a", "1", 2),
                Row("2", "b", null, 3)
            };

            Action act = () => verifier.VerifyAll(rows, Headers);
            act.Should().Throw<InvalidFileException>();
        }

        [Test]
        public void VerifyAll_should_throw_when_duplicated_by_id_and_parent()
        {
            var rows = new List<CategoriesRow>
            {
                Row("1", "a", rowId: 2),
                Row("1", "b", rowId: 3)
            };

            var ex = Assert.Throws<InvalidFileException>(() => verifier.VerifyAll(rows, Headers))!;

            ex.FoundErrors.Should().HaveCount(1);
            ex.FoundErrors.Single().ErrorAddress.Should().Be("A2");
        }

        [Test]
        public void VerifyAll_should_throw_when_duplicated_by_parent_and_text()
        {
            var rows = new List<CategoriesRow>
            {
                Row("1", "a", rowId: 2),
                Row("2", "a", rowId: 3)
            };

            var ex = Assert.Throws<InvalidFileException>(() => verifier.VerifyAll(rows, Headers))!;

            ex.FoundErrors.Should().HaveCount(1);
        }

        [Test]
        public void VerifyAll_should_allow_same_text_under_different_parents()
        {
            var rows = new List<CategoriesRow>
            {
                Row("1", "a", "10", 2),
                Row("2", "a", "20", 3)
            };

            Action act = () => verifier.VerifyAll(rows, Headers);
            act.Should().NotThrow();
        }
    }
}


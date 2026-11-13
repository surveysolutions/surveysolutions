using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Tester.Infrastructure.Internals.Storage;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestFixture]
    [TestOf(typeof(TesterQuestionnaireAssemblyAccessor))]
    public class TesterQuestionnaireAssemblyAccessorTests
    {
        [Test]
        public void when_storing_a_new_revision_should_keep_the_previous_assembly_available()
        {
            var files = new Dictionary<string, byte[]>();
            var fileSystem = new Mock<IFileSystemAccessor>();
            fileSystem.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystem.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(true);
            fileSystem.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), false)).Returns(Array.Empty<string>());
            fileSystem.Setup(x => x.IsFileExists(It.IsAny<string>())).Returns<string>(files.ContainsKey);
            fileSystem.Setup(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((path, bytes) => files[path] = bytes);
            fileSystem.Setup(x => x.ReadAllBytes(It.IsAny<string>(), null, null))
                .Returns<string, long?, long?>((path, _, _) => files[path]);

            var accessor = new TesterQuestionnaireAssemblyAccessor("assemblies", fileSystem.Object);
            var questionnaireId = Guid.NewGuid();
            accessor.StoreAssembly(questionnaireId, 1, Convert.ToBase64String(new byte[] { 1 }));
            accessor.StoreAssembly(questionnaireId, 2, Convert.ToBase64String(new byte[] { 2 }));

            accessor.GetAssemblyAsByteArray(questionnaireId, 1).Should().Equal(1);
            accessor.GetAssemblyAsByteArray(questionnaireId, 2).Should().Equal(2);
            accessor.IsQuestionnaireAssemblyExists(questionnaireId, 3).Should().BeFalse();
            fileSystem.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Tests.Abc;
using WB.UI.Designer.Areas.Pdf.Services;
using WB.UI.Designer.Areas.Pdf.Utils;

namespace WB.Tests.Unit.Designer.Services
{
    [TestOf(typeof(PdfQuery))]
    internal class PdfQueryTests
    {
        private Mock<IOptions<PdfSettings>> mockOptions;
        private PdfSettings pdfSettings;
        private PdfQuery pdfQuery;

        [SetUp]
        public void Setup()
        {
            this.pdfSettings = new PdfSettings { MaxPerUser = 3, WorkerCount = 1 };
            this.mockOptions = new Mock<IOptions<PdfSettings>>();
            this.mockOptions.Setup(o => o.Value).Returns(this.pdfSettings);
            this.pdfQuery = new PdfQuery(this.mockOptions.Object);
        }

        [Test]
        public void GetOrAdd_ShouldReturnExistingProgress_WhenKeyExists()
        {
            // Arrange
            var key = "test_key";
            var userId = Id.g1;
            var taskExecuted = false;
            
            Func<PdfGenerationProgress, Task> runGeneration = async progress => 
            {
                taskExecuted = true;
                await Task.Delay(10);
            };
            
            // Act - first call should add the job
            var progress1 = this.pdfQuery.GetOrAdd(userId, key, runGeneration);
            
            // Act - second call should return the same progress
            var progress2 = this.pdfQuery.GetOrAdd(userId, key, runGeneration);
            
            // Assert
            Assert.That(progress1, Is.Not.Null);
            Assert.That(progress2, Is.Not.Null);
            Assert.That(progress2, Is.SameAs(progress1), "Should return the same progress object for the same key");
        }

        [Test]
        public void GetOrAdd_ShouldReturnFailedProgress_WhenUserExceedsMaxJobs()
        {
            // Arrange
            var userId = Id.g1;
            Func<PdfGenerationProgress, Task> runGeneration = async progress => await Task.Delay(10);

            // Act - add maximum allowed jobs
            for (int i = 0; i < this.pdfSettings.MaxPerUser; i++)
            {
                this.pdfQuery.GetOrAdd(userId, $"key_{i}", runGeneration);
            }

            // Act - next attempt should throw an exception
            var ex = Assert.Catch<PdfLimitReachedException>(() => this.pdfQuery.GetOrAdd(userId, "one_too_many", runGeneration));

            // Assert
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.UserLimit, Is.EqualTo(this.pdfSettings.MaxPerUser));
        }

        [Test]
        public void GetOrNull_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "nonexistent_key";
            
            // Act
            var result = this.pdfQuery.GetOrNull(key);
            
            // Assert
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void GetOrNull_ShouldReturnProgress_WhenKeyExists()
        {
            // Arrange
            var key = "test_key";
            var userId = Id.g1;
            Func<PdfGenerationProgress, Task> runGeneration = async progress => await Task.Delay(10);
            
            // Act - add a job
            var addedProgress = this.pdfQuery.GetOrAdd(userId, key, runGeneration);
            
            // Act - get the progress
            var retrievedProgress = this.pdfQuery.GetOrNull(key);
            
            // Assert
            Assert.That(retrievedProgress, Is.Not.Null);
            Assert.That(retrievedProgress, Is.SameAs(addedProgress));
        }

        [Test]
        public async Task WorkerLoop_ShouldExecuteJob()
        {
            // Arrange
            var key = "test_key";
            var userId = Id.g1;
            var taskExecuted = false;
            
            Func<PdfGenerationProgress, Task> runGeneration = async progress => 
            {
                taskExecuted = true;
                await Task.Delay(10);
                progress.Finish();
            };
            
            // Act
            var progress = this.pdfQuery.GetOrAdd(userId, key, runGeneration);
            
            // Allow worker thread to process the job
            await Task.Delay(100);
            
            // Assert
            Assert.That(taskExecuted, Is.True, "Job should have been executed");
            Assert.That(progress.IsFinished, Is.True, "Progress should be marked as finished");
        }

        [Test]
        public async Task WorkerLoop_ShouldMarkProgressAsFailed_WhenJobThrowsException()
        {
            // Arrange
            var key = "test_key";
            var userId = Id.g1;
            
            Func<PdfGenerationProgress, Task> runGeneration = async progress => 
            {
                await Task.Delay(10);
                throw new Exception("Test exception");
            };
            
            // Act
            var progress = this.pdfQuery.GetOrAdd(userId, key, runGeneration);
            
            // Allow worker thread to process the job
            await Task.Delay(100);
            
            // Assert
            Assert.That(progress.IsFailed, Is.True, "Progress should be marked as failed");
        }
    }
}

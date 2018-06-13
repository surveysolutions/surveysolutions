using System;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Unit.Applications.WebTester.Services
{
    [TestFixture]
    public class MediaStorageTests
    {
        [Test]
        public void should_add_item_to_cache()
        {
            string fileName = Id.g1.ToString();
            
            var subj = Create.Storage.MediaStorage();

            subj.Store(new MultimediaFile
            {
                Filename = fileName
            }, fileName, Id.g1);

            Assert.That(subj.Get(fileName, Id.g1).Filename, Is.EqualTo(fileName));
        }

        [Test]
        public void should_add_multiple_items_per_interview_to_cache()
        {
            var subj = Create.Storage.MediaStorage();

            var interviewA = Id.gA;
            var interviewB = Id.gB;

            string fileA = Id.g1.ToString(),
                fileA1 = Id.g1.ToString(),
                fileB = Id.g1.ToString();

            StoreFile(subj, fileA,  interviewA);
            StoreFile(subj, fileA1, interviewA);
            StoreFile(subj, fileB,  interviewB);

            Assert.That(subj.Get(fileA1, interviewA).Filename, Is.EqualTo(fileA1));
        }

        [Test]
        public void should_remove_items_from_cache_by_area()
        {
            InMemoryCacheStorage<MultimediaFile, string> subj = Create.Storage.MediaStorage();

            var interviewA = Id.gA;
            var interviewB = Id.gB;

            string fileA = Id.g1.ToString(),
                fileA1 = Id.g1.ToString(),
                fileB = Id.g1.ToString();

            StoreFile(subj, fileA,  interviewA);
            StoreFile(subj, fileA1, interviewA);
            StoreFile(subj, fileB,  interviewB);

            subj.RemoveArea(interviewA);
                
            Assert.That(subj.Get(fileA1, interviewA), Is.Null);
            Assert.That(subj.Get(fileA, interviewA), Is.Null);

            subj.RemoveArea(Id.gB);
            
            Assert.That(subj.Get(fileB, interviewB), Is.Null);
        }

        private void StoreFile(InMemoryCacheStorage<MultimediaFile, string> subj, string filename, Guid interviewId)
        {
            subj.Store(new MultimediaFile
            {
                Filename = filename
            }, filename, interviewId);
        }
    }
}

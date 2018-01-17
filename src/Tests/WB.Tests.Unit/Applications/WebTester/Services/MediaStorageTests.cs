using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Unit.Applications.WebTester.Services
{
    [TestFixture]
    public class MediaStorageTests
    {
        [Test]
        public void should_add_item_to_cache()
        {
            IObservable<Guid> evictionNotification = new Subject<Guid>();
            var subj = new MediaStorage(evictionNotification);

            subj.Store(Id.gA, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g1.ToString()
            });

            Assert.That(subj.Get(Id.gA, Id.g1.ToString()).Filename, Is.EqualTo(Id.g1.ToString()));
        }

        [Test]
        public void should_add_multiple_items_per_interview_to_cache()
        {
            IObservable<Guid> evictionNotification = new Subject<Guid>();
            var subj = new MediaStorage(evictionNotification);

            subj.Store(Id.gA, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g1.ToString()
            });

            subj.Store(Id.gA, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g2.ToString()
            });

            subj.Store(Id.gB, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g2.ToString()
            });

            Assert.That(subj.Get(Id.gA, Id.g2.ToString()).Filename, Is.EqualTo(Id.g2.ToString()));
        }

        [Test]
        public void should_remove_items_from_cache_on_evict()
        {
            var evictionNotification = new Subject<Guid>();
            var subj = new MediaStorage(evictionNotification);

            subj.Store(Id.gA, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g1.ToString()
            });

            subj.Store(Id.gA, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g2.ToString()
            });

            subj.Store(Id.gB, new UI.WebTester.Services.MultimediaFile
            {
                Filename = Id.g2.ToString()
            });

            evictionNotification.OnNext(Id.gA);
                
            Assert.That(subj.Get(Id.gA, Id.g2.ToString()), Is.Null);

            evictionNotification.OnNext(Id.gB);

            Assert.That(subj.Get(Id.gB, Id.g2.ToString()), Is.Null);
        }
    }
}

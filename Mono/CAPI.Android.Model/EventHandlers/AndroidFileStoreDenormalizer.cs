using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.File;
using Main.Core.Services;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class AndroidFileStoreDenormalizer : FileStoreDenormalizer
    {
        public AndroidFileStoreDenormalizer(IReadSideRepositoryWriter<FileDescription> attachments, IFileStorageService storage) : base(attachments, storage)
        {
        }

        public override void PostSaveHandler(IPublishedEvent<FileUploaded> evnt)
        {
            
        }
    }
}
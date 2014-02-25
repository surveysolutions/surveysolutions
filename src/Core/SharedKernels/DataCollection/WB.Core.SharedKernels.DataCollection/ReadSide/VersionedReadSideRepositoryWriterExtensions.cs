﻿using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public static class VersionedReadSideRepositoryWriterExtensions
    {
        public static T GetById<T>(this IVersionedReadSideRepositoryWriter<T> writer, Guid id, long version) where T : class, IVersionedView
        {
            return writer.GetById(id.FormatGuid(), version);
        }

        public static void Remove<T>(this IVersionedReadSideRepositoryWriter<T> writer, Guid id, long version) where T : class, IVersionedView
        {
            writer.Remove(id.FormatGuid(), version);
        }

        public static void Store<T>(this IVersionedReadSideRepositoryWriter<T> writer, T view, Guid id) where T : class, IVersionedView
        {
            writer.Store(view, id.FormatGuid());
        }
    }
}
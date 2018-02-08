﻿using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class ModeOfDataCollectionItem
    {
        public ModeOfDataCollection Code { get; set; }
        public string Title { get; set; }
    }

    public static class ModeOfDataCollectionProvider
    {
        public static List<ModeOfDataCollectionItem> GetModeOfDataCollectionItems()
        {
            var codes = GetModeOfDataCollectionCodes();
            return codes.Select(code => new ModeOfDataCollectionItem()
            {
                Code = code,
                Title = GetModeOfDataCollectionTitleByCode(code)
            }).ToList();
        }

        public static string GetModeOfDataCollectionTitleByCode(ModeOfDataCollection code)
        {
            return Resources.ModeOfDataCollection.ResourceManager.GetString(code.ToString());
        }

        public static List<ModeOfDataCollection> GetModeOfDataCollectionCodes()
        {
            return new List<ModeOfDataCollection>()
            {
                ModeOfDataCollection.Capi,
                ModeOfDataCollection.Cati,
                ModeOfDataCollection.FaceToFace,
                ModeOfDataCollection.Mail,
                ModeOfDataCollection.FocusGroup,
                ModeOfDataCollection.Internet,
                ModeOfDataCollection.Other
            };
        }
    }
}
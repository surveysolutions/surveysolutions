﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.TabletInformation;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_content_file : FileBasedTabletInformationServiceTestContext
    {
        Establish context = () =>
        {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService();
        };

        Because of = () => returnedPackagePath = fileBasedTabletInformationService.GetFullPathToContentFile(packageName);

        It should_result_ends_with_packageName = () => returnedPackagePath.ShouldEndWith(packageName);

        private static FileBasedTabletInformationService fileBasedTabletInformationService;
        
        private static string packageName = "packName";
        private static string returnedPackagePath;
    }
}

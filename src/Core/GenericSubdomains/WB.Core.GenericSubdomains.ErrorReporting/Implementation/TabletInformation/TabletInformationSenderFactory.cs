﻿using WB.Core.GenericSubdomain.Rest;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation
{
    internal class TabletInformationSenderFactory : ITabletInformationSenderFactory
    {
        private readonly INetworkService networkService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;
        private readonly IJsonUtils jsonUtils;
        private readonly IRestServiceWrapperFactory restServiceWrapperFactory;

        public TabletInformationSenderFactory(INetworkService networkService, IFileSystemAccessor fileSystemAccessor,
            ICapiInformationService capiInformationService,  IJsonUtils jsonUtils, IRestServiceWrapperFactory restServiceWrapperFactory)
        {
            this.networkService = networkService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.capiInformationService = capiInformationService;
            this.jsonUtils = jsonUtils;
            this.restServiceWrapperFactory = restServiceWrapperFactory;
        }

        public ITabletInformationSender CreateTabletInformationSender(string syncAddressPoint, string registrationKeyName, string androidId)
        {
            return new TabletInformationSender(this.capiInformationService, this.networkService, this.fileSystemAccessor, this.jsonUtils, syncAddressPoint,
                registrationKeyName, androidId, restServiceWrapperFactory);
        }
    }
}

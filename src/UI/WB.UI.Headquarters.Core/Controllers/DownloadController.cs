﻿using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IQRCodeHelper qRCodeHelper;
        private readonly IClientApkProvider clientApkProvider;

        public DownloadController(IQRCodeHelper qRCodeHelper, IClientApkProvider clientApkProvider)
        {
            this.qRCodeHelper = qRCodeHelper;
            this.clientApkProvider = clientApkProvider;
        }

        public IActionResult Index()
        {
            var smallApkUrl = Url.Action("GetLatestVersion", "DownloadApi");
            var fullApkUrl = Url.Action("GetLatestExtendedVersion", "DownloadApi");

            return View(new
            {
                SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration(),
                SmallApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(smallApkUrl), 250, 250),
                FullApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(fullApkUrl), 250, 250),
                SmallApkUrl = smallApkUrl,
                FullApkUrl = fullApkUrl,
                SmallApkVersion = clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerFileName),
                FullApkVersion = clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerExtendedFileName),
            });
        }

        public IActionResult Supervisor()
        {
            var apkUrl = Url.Action("GetLatestSupervisor", "DownloadApi");
            return View(new
            {
                SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration(),
                ApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(apkUrl), 250, 250),
                ApkUrl = apkUrl,
                SupervisorVersion = clientApkProvider.GetApplicationVersionString(ClientApkInfo.SupervisorFileName)
            });
        }
    }
}

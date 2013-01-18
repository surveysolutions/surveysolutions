// -----------------------------------------------------------------------
// <copyright file="SuccessMarker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Web.Hosting;
using Main.Core;
using Ninject;

namespace Questionnaire.Core.Web.Helpers
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class SuccessMarker
    {
        /// <summary>
        /// Gets the success file path.
        /// </summary>
        private static string SuccessFilePath
        {
            get { return HostingEnvironment.MapPath("/App_Data/" + SuccessMarkerFileName); }
        }

        /// <summary>
        /// The success marker.
        /// </summary>
        private const string SuccessMarkerFileName = "success.marker";

        /// <summary>
        /// The stop.
        /// </summary>
        public static void Stop()
        {
            /*
            File.CreateText(SuccessMarker.SuccessFilePath);
            // Get file info
            FileInfo myFile = new FileInfo(SuccessMarker.SuccessFilePath);

            // Remove the hidden attribute of the file
            myFile.Attributes |= FileAttributes.Hidden;*/
        }

        public static void Start(IKernel kernel)
        {
           /* if (!File.Exists(SuccessMarker.SuccessFilePath))
            {*/
                NcqrsInit.RebuildReadLayer(kernel);
         /*   }
            else
                File.Delete(SuccessMarker.SuccessFilePath);*/
        }
    }
}

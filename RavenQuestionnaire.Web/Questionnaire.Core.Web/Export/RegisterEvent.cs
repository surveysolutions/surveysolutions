// -----------------------------------------------------------------------
// <copyright file="RegisterEvent.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Export
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web;

    using Main.Core.Commands.Synchronization;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Newtonsoft.Json;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRegisterEvent
    {
        /// <summary>
        /// Register method
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        void Register(HttpPostedFileBase uploadFile);
    }

    /// <summary>
    /// RegisterEvent for new device
    /// </summary>
    public class RegisterEvent : IRegisterEvent
    {
        /// <summary>
        /// Create new event with publicKey of device
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        public void Register(HttpPostedFileBase uploadFile)
        {
            uploadFile.InputStream.Position = 0;
            string data = string.Empty;
            byte[] buffer = new byte[uploadFile.InputStream.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = uploadFile.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                {   
                    ms.Write(buffer, 0, read);
                }

                data = Encoding.Default.GetString(ms.ToArray());
            }

            var result = JsonConvert.DeserializeObject<RegisterData>(data);
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new RegisterNewDeviceCommand(result.Description, result.SecretKey, result.TabletId));
        }
    }
}

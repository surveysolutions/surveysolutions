using System;
using System.IO;
using Android.Content;
using CAPI.Android.Core.Model.ModelUtils;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ProjectionStorage
{
    public class InternalProjectionStorage : IProjectionStorage
    {
      //  private Context context;
        public InternalProjectionStorage(/*Context context*/)
        {
         //   this.context = context;

        }

        #region Implementation of IProjectionStorage

        protected string GetFileName(Guid publicKey)
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                           publicKey.ToString());
        }

        public void SaveOrUpdateProjection<T>(T projection, Guid publicKey) where T : class 
        {
            using (var fs = File.Open(GetFileName(publicKey),FileMode.OpenOrCreate))
            {
                var bytes = GetBytes(JsonUtils.GetJsonData(projection));
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public T RestoreProjection<T>(Guid publicKey) where T : class 
        {
            var filePath = GetFileName(publicKey);
            if (!File.Exists(filePath))
                return null;
            byte[] cachedBytes = System.IO.File.ReadAllBytes(filePath);
            return JsonUtils.GetObject<T>(GetString(cachedBytes));
            /*    using (var fs = context.OpenFileInput(publicKey.ToString()))
                {

                    var bytes = new List<byte>();
                    var chunk = new byte[1024];
                    var step = 0;
                    while(fs.Read(chunk, step, chunk.Length) > 0)
                    {
                        bytes.AddRange(chunk);
                        step = step + chunk.Length;
                        chunk = new byte[1024];
                    }

                    return GetObject(GetString(bytes.ToArray()));
                }*/
        }

        public void ClearStorage()
        {
           // throw new NotImplementedException();
        }

        public void ClearProjection(Guid prjectionKey)
        {
            File.Delete(GetFileName(prjectionKey));
        }

        #endregion
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        private string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
      
    }
}

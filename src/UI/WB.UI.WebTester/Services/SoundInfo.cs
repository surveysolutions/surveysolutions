using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WB.UI.WebTester.Services
{
    public static class SoundInfo
    {
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        public static int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString($"open \"{fileName}\" type waveaudio alias wave", null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            if(int.TryParse(lengthBuf.ToString(), out var length)) return length;

            return 0;
        }

        public static TimeSpan GetSoundLength(byte[] data)
        {
            var temp = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(temp, data);
                var legth = GetSoundLength(temp);
                return TimeSpan.FromMilliseconds(legth);
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}
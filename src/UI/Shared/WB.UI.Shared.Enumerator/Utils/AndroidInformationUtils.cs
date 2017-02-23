using Android.App;
using Android.Content;
using Android.OS;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class AndroidInformationUtils
    {
        public static string GetRAMInformation()
        {
            ActivityManager activityManager = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            if (activityManager == null)
                return "UNKNOWN";

            ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(mi);
            return $"{FileSizeUtils.SizeSuffix(mi.TotalMem)} total, avaliable {(int)(((double)(100 * mi.AvailMem)) / mi.TotalMem)}% ({FileSizeUtils.SizeSuffix(mi.AvailMem)})";
        }

        public static string GetDiskInformation()
        {
            string path = global::Android.OS.Environment.DataDirectory.Path;
            StatFs stat = new StatFs(path);
            long blockSize = stat.BlockSizeLong;
            long availableBlocks = stat.AvailableBlocksLong;
            long totalBlocks = stat.BlockCountLong;
            var availableInternalMemorySize = (availableBlocks * blockSize);
            var totalInternalMemorySize = totalBlocks * blockSize;
            return $"{FileSizeUtils.SizeSuffix(totalInternalMemorySize)} total, avaliable {(int)(((double)(100 * availableInternalMemorySize)) / totalInternalMemorySize)}% ({FileSizeUtils.SizeSuffix(availableInternalMemorySize)})";
        }
    }
}
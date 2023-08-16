using Android.Content;
using Android.OS;
using Java.Interop;
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
            return $"{FileSizeUtils.SizeSuffix(mi.TotalMem)} total, avaliable {mi.AvailMem.PercentOf(mi.TotalMem)}% ({FileSizeUtils.SizeSuffix(mi.AvailMem)})";
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
            return $"{FileSizeUtils.SizeSuffix(totalInternalMemorySize)} total, avaliable {availableInternalMemorySize.PercentOf(totalInternalMemorySize)}% ({FileSizeUtils.SizeSuffix(availableInternalMemorySize)})";
        }

        public static List<GroupedPeer> GetPeers()
        {
            var groupBy = Java.Interop.JniRuntime.CurrentRuntime.ValueManager.GetSurfacedPeers()
                .Select(s => s.SurfacedPeer.TryGetTarget(out var target)
                    ? target
                    : null).Where(v => v != null)
                .GroupBy(k => k.GetType().Name)
                .Select(k => new GroupedPeer{ Type = k.Key, Count = k.Count(), Instances = k.ToList() })
                .OrderByDescending(k => k.Count)
                .ToList();

            return groupBy;
        }

        public class GroupedPeer
        {
            public string Type { get; set; }
            public int Count { get; set; }
            public List<IJavaPeerable> Instances { set; get; } = new List<IJavaPeerable>();
        }
    }
}

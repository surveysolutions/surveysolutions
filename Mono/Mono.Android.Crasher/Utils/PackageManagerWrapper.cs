using Android.Content;
using Android.Content.PM;
using Android.Util;
using Java.Lang;

namespace Mono.Android.Crasher.Utils
{
    /// <summary>
    ///  Responsible for wrapping calls to <see cref="PackageManager"/> to ensure that they always complete without throwing <see cref="RuntimeException"/>.
    /// </summary>
    class PackageManagerWrapper
    {
        private readonly Context _context;

        public PackageManagerWrapper(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// With this method you can check application permissions
        /// </summary>
        /// <param name="permission">Manifest.Permission to check whether it has been granted.</param>
        /// <returns>true if the permission has been granted to the app, false if it hasn't been granted or the <see cref="PackageManager"/> could not be contacted.</returns>
        public bool HasPermission(string permission)
        {
            var pm = _context.PackageManager;
            if (pm == null)
            {
                return false;
            }

            try
            {
                return pm.CheckPermission(permission, _context.PackageName) == Permission.Granted;
            }
            catch (RuntimeException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Get <see cref="PackageInfo"/> from <see cref="PackageManager"/>
        /// </summary>
        /// <returns><see cref="PackageInfo"/> for the current application or null if the <see cref="PackageManager"/> could not be contacted.</returns>
        public PackageInfo GetPackageInfo()
        {
            var pm = _context.PackageManager;
            if (pm == null)
            {
                return null;
            }

            try
            {
                return pm.GetPackageInfo(_context.PackageName, 0);
            }
            catch (PackageManager.NameNotFoundException e)
            {
                Log.Debug(Constants.LOG_TAG, "Failed to find PackageInfo for current App : " + _context.PackageName);
                return null;
            }
            catch (RuntimeException e)
            {
                return null;
            }
        }
    }
}
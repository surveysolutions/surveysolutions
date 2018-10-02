using System.Threading.Tasks;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorSplashScreenAppCompatActivity<TSetup, TApplication> : MvxSplashScreenAppCompatActivity<TSetup, TApplication>
        where TSetup : MvxAppCompatSetup<TApplication>, new()
        where TApplication : class, IMvxApplication, new()
    {
        protected EnumeratorSplashScreenAppCompatActivity(int resourceId) : base(resourceId)
        {
        }

        public override Task InitializationComplete()
        {
            this.EncryptApplication();
            return base.InitializationComplete();
        }

        protected virtual void EncryptApplication() 
        {
            var settings = ServiceLocator.Current.GetInstance<IEnumeratorSettings>();
            if (!settings.Encrypted)
            {
                var encryptionProvider = ServiceLocator.Current.GetInstance<IEncryptionService>();

                encryptionProvider.GenerateKeys();

                this.EncryptDatabase(encryptionProvider);

                //settings.SetEncrypted(true);
            }
        }

        private void EncryptDatabase(IEncryptionService encryptionProvider)
        {
            var encryptedText = encryptionProvider.Encrypt("Привет мир!");  
            var decryptedText = encryptionProvider.Decrypt(encryptedText);
        }
    }
}

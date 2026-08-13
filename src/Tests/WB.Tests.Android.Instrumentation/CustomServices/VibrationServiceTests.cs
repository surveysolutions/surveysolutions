using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.CustomServices;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    /// <summary>
    /// On-device tests for <see cref="VibrationService"/>. These tests run inside an
    /// Android instrumentation test APK (see <see cref="NUnitInstrumentation"/>) so that
    /// real Android types (<see cref="global::Android.OS.Vibrator"/>, <see cref="global::Android.App.Activity"/>)
    /// are available, unlike in the portable WB.Tests.Unit project.
    /// </summary>
    [TestFixture]
    public class VibrationServiceTests
    {
        private class StubCurrentTopActivity : IMvxAndroidCurrentTopActivity
        {
            public global::Android.App.Activity Activity { get; set; }
        }

        private StubCurrentTopActivity stubCurrentTopActivity;

        [SetUp]
        public void SetUp()
        {
            // Every test gets a brand-new IoC container so tests don't leak state between each other.
            MvxIoCProvider.Initialize();

            this.stubCurrentTopActivity = new StubCurrentTopActivity { Activity = null };
            Mvx.IoCProvider.RegisterSingleton<IMvxAndroidCurrentTopActivity>(this.stubCurrentTopActivity);
        }

        [Test]
        public void Vibrate_WhenNoCurrentActivity_DoesNotThrow()
        {
            var vibrationService = new VibrationService();

            Assert.DoesNotThrow(() => vibrationService.Vibrate());
        }

        [Test]
        public void Vibrate_WhenDisabledAndNoCurrentActivity_DoesNotThrow()
        {
            var vibrationService = new VibrationService();

            vibrationService.Disable();

            Assert.DoesNotThrow(() => vibrationService.Vibrate());
        }

        [Test]
        public void Enable_AfterDisable_AllowsVibrateToBeCalledAgainWithoutThrowing()
        {
            var vibrationService = new VibrationService();

            vibrationService.Disable();
            vibrationService.Enable();

            Assert.DoesNotThrow(() => vibrationService.Vibrate());
        }

        [Test]
        public void Disable_CanBeCalledMultipleTimes_DoesNotThrow()
        {
            var vibrationService = new VibrationService();

            Assert.DoesNotThrow(() =>
            {
                vibrationService.Disable();
                vibrationService.Disable();
            });
        }

        [Test]
        public void Enable_CanBeCalledMultipleTimes_DoesNotThrow()
        {
            var vibrationService = new VibrationService();

            Assert.DoesNotThrow(() =>
            {
                vibrationService.Enable();
                vibrationService.Enable();
            });
        }
    }
}


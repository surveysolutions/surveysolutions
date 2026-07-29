using Android.App;
using Android.Content;
using MvvmCross.Platforms.Android;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services.Internals;
using Xamarin.Essentials;

namespace WB.Tests.Android.Instrumentation;

[TestFixture]
[NonParallelizable]
public class QRBarcodeScanServiceTests
{
	[TearDown]
	public void TearDown() => QRBarcodeScanService.SetResult(null);

	[Test]
	public async Task ScanAsync_when_camera_permission_is_granted_launches_scanner_and_returns_its_result()
	{
		var activity = new RecordingActivity();
		var permissions = new RecordingPermissionsService();
		var service = new QRBarcodeScanService(permissions, new CurrentTopActivity(activity));

		var scan = service.ScanAsync();

		Assert.Multiple(() =>
		{
			Assert.That(permissions.CameraPermissionWasRequested, Is.True);
			Assert.That(activity.StartedIntent, Is.Not.Null);
			Assert.That(activity.StartedIntent.Component.ClassName, Does.EndWith(nameof(BarcodeScannerActivity)));
			Assert.That(scan.IsCompleted, Is.False);
		});

		var expected = new QRBarcodeScanResult { Code = "qr-code", RawBytes = [1, 2, 3] };
		QRBarcodeScanService.SetResult(expected);

		Assert.That(await scan, Is.SameAs(expected));
	}

	[Test]
	public void ScanAsync_when_camera_permission_is_denied_does_not_launch_scanner()
	{
		var activity = new RecordingActivity();
		var service = new QRBarcodeScanService(
			new ThrowingPermissionsService(),
			new CurrentTopActivity(activity));

		Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ScanAsync());
		Assert.That(activity.StartedIntent, Is.Null);
	}

	private sealed class CurrentTopActivity(Activity activity) : IMvxAndroidCurrentTopActivity
	{
		public Activity Activity { get; } = activity;
	}

	private sealed class RecordingPermissionsService : IPermissionsService
	{
		public bool CameraPermissionWasRequested { get; private set; }

		public Task AssureHasPermissionOrThrow<T>() where T : Permissions.BasePermission, new()
		{
			CameraPermissionWasRequested = typeof(T) == typeof(Permissions.Camera);
			return Task.CompletedTask;
		}

		public Task EnsureHasPermissionToInstallFromUnknownSourcesAsync() => Task.CompletedTask;
		public Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T : Permissions.BasePermission, new() => Task.FromResult(PermissionStatus.Granted);
		public Task AssureHasExternalStoragePermissionOrThrow() => Task.CompletedTask;
		public Task AssureHasBluetoothPermissionOrThrow() => Task.CompletedTask;
		public Task AssureHasNearbyWifiDevicesPermissionOrThrow() => Task.CompletedTask;
	}

	private sealed class ThrowingPermissionsService : IPermissionsService
	{
		public Task AssureHasPermissionOrThrow<T>() where T : Permissions.BasePermission, new() => Task.FromException(new InvalidOperationException("Camera permission denied."));

		public Task EnsureHasPermissionToInstallFromUnknownSourcesAsync() => Task.CompletedTask;
		public Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T : Permissions.BasePermission, new() => Task.FromResult(PermissionStatus.Denied);
		public Task AssureHasExternalStoragePermissionOrThrow() => Task.CompletedTask;
		public Task AssureHasBluetoothPermissionOrThrow() => Task.CompletedTask;
		public Task AssureHasNearbyWifiDevicesPermissionOrThrow() => Task.CompletedTask;
	}
}

public sealed class RecordingActivity : Activity
{
	public Intent StartedIntent { get; private set; }

	public override void StartActivity(Intent intent) => StartedIntent = intent;
}


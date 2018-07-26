using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public enum ConnectionStatusCode
    {
        ApiConnectionFailedAlreadyInUse = 8050,
        MissingPermissionAccessCoarseLocation = 8034,
        MissingPermissionAccessWifiState = 8032,
        MissingPermissionBluetooth = 8030,
        MissingPermissionBluetoothAdmin = 8031,
        MissingPermissionChangeWifiState = 8033,
        MissingPermissionRecordAudio = 8035,
        MissingSettingLocationMustBeOn = 8025,
        StatusAlreadyAdvertising = 8001,
        StatusAlreadyConnectedToEndpoint = 8003,
        StatusAlreadyDiscovering = 8002,
        StatusAlreadyHaveActiveStrategy = 8008,
        StatusBluetoothError = 8007,
        StatusConnectionRejected = 8004,
        StatusEndpointIoError = 8012,
        StatusEndpointUnknown = 8011,
        StatusError = 13,
        [Obsolete("deprecated")]
        StatusNetworkNotConnected = 8000,
        StatusNotConnectedToEndpoint = 8005,
        StatusOk = 0,
        StatusOutOfOrderApiCall = 8009,
        StatusPayloadIoError = 8013,
        Unknown = 9999
    }
}

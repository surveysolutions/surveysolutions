import { getSilentlyText } from './apiService';

export function getOnlineVersion() {
    return getSilentlyText('/.version');
}

export function getCurrentVersion() {
    const versionMetaTag = document.querySelector('meta[name="version"]');
    const siteVersion = versionMetaTag
        ? versionMetaTag.getAttribute('content')
        : 'unknown';
    return siteVersion;
}

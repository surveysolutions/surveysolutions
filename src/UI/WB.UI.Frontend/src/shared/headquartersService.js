

export function getCurrentVersion() {
    const versionSpan = document.getElementById('appVersion');
    const siteVersion = versionSpan
        ? versionSpan.textContent
        : 'unknown';
    return siteVersion;
}

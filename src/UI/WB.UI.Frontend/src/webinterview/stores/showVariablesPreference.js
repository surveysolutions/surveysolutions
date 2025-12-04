import browserLocalStore from '../../shared/localStorage'

const STORAGE_KEY = 'webinterview_devMode'

export function getDevModeSetting() {
    const savedValue = browserLocalStore.getItem(STORAGE_KEY)
    return savedValue === 'true'
}

export function setDevModeSetting(value) {
    browserLocalStore.setItem(STORAGE_KEY, value.toString())
}

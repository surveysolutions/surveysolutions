import browserLocalStore from '../../shared/localStorage'

const STORAGE_KEY = 'webinterview_showVariables'

export function getShowVariables() {
    const savedValue = browserLocalStore.getItem(STORAGE_KEY)
    return savedValue === 'true'
}

export function setShowVariables(value) {
    browserLocalStore.setItem(STORAGE_KEY, value.toString())
}

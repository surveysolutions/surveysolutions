import browserLocalStore from '../../shared/localStorage'

const STORAGE_KEY = 'webinterview_showVariables'

// Create a single instance to avoid unnecessary object creation
const localStore = new browserLocalStore()

export function getShowVariables() {
    const savedValue = localStore.getItem(STORAGE_KEY)
    return savedValue === 'true'
}

export function setShowVariables(value) {
    localStore.setItem(STORAGE_KEY, value.toString())
}

class BrowserLocalStore {
    constructor() {
        this.store = window.localStorage
    }

    setItem(key, value) {
        if (this.store) {
            try {
                localStorage.setItem(key, value)
            }
            catch {
                /* empty */
            }
        }
    }

    getItem(key) {
        if (this.store) {
            try {
                var result = localStorage.getItem(key)
            }
            catch {
                return null
            }
            return result
        }
    }

    remove(key) {
        if (this.store) {
            try {
                localStorage.removeItem(key)
            }
            catch {
                /* empty */
            }
        }
    }
}

export default new BrowserLocalStore()
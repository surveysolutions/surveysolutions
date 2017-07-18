export default (Vue, options) => {

    const config = JSON.parse(window.vueApp.getAttribute('configuration'))

    Object.defineProperty(Vue.prototype, '$config', {
        get() { return config; }
    })

}
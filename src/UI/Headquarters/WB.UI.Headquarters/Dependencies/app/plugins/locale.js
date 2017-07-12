export default (Vue, options) => {

    Object.defineProperty(Vue.prototype, '$t', {
        get() {

            return (arg) => {                
                if (this.$config) {
                    var resource = this.$config.resource;
                    
                    return this.$config.resource[arg] || arg;
                }
                return arg;
            }
        }});
}
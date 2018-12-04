var ClassificationsConfig = {};
ClassificationsConfig.install = function(Vue, options) {
    Object.defineProperty(Vue, '$config',
        {
            get: function() {
                return options;
            }
        });

    Object.defineProperty(Vue.prototype, '$config',
        {
            get: function() {
                return options;
            }
        });
};

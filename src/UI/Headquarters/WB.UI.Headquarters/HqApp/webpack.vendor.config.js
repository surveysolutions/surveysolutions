const webpack = require('webpack')
const merge = require('webpack-merge');

const devMode = process.env.NODE_ENV != 'production';

const packageName = "shared_vendor";

const shared = require("./.build/webpack.shared.vendor");

module.exports = merge(shared(packageName, devMode), {
    entry: {
        [packageName]: [                       
            "autonumeric/dist/autonumeric.min",
            "axios",
            "babel-polyfill",
            "bootbox",            
            "bootstrap/dist/js/bootstrap.js",
            "flatpickr",
            "flatpickr/dist/l10n",
            "lodash",
            "jquery-mask-plugin",
            "jquery",
            "moment",
            "scriptjs",
            "signalr",
            "smoothscroll-polyfill",
            "toastr",
            "i18next",
            "vue-router",
            "vue",
            "vuex-router-sync",
            "vuex"
        ]
    }
})

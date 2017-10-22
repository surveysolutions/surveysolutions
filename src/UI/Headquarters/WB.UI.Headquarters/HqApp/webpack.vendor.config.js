const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"

const merge = require('webpack-merge');

const devMode = process.env.NODE_ENV != 'production';

const packageName = "shared_vendor";

// console.log("Building HQ UI vendor libs");

const shared = require("./.build/webpack.shared.vendor");

module.exports = merge(shared(packageName), {
    entry: {
        [packageName]: [
            "autonumeric",
            "axios",
            "bootbox",
            "bootstrap/dist/js/bootstrap.js",
            "date-fns",
            "flatpickr",
            "jquery-mask-plugin",
            "jquery",
            "moment",
            "scriptjs",
            "signalr",
            "toastr",
            "vue-i18n",
            "vue-router",
            "vue",
            "vuex-router-sync",
            "vuex"
        ]
    }
})
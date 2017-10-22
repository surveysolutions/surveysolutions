const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"

const merge = require('webpack-merge');

const devMode = process.env.NODE_ENV != 'production';

const packageName = "shared_vendor";

const shared = require("./.build/webpack.shared.vendor");

module.exports = merge(shared(packageName, devMode), {
    entry: {
        [packageName]: [
            "autonumeric",
            "axios",
            "bootbox",
            "babel-polyfill",
            "bootstrap/dist/js/bootstrap.js",
            "date-fns",
            "flatpickr",
            "lodash",
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
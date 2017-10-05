import { browserLanguage } from "shared/helpers"

global.jQuery = require("jquery")

import * as moment from 'moment'
moment.locale(browserLanguage);

import Vue from "vue"
import config from "shared/config"
Vue.use(config)
import VueI18n from "./locale"

import * as poly from "smoothscroll-polyfill"
poly.polyfill()

Vue.use(VueI18n, {
    defaultNS: 'WebInterviewUI',
    ns: ['WebInterviewUI', 'Common'],
    nsSeparator: '.',
    resources: {
        'en': Vue.$config.locale
    }
})
import { browserLanguage } from "shared/helpers"

global.jQuery = require("jquery")

import * as moment from 'moment'
moment.locale(browserLanguage);
import * as poly from "smoothscroll-polyfill"
poly.polyfill()
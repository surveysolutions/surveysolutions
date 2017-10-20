import { browserLanguage } from "shared/helpers"
import "bootstrap/js/dropdown";

global.jQuery = require("jquery")

import moment from 'moment'
moment.locale(browserLanguage);
import * as poly from "smoothscroll-polyfill"
poly.polyfill()
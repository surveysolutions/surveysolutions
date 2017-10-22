import { browserLanguage } from "~/shared/helpers"
import "bootstrap/js/dropdown";
import 'flatpickr/dist/flatpickr.css'

global.jQuery = require("jquery")

import moment from 'moment'
moment.locale(browserLanguage);
import * as poly from "smoothscroll-polyfill"
poly.polyfill()
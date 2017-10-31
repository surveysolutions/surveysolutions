import { browserLanguage } from "~/shared/helpers"
import "bootstrap";
import 'flatpickr/dist/flatpickr.css'
import "toastr/build/toastr.css"

global.jQuery = require("jquery")

import moment from 'moment'
moment.locale(browserLanguage);
import * as poly from "smoothscroll-polyfill"
poly.polyfill()

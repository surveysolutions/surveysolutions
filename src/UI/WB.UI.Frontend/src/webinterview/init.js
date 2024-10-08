import { browserLanguage } from '~/shared/helpers'
import jquery from '~/shared/jquery'
import 'bootstrap'
import * as bootstrap from 'bootstrap'
import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import moment from 'moment'

moment.locale(browserLanguage)

import * as poly from 'smoothscroll-polyfill'

poly.polyfill()

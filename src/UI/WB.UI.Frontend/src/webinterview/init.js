import { browserLanguage } from '~/shared/helpers'
import 'bootstrap'
import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import 'ag-grid-community/dist/styles/ag-grid.css'
import 'ag-grid-community/dist/styles/ag-theme-bootstrap.css'

window.jQuery = require('jquery')

import moment from 'moment'
moment.locale(browserLanguage)
import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

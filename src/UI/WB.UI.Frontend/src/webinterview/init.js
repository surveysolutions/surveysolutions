import { browserLanguage } from '~/shared/helpers'

import 'bootstrap'
import * as bootstrap from 'bootstrap'

import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'

//TODO: MIGRATION, fix styles refs
//import '@ag-grid-community/styles/ag-grid.css'
//import '@ag-grid-community/styles/ag-theme-bootstrap.css'

import $ from 'jquery';
window.jQuery = $;

import moment from 'moment'
moment.locale(browserLanguage)
import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

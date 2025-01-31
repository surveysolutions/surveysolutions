import { setupErrorHandler } from './errorsHandler';
setupErrorHandler();

import $ from 'jquery';
window.jQuery = window.$ = $;

import 'bootstrap';
import "../../Content/bootstrap-migrate.less"
import '/Scripts/custom/utils.js';

import './validation';

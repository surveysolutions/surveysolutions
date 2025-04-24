import { setupErrorHandler } from './errorsHandler';
setupErrorHandler();

import $ from 'jquery';
window.jQuery = window.$ = $;

import '../../Content/bootstrap-migrate.less';
import '/Scripts/custom/utils.js';

import './validation';

import '/questionnaire/content/designer-start/designer-start.less';

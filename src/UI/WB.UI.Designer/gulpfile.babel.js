import { series, parallel } from "gulp";
import path from "path"

import vendor from "./build/vendor";
import questionnaire from "./build/questionnaire";
import bundle from "./build/bundler";
import config from "./build/config"
import rimraf from "rimraf"

export const questionnaires = questionnaire;
export const bundler = bundle;

export const cleanup = (done) => rimraf(path.join(config.dist, "**/*"), done)
export default series(cleanup, parallel(vendor, bundler), questionnaire);

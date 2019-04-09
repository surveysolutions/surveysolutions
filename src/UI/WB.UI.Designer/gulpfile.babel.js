import { series, src, parallel } from "gulp";
import clean from "gulp-clean";

import vendor from "./build/vendor";
import questionnaire from "./build/questionnaire";
import bundle from "./build/bundler";
import config from "./build/config"

export const cleanup = () => {
  return src(config.dist, { allowEmpty: true }).pipe(clean());
};

export const questionnaires = questionnaire;
export const bundler = bundle;

export default series(cleanup, parallel(vendor, bundler, questionnaire));

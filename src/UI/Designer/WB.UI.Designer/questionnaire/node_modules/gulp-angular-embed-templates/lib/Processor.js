var createLogger = require('./utils').createLogger;

function Processor() {}

/**
 * @param config
 */
Processor.prototype.init = function(config) {
    this.config = Object.assign({}, config);
    this.logger = createLogger(this.config.logger);
};

/**
 *
 * @param {Object} fileContext config object with params:
 * {
 *   "content": {string} file content
 *   "path" : {string} file path in system
 * }
 *
 * @param {Function} cb function to call when process complete, should be called with Array of replacements:
 * [
 *   {
 *     "start": {number} replacement start
 *     "length": {number} replacement length
 *     "replace": [Buffer] replace by this buffers
 *   }
 * ]
 *
 * @param {Function} onErr error callback
 */
Processor.prototype.process = function(fileContext, cb, onErr) {
    cb();
};

module.exports = Processor;
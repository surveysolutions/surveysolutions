var pathModule = require('path');
var utils = require('./utils');

var ProcessorEngine = utils.extend(Object, {
    init: function (config) {
        this.config = Object.assign({}, config);

        this.logger = utils.createLogger(config.logger);

        if (this.config.skipErrors === undefined) {
            this.config.skipErrors = false;
        }
        if (!this.config.jsEncoding) {
            this.config.jsEncoding = 'utf-8';
        }

        this.processors = this.config.processors;
        delete this.config.processors;
        for (var i = 0; i < this.processors.length; i++) {
            this.processors[i].init(this.config);
        }
    },

    /**
     * @param {File} file node file object
     * @param {Function} cb success callback
     * @param {Function} onErr error callback
     */
    process: function (file, cb, onErr) {
        var fileContent = file.contents.toString(this.config.jsEncoding);
        var entrances = [];

        var fileContext = {
            content: fileContent,
            path: this.config.basePath ? this.config.basePath : pathModule.dirname(file.path)
        };

        utils.recursiveCycle(
            this.processors,
            function onIteration(processor, next) {
                processor.process(fileContext, function cb(paths) {
                    Array.prototype.push.apply(entrances, paths);
                    next();
                }, onErr);
            },
            function onProcessorSuccess() {
                if (entrances.length !== 0) {
                    file.contents = this.joinParts(fileContent, entrances);
                }
                cb(null, file);
            }.bind(this)
        );
    },

    /**
     * join parts [before] ['template':] [template] [after]
     * @param {String} fileContent
     * @param {Array} entrances
     * @return Buffer
     */
    joinParts: function (fileContent, entrances) {
        entrances.sort(function (e1, e2) {
            return e1.start - e2.start;
        });

        var parts = [];
        var index = 0;
        for (var i = 0; i < entrances.length; i++) {
            var entrance = entrances[i];

            parts.push(Buffer(fileContent.substring(index, entrance.start)));
            Array.prototype.push.apply(parts, entrance.replace);

            index = entrance.start + entrance.length;
        }
        parts.push(Buffer(fileContent.substr(index)));
        return Buffer.concat(parts);
    }
});

module.exports = ProcessorEngine;
var extend = require('./utils').extend;
var Processor = require('./Processor');

var RegexpProcessor = extend(Processor, {
    init : function(config) {
        this._super.init(config);

        var skipTemplates = config.skipTemplates || function() {return false;};
        delete config.skipTemplates;
        if (typeof skipTemplates === 'function') {
            /* OK */
        } else if (skipTemplates instanceof RegExp) {
            var regexp = skipTemplates;
            skipTemplates = function(templateUrl, fileContext) {
                return regexp.test(templateUrl);
            }
        } else {
            logger.warn('"skipTemplates" options should be either function or regexp, actual type is ' + typeof skipTemplates);
            skipTemplates = function() {return false;}
        }
        this.config.skipTemplates = skipTemplates;
    },

    /**
     * @return {String} return regexp pattern
     */
    getPattern : function() {
        throw 'not implemented';
    },

    process : function(fileContext, cb, onErr) {
        /**
         * @type {RegExp} we create a regexp each time with 'g' flag to hold current position
         * and search second time from previous position + 1
         */
        var pattern = this.getPattern();
        var regexp = new RegExp(pattern, 'g');
        var entrances = [];

        var that = this;
        function next() {
            var match = regexp.exec(fileContext.content);
            if (match === null) {
                cb(entrances);
                return;
            }
            if (that.config.skipTemplates(match[1], fileContext)) {
                that.logger.info('skip template "%s" in file "%s"', match[1], fileContext.path);
                next();
                return;
            }
            that.replaceMatch(fileContext, match, function(entrance) {
                if (entrance) {
                    entrances.push(entrance);
                }
                next();
            }, onErr);
        }
        next();
    },

    replaceMatch : function(fileContext, match, cb, onErr) {
        throw 'not implemented';
    }
});

module.exports = RegexpProcessor;
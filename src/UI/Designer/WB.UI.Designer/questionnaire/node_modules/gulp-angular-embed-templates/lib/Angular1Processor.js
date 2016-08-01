var fs = require('fs');
var pathModule = require('path');
var Minimize = require('minimize');
var html = require('htmlparser2');

var extend = require('./utils').extend;
var RegexpProcessor = require('./RegexpProcessor');

const TEMPLATE_BEGIN = Buffer('template:\'');
const TEMPLATE_END = Buffer('\'');

function escapeSingleQuotes(string) {
    const ESCAPING = {
        '\'': '\\\'',
        '\\': '\\\\',
        '\n': '\\n',
        '\r': '\\r',
        '\u2028': '\\u2028',
        '\u2029': '\\u2029'
    };
    return string.replace(/['\\\n\r\u2028\u2029]/g, function (character) {
        return ESCAPING[character];
    });
}

var Angular1Processor = extend(RegexpProcessor, {
    init : function(config) {
        this._super.init(config);

        if (!this.config.minimize) {
            this.config.minimize = {};
        }
        this.minimizer = new Minimize(this.config.minimize);
        if (!this.config.minimize.parser) {
            this.minimizer.htmlparser = new html.Parser(
                new html.DomHandler(this.minimizer.emits('read')), {lowerCaseAttributeNames:false}
            );
        }

        if (!this.config.templateEncoding) {
            this.config.templateEncoding = 'utf-8';
        }
    },

    /**
     * @returns {String} pattern to search
     */
    getPattern : function() {
        return '[\'"]?templateUrl[\'"]?[\\s]*:[\\s]*[\'"`]([^\'"`]+)[\'"`]';
    },

    /**
     * Find next "templateUrl:", and try to replace url with content if template available, less then maximum size.
     * This is recursive function: it call itself until one of two condition happens:
     * - error happened (error emitted in pipe and stop recursive calls)
     * - no 'templateUrl' left (call 'fileCallback' and stop recursive calls)
     *
     * @param {Object} fileContext source file content
     * @param {Object} match Regexp.exec result
     * @param {Function} cb to call after match replaced
     * @param {Function} onErr error handler
     */
    replaceMatch : function(fileContext, match, cb, onErr) {
        var relativeTemplatePath = match[1];
        var templatePath = pathModule.join(fileContext.path, relativeTemplatePath);
        var warnNext = function(msg) {
            this.logger.warn(msg);
            cb();
        }.bind(this);
        var onError = this.config.skipErrors ? warnNext : onErr;

        this.logger.debug('template path: %s', templatePath);

        if (this.config.maxSize) {
            var fileStat = fs.statSync(templatePath);
            if (fileStat && fileStat.size > this.config.maxSize) {
                warnNext('template file "' + templatePath + '" exceeds configured max size "' + this.config.maxSize + '" actual size is "' + fileStat.size + '"');
                return;
            }
        }

        var embedTemplate = this.embedTemplate.bind(this);
        var minimizer = this.minimizer;
        fs.readFile(templatePath, {encoding: this.config.templateEncoding}, function(err, templateContent) {
            if (err) {
                onError('Can\'t read template file: "' + templatePath + '". Error details: ' + err);
                return;
            }

            minimizer.parse(templateContent, function (err, minifiedContent) {
                if (err) {
                    onError('Error while minifying angular template "' + templatePath + '". Error from "minimize" plugin: ' + err);
                    return;
                }

                var templateBuffer = Buffer(escapeSingleQuotes(minifiedContent));
                cb(embedTemplate(match, templateBuffer));
            });
        });
    },

    embedTemplate : function(match, templateBuffer) {
        return {
            start : match.index,
            length: match[0].length,
            replace: [TEMPLATE_BEGIN, templateBuffer, TEMPLATE_END]
        }
    }
});

module.exports = Angular1Processor;
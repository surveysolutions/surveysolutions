define('ace/mode/ncalc', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text', 'ace/tokenizer', 'ace/mode/ncalc_highlight_rules', 'ace/mode/folding/ncalc'], function (require, exports, module) {

    var oop = require("../lib/oop");
    var TextMode = require("./text").Mode;
    var Tokenizer = require("../tokenizer").Tokenizer;
    var NCalcHighlightRules = require("./ncalc_highlight_rules").NCalcHighlightRules;
    var NCalcFoldMode = require("./folding/ncalc").FoldMode;

    var Mode = function () {
        var highlighter = new NCalcHighlightRules();

        this.$tokenizer = new Tokenizer(highlighter.getRules());
        this.foldingRules = new NCalcFoldMode();
    };
    oop.inherits(Mode, TextMode);

    (function () {
        this.getNextLineIndent = function (state, line, tab) {
            if (state == "listblock") {
                var match = /^((?:.+)?)([-+*][ ]+)/.exec(line);
                if (match) {
                    return new Array(match[1].length + 1).join(" ") + match[2];
                } else {
                    return "";
                }
            } else {
                return this.$getIndent(line);
            }
        };
    }).call(Mode.prototype);

    exports.Mode = Mode;
});

define('ace/mode/ncalc_highlight_rules', 
['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text_highlight_rules'], 
function (require, exports, module) {

    var oop = require("../lib/oop");
    var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;

    var NCalcHighlightRules = function () {
        var identifier = "[$A-Za-z_][$\\w]*";
        var keywords = ("this");
        var operators = ("and|or");
        var langConstant = ("true|false");
        var illegal = ("");

        var keywordMapper = this.createKeywordMapper({
            "keyword": keywords,
            "keyword.operator": operators,
            "constant.language": langConstant,
            "invalid.illegal": illegal,
        }, "identifier");

        this.$rules = {
            start: [{
                    token: "variable",
                    regex: "\\[(?:" + identifier + ")?\\]"
                }, {
                    token: "constant.numeric",
                    regex: "(?:0x[\\da-fA-F]+|(?:\\d+(?:\\.\\d+)?|\\.\\d+)(?:[eE][+-]?\\d+)?)"
                }, {
                    token: "keyword.operator",
                    regex: "(?:[-+*/%<>&|^!?=]=|>>>=?|\\-\\-|\\+\\+|::|&&=|\\|\\|=|<<=|>>=|\\?\\.|\\.{2,3}|[!*+-=><])"
                }, {
                    token: keywordMapper,
                    regex: identifier
                }, {
                    token: "paren.lparen",
                    regex: "[(]"
                }, {
                    token: "paren.rparen",
                    regex: "[)]"
                },{ 
                    token: "text",
                    regex: "\\s+"
                }
            ]
        };
    };
	
	oop.inherits(NCalcHighlightRules, TextHighlightRules);

	exports.NCalcHighlightRules = NCalcHighlightRules;
});

define('ace/mode/folding/ncalc', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/folding/fold_mode', 'ace/range'], function (require, exports, module) {

    var oop = require("../../lib/oop");
    var BaseFoldMode = require("./fold_mode").FoldMode;
    var Range = require("../../range").Range;

    var FoldMode = exports.FoldMode = function () { };
    oop.inherits(FoldMode, BaseFoldMode);

    (function () {
        this.foldingStartMarker = "";
        
        this.getFoldWidget = function (session, foldStyle, row) {
        };

        this.getFoldWidgetRange = function (session, foldStyle, row) {
        };
        
    }).call(FoldMode.prototype);

});
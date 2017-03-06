ace.define("ace/mode/text-extended",["require","exports","module","ace/lib/oop","ace/mode/text"], function(require, exports, module) {
"use strict";
    var oop = require("../lib/oop");
    var TextMode = require("./text").Mode;
    var SubstitutionsHighlightRules = require("ace/mode/text_with_substitutions_highlight_rules").SubstitutionsHighlightRules;

    var Mode = function () {
        this.HighlightRules = SubstitutionsHighlightRules;
    };

    oop.inherits(Mode, TextMode);

    (function() {

    }).call(Mode.prototype);

    exports.Mode = Mode;
});

ace.define("ace/mode/text_with_substitutions_highlight_rules", ["require", "exports", "module", "ace/lib/oop", "text_highlight_rules"], function (require, exports, module) {
    "use strict";

    var oop = require("../lib/oop");
    var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;

    var SubstitutionsHighlightRules = function () {
        // useful regexp resources: https://regex101.com/
        // mode tester:  https://ace.c9.io/tool/mode_creator.html
        this.$rules = {
            "start": [
                {
                    token: "support.variable", // String, Array, or Function: the CSS token to apply
                    regex: "\%[a-zA-Z][_a-zA-Z0-9]{0,31}\%" // String or RegExp: the regexp to match
                },
                {
                    token: "variable", // String, Array, or Function: the CSS token to apply
                    regex: "\%[_a-zA-Z0-9.,\/#!$%\^&\*;:{}=\-_`~()]+\%" // String or RegExp: the regexp to match
                }
            ]
        };
    };
    oop.inherits(SubstitutionsHighlightRules, TextHighlightRules);

    exports.SubstitutionsHighlightRules = SubstitutionsHighlightRules;
});
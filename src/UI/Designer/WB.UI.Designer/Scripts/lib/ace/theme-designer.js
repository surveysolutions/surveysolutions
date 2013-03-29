/* ***** BEGIN LICENSE BLOCK *****
 * Distributed under the BSD license:
 *
 * Copyright (c) 2010, Ajax.org B.V.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Ajax.org B.V. nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL AJAX.ORG B.V. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * ***** END LICENSE BLOCK ***** */

define('ace/theme/designer', ['require', 'exports', 'module' , 'ace/lib/dom'], function(require, exports, module) {

exports.isDark = true;
exports.cssClass = "ace-designer";
exports.cssText = ".ace-designer .ace_gutter {\
background: #011e3a;\
color: #fff\
}\
.ace-designer .ace_print-margin {\
width: 1px;\
background: #011e3a\
}\
.ace-designer .ace_scroller {\
background-color: #164b69\
}\
.ace-designer .ace_text-layer {\
color: #FFFFFF\
}\
.ace-designer .ace_cursor {\
border-left: 2px solid #FFFFFF\
}\
.ace-designer .ace_overwrite-cursors .ace_cursor {\
border-left: 0px;\
border-bottom: 1px solid #FFFFFF\
}\
.ace-designer .ace_marker-layer .ace_selection {\
background: rgba(179, 101, 57, 0.75)\
}\
.ace-designer.ace_multiselect .ace_selection.ace_start {\
box-shadow: 0 0 3px 0px #002240;\
border-radius: 2px\
}\
.ace-designer .ace_marker-layer .ace_step {\
background: rgb(127, 111, 19)\
}\
.ace-designer .ace_marker-layer .ace_bracket {\
margin: -1px 0 0 -1px;\
border: 1px solid rgba(255, 255, 255, 0.15)\
}\
.ace-designer .ace_marker-layer .ace_active-line {\
background: rgba(0, 0, 0, 0.35)\
}\
.ace-designer .ace_gutter-active-line {\
background-color: rgba(0, 0, 0, 0.35)\
}\
.ace-designer .ace_marker-layer .ace_selected-word {\
border: 1px solid rgba(179, 101, 57, 0.75)\
}\
.ace-designer .ace_invisible {\
color: rgba(255, 255, 255, 0.15)\
}\
.ace-designer .ace_keyword,\
.ace-designer .ace_meta {\
color: #FF9D00\
}\
.ace-designer .ace_constant,\
.ace-designer .ace_constant.ace_character,\
.ace-designer .ace_constant.ace_character.ace_escape,\
.ace-designer .ace_constant.ace_other {\
color: #FF628C\
}\
.ace-designer .ace_invalid {\
color: #F8F8F8;\
background-color: #800F00\
}\
.ace-designer .ace_support {\
color: #80FFBB\
}\
.ace-designer .ace_support.ace_constant {\
color: #EB939A\
}\
.ace-designer .ace_fold {\
background-color: #FF9D00;\
border-color: #FFFFFF\
}\
.ace-designer .ace_support.ace_function {\
color: #FFB054\
}\
.ace-designer .ace_storage {\
color: #FFEE80\
}\
.ace-designer .ace_entity {\
color: #FFDD00\
}\
.ace-designer .ace_string {\
color: #3AD900\
}\
.ace-designer .ace_string.ace_regexp {\
color: #80FFC2\
}\
.ace-designer .ace_comment {\
font-style: italic;\
color: #0088FF\
}\
.ace-designer .ace_variable {\
color: #CCCCCC\
}\
.ace-designer .ace_variable.ace_language {\
color: #FF80E1\
}\
.ace-designer .ace_meta.ace_tag {\
color: #9EFFFF\
}\
.ace-designer .ace_markup.ace_underline {\
text-decoration: underline\
}\
.ace-designer .ace_markup.ace_heading {\
color: #C8E4FD;\
background-color: #001221\
}\
.ace-designer .ace_markup.ace_list {\
background-color: #130D26\
}\
.ace-designer .ace_indent-guide {\
background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWNgUHL4zzBz5sz/AA80BCzv+WXhAAAAAElFTkSuQmCC) right repeat-y\
}";

var dom = require("../lib/dom");
dom.importCssString(exports.cssText, exports.cssClass);
});

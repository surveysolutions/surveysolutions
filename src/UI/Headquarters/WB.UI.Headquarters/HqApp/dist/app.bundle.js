webpackJsonp([0],{

/***/ 122:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_polyfill__ = __webpack_require__(123);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_polyfill___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_babel_polyfill__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vue__ = __webpack_require__(90);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__components_ExportButtons__ = __webpack_require__(306);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__components_Typeahead__ = __webpack_require__(309);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__components_Layout__ = __webpack_require__(312);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__components_Filters__ = __webpack_require__(315);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__components_FilterBlock__ = __webpack_require__(318);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__components_DataTables__ = __webpack_require__(321);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__components_ModalFrame__ = __webpack_require__(324);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_9__components_Confirm__ = __webpack_require__(327);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_10__plugins_locale__ = __webpack_require__(330);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_11__store__ = __webpack_require__(341);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_12__App__ = __webpack_require__(345);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_13__config__ = __webpack_require__(347);
















__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].use(__WEBPACK_IMPORTED_MODULE_13__config__["a" /* default */]);

__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].use(__WEBPACK_IMPORTED_MODULE_10__plugins_locale__["a" /* default */], {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': __WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].$config.resources
    }
});

__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Layout", __WEBPACK_IMPORTED_MODULE_4__components_Layout__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Filters", __WEBPACK_IMPORTED_MODULE_5__components_Filters__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("FilterBlock", __WEBPACK_IMPORTED_MODULE_6__components_FilterBlock__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Typeahead", __WEBPACK_IMPORTED_MODULE_3__components_Typeahead__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("DataTables", __WEBPACK_IMPORTED_MODULE_7__components_DataTables__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("ModalFrame", __WEBPACK_IMPORTED_MODULE_8__components_ModalFrame__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Confirm", __WEBPACK_IMPORTED_MODULE_9__components_Confirm__["a" /* default */]);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("ExportButtons", __WEBPACK_IMPORTED_MODULE_2__components_ExportButtons__["a" /* default */]);

var router = __webpack_require__(348).default;

var vueApp = new __WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */]({
    el: "#vueApp",
    render: function render(h) {
        return h(__WEBPACK_IMPORTED_MODULE_12__App__["a" /* default */]);
    },
    components: { App: __WEBPACK_IMPORTED_MODULE_12__App__["a" /* default */] },
    store: __WEBPACK_IMPORTED_MODULE_11__store__["a" /* default */],
    router: router
});

/***/ }),

/***/ 306:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_ExportButtons_vue__ = __webpack_require__(307);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_ce5b43f2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_ExportButtons_vue__ = __webpack_require__(308);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_ExportButtons_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_ce5b43f2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_ExportButtons_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\ExportButtons.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] ExportButtons.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-ce5b43f2", Component.options)
  } else {
    hotAPI.reload("data-v-ce5b43f2", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 307:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    props: {
        table: {
            type: Object,
            reqiured: true
        }
    }
});

/***/ }),

/***/ 308:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "dropdown"
  }, [_c('button', {
    staticClass: "btn btn-default dropdown-toggle",
    attrs: {
      "type": "button",
      "id": "dropdownMenu1",
      "data-toggle": "dropdown",
      "aria-haspopup": "true",
      "aria-expanded": "true"
    }
  }, [_vm._v("\n        " + _vm._s(_vm.$t('Pages.ExportButton')) + "\n        "), _c('span', {
    staticClass: "caret"
  })]), _vm._v(" "), _c('ul', {
    staticClass: "dropdown-menu",
    attrs: {
      "aria-labelledby": "dropdownMenu1"
    }
  }, [_c('li', [_c('a', {
    attrs: {
      "href": _vm.$store.state.exportUrls.excel
    }
  }, [_vm._v(_vm._s(_vm.$t('Pages.ExportToExcel')))])]), _vm._v(" "), _c('li', [_c('a', {
    attrs: {
      "href": _vm.$store.state.exportUrls.csv
    }
  }, [_vm._v(_vm._s(_vm.$t('Pages.ExportToCsv')))])]), _vm._v(" "), _c('li', [_c('a', {
    attrs: {
      "href": _vm.$store.state.exportUrls.tab
    }
  }, [_vm._v(_vm._s(_vm.$t('Pages.ExportToTab')))])])])])
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-ce5b43f2", esExports)
  }
}

/***/ }),

/***/ 309:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Typeahead_vue__ = __webpack_require__(310);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Typeahead_vue___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Typeahead_vue__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_663fdba1_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Typeahead_vue__ = __webpack_require__(311);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Typeahead_vue___default.a,
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_663fdba1_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Typeahead_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\Typeahead.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Typeahead.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-663fdba1", Component.options)
  } else {
    hotAPI.reload("data-v-663fdba1", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 310:
/***/ (function(module, exports) {

//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//

module.exports = {
    name: 'user-selector',
    props: ['fetchUrl', 'controlId', 'value', 'placeholder', 'ajaxParams'],
    data: function data() {
        return {
            options: [],
            isLoading: false,
            searchTerm: ''
        };
    },
    computed: {
        inputId: function inputId() {
            return 'sb_' + this.controlId;
        },
        placeholderText: function placeholderText() {
            return this.placeholder || "Select";
        }
    },
    mounted: function mounted() {
        var _this = this;

        var jqEl = $(this.$el);
        var focusTo = jqEl.find('#' + this.inputId);
        jqEl.on('shown.bs.dropdown', function () {
            focusTo.focus();
            _this.fetchOptions(_this.searchTerm);
        });

        jqEl.on('hidden.bs.dropdown', function () {
            _this.searchTerm = "";
        });
    },
    methods: {
        onSearchBoxDownKey: function onSearchBoxDownKey(event) {
            var $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first();
            $firstOptionAnchor.focus();
        },
        onOptionUpKey: function onOptionUpKey(event) {
            var isFirstOption = $(event.target).parent().index() === 1;

            if (isFirstOption) {
                this.$refs.searchBox.focus();
                event.stopPropagation();
            }
        },

        fetchOptions: function fetchOptions() {
            var _this2 = this;

            var filter = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : "";

            this.isLoading = true;
            var requestParams = Object.assign({ query: filter, cache: false }, this.ajaxParams);
            $.get(this.fetchUrl, requestParams).done(function (response) {
                _this2.options = response.body.options || [];
                _this2.isLoading = false;
            }).always(function () {
                return _this2.isLoading = false;
            });
        },
        clear: function clear() {
            this.$emit('selected', null, this.controlId);
            this.searchTerm = "";
        },
        selectOption: function selectOption(value) {
            this.$emit('selected', value, this.controlId);
        },
        updateOptionsList: function updateOptionsList(e) {
            this.fetchOptions(e.target.value);
        },

        highlight: function highlight(title, searchTerm) {
            var encodedTitle = _.escape(title);
            if (searchTerm) {
                var safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));

                var iQuery = new RegExp(safeSearchTerm, "ig");
                return encodedTitle.replace(iQuery, function (matchedTxt, a, b) {
                    return '<strong>' + matchedTxt + '</strong>';
                });
            }

            return encodedTitle;
        }
    }
};

/***/ }),

/***/ 311:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "combo-box"
  }, [_c('div', {
    staticClass: "btn-group btn-input clearfix"
  }, [_c('button', {
    staticClass: "btn dropdown-toggle",
    attrs: {
      "type": "button",
      "data-toggle": "dropdown"
    }
  }, [(_vm.value === null) ? _c('span', {
    staticClass: "gray-text",
    attrs: {
      "data-bind": "label"
    }
  }, [_vm._v(_vm._s(_vm.placeholderText))]) : _c('span', {
    attrs: {
      "data-bind": "label"
    }
  }, [_vm._v(_vm._s(_vm.value.value))])]), _vm._v(" "), _c('ul', {
    ref: "dropdownMenu",
    staticClass: "dropdown-menu",
    attrs: {
      "role": "menu"
    }
  }, [_c('li', [_c('input', {
    directives: [{
      name: "model",
      rawName: "v-model",
      value: (_vm.searchTerm),
      expression: "searchTerm"
    }],
    ref: "searchBox",
    attrs: {
      "type": "text",
      "id": _vm.inputId,
      "placeholder": "Search"
    },
    domProps: {
      "value": (_vm.searchTerm)
    },
    on: {
      "input": [function($event) {
        if ($event.target.composing) { return; }
        _vm.searchTerm = $event.target.value
      }, _vm.updateOptionsList],
      "keyup": function($event) {
        if (!('button' in $event) && _vm._k($event.keyCode, "down", 40)) { return null; }
        _vm.onSearchBoxDownKey($event)
      }
    }
  })]), _vm._v(" "), _vm._l((_vm.options), function(option) {
    return _c('li', {
      key: option.key
    }, [_c('a', {
      attrs: {
        "href": "javascript:void(0);"
      },
      domProps: {
        "innerHTML": _vm._s(_vm.highlight(option.value, _vm.searchTerm))
      },
      on: {
        "click": function($event) {
          _vm.selectOption(option)
        },
        "keydown": function($event) {
          if (!('button' in $event) && _vm._k($event.keyCode, "up", 38)) { return null; }
          _vm.onOptionUpKey($event)
        }
      }
    })])
  }), _vm._v(" "), (_vm.isLoading) ? _c('li', [_c('a', [_vm._v(_vm._s(_vm.$t("Common.Loading")))])]) : _vm._e(), _vm._v(" "), (!_vm.isLoading && _vm.options.length === 0) ? _c('li', [_c('a', [_vm._v(_vm._s(_vm.$t("Common.NoResultsFound")))])]) : _vm._e()], 2)]), _vm._v(" "), (_vm.value !== null) ? _c('button', {
    staticClass: "btn btn-link btn-clear",
    on: {
      "click": _vm.clear
    }
  }, [_c('span')]) : _vm._e()])
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-663fdba1", esExports)
  }
}

/***/ }),

/***/ 312:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Layout_vue__ = __webpack_require__(313);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_00d5e640_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Layout_vue__ = __webpack_require__(314);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Layout_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_00d5e640_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Layout_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\Layout.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Layout.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-00d5e640", Component.options)
  } else {
    hotAPI.reload("data-v-00d5e640", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 313:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    props: {
        title: String,
        subtitle: String,
        hasFilter: {
            type: Boolean,
            default: function _default() {
                return false;
            }
        }
    },
    watch: {
        showProgress: function showProgress(value) {
            if (value) {
                $(this.$refs.pending.$el).modal({
                    backdrop: 'static',
                    keyboard: false
                });
            } else {
                $(this.$refs.pending.$el).modal("hide");
            }
        }
    },
    computed: {
        information: function information() {
            return {
                "main-information": this.hasFilter,
                "information": !this.hasFilter
            };
        },
        showProgress: function showProgress() {
            return this.$store.state.pendingProgress;
        }
    }
});

/***/ }),

/***/ 314:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('main', [_c('div', {
    staticClass: "container-fluid"
  }, [_c('div', {
    staticClass: "row"
  }, [_vm._t("filters"), _vm._v(" "), _c('div', {
    class: _vm.information
  }, [_c('div', {
    staticClass: "page-header clearfix"
  }, [_c('h1', [_vm._v("\n\t\t\t\t\t\t\t" + _vm._s(_vm.title) + "\n\t\t\t\t\t\t")]), _vm._v(" "), (_vm.subtitle) ? _c('h3', [_vm._v(_vm._s(_vm.subtitle))]) : _vm._e(), _vm._v(" "), _vm._t("exportButtons")], 2), _vm._v(" "), _vm._t("default")], 2)], 2), _vm._v(" "), _vm._t("modals"), _vm._v(" "), _c('ModalFrame', {
    ref: "pending",
    attrs: {
      "id": "pendingProgress",
      "title": _vm.$t('Common.Loading'),
      "canClose": false
    }
  }, [_c('div', {
    staticClass: "progress progress-striped active",
    staticStyle: {
      "margin-bottom": "0"
    }
  }, [_c('div', {
    staticClass: "progress-bar",
    staticStyle: {
      "width": "100%"
    }
  })])])], 2)])
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-00d5e640", esExports)
  }
}

/***/ }),

/***/ 315:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Filters_vue__ = __webpack_require__(316);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_70a0b355_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Filters_vue__ = __webpack_require__(317);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Filters_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_70a0b355_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Filters_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\Filters.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Filters.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-70a0b355", Component.options)
  } else {
    hotAPI.reload("data-v-70a0b355", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 316:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({});

/***/ }),

/***/ 317:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('aside', {
    staticClass: "filters"
  }, [_vm._m(0), _vm._v(" "), _c('div', {
    staticClass: "filters-container"
  }, [_c('h4', [_vm._v(_vm._s(_vm.$t('Pages.FilterTitle')))]), _vm._v(" "), _vm._t("default")], 2)])
}
var staticRenderFns = [function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "foldback-button",
    attrs: {
      "id": "hide-filters"
    }
  }, [_c('span', {
    staticClass: "arrow"
  }), _vm._v(" "), _c('span', {
    staticClass: "arrow"
  }), _vm._v(" "), _c('span', {
    staticClass: "glyphicon glyphicon-tasks",
    attrs: {
      "aria-hidden": "true"
    }
  })])
}]
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-70a0b355", esExports)
  }
}

/***/ }),

/***/ 318:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_FilterBlock_vue__ = __webpack_require__(319);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_3695054f_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_FilterBlock_vue__ = __webpack_require__(320);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_FilterBlock_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_3695054f_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_FilterBlock_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\FilterBlock.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] FilterBlock.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-3695054f", Component.options)
  } else {
    hotAPI.reload("data-v-3695054f", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 319:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    props: ["title"]
});

/***/ }),

/***/ 320:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "block-filter"
  }, [_c('h5', [_vm._v(_vm._s(_vm.title))]), _vm._v(" "), _vm._t("default")], 2)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-3695054f", esExports)
  }
}

/***/ }),

/***/ 321:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_DataTables_vue__ = __webpack_require__(322);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_50d1e445_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_DataTables_vue__ = __webpack_require__(323);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_DataTables_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_50d1e445_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_DataTables_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\DataTables.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] DataTables.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-50d1e445", Component.options)
  } else {
    hotAPI.reload("data-v-50d1e445", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 322:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//


/* harmony default export */ __webpack_exports__["a"] = ({

    props: {
        addParamsToRequest: {
            type: Function,
            default: function _default(d) {
                return d;
            }
        },
        responseProcessor: {
            type: Function,
            default: function _default(r) {
                return r;
            }
        },
        tableOptions: {
            type: Object,
            default: function _default() {
                return {};
            }
        },
        contextMenuItems: {
            type: Function
        },
        authorizedUser: { type: Object, default: function _default() {
                return {};
            }
        },
        hasPaging: {
            type: Boolean,
            default: function _default() {
                return true;
            }
        },
        hasSearch: {
            type: Boolean,
            default: function _default() {
                return true;
            }
        }
    },

    data: function data() {
        return {
            table: null
        };
    },

    methods: {
        reload: function reload(data) {
            this.table.ajax.data = this.addParamsToRequest(data || {});
            this.table.rows().deselect();
            this.table.ajax.reload();
        },
        disableRow: function disableRow(rowIndex) {
            $(this.table.row(rowIndex).node()).addClass("disabled");
        },
        selectRowAndGetData: function selectRowAndGetData(selectedItem) {
            this.table.rows().deselect();
            var rowIndex = selectedItem.parent().children().index(selectedItem);
            this.table.row(rowIndex).select();
            var rowData = this.table.rows({ selected: true }).data()[0];

            this.table.rows().deselect();

            return {
                rowData: rowData,
                rowIndex: rowIndex
            };
        },
        onTableInitComplete: function onTableInitComplete() {
            var self = this;

            $(this.$el).parents('.dataTables_wrapper').find('.dataTables_filter label').on('click', function (e) {
                if (e.target !== this) return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                } else {
                    $(this).addClass("active");
                    $(this).children("input[type='search']").delay(200).queue(function () {
                        $(this).focus();$(this).dequeue();
                    });
                }
            });
        },
        initContextMenu: function initContextMenu() {
            var _this = this;

            if (this.contextMenuItems == null) return;
            var contextMenuOptions = {
                selector: "#" + this.$el.attributes.id.value + " tbody tr",
                autoHide: false,
                build: function build($trigger, e) {
                    var selectedRow = _this.selectRowAndGetData($trigger);

                    if (selectedRow.rowData == null) return false;

                    var items = _this.contextMenuItems(selectedRow);
                    return { items: items };
                },
                trigger: 'left'
            };

            $.contextMenu(contextMenuOptions);
        }
    },

    mounted: function mounted() {
        var _this2 = this;

        var self = this;
        var options = $.extend({
            processing: true,
            serverSide: true,
            language: {
                "url": window.input.settings.config.dataTableTranslationsUrl
            },
            searchHighlight: true,
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 20, // page size
            dom: "frtp",
            conditionalPaging: true,
            paging: this.hasPaging,
            searching: this.hasSearch
        }, this.tableOptions);

        options.ajax.data = function (d) {
            _this2.addParamsToRequest(d);

            var requestUrl = _this2.table.ajax.url() + '?' + decodeURIComponent($.param(d));

            _this2.$store.dispatch('setExportUrls', {
                excel: requestUrl + "&exportType=excel",
                csv: requestUrl + "&exportType=csv",
                tab: requestUrl + "&exportType=tab"
            });
        };

        options.ajax.complete = function (response) {
            _this2.responseProcessor(response.responseJSON);
        };

        this.table = $(this.$el).DataTable(options);
        this.table.on('init.dt', this.onTableInitComplete);
        this.table.on('select', function (e, dt, type, indexes) {
            self.$emit('select', e, dt, type, indexes);
        });
        this.table.on('deselect', function (e, dt, type, indexes) {
            self.$emit('deselect', e, dt, type, indexes);
        });
        this.table.on('click', 'tbody td', function () {
            var cell = self.table.cell(this);

            if (cell.index() != null && cell.index().column > 0) {
                var rowId = self.table.row(this).id();
                var columns = self.table.settings().init().columns;

                self.$emit('cell-clicked', columns[this.cellIndex].name, rowId, cell.data());
            }
        });
        this.$emit('DataTableRef', this.table);

        this.initContextMenu();
    }
});

/***/ }),

/***/ 323:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _vm._m(0)
}
var staticRenderFns = [function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('table', {
    staticClass: "table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive"
  }, [_c('thead'), _vm._v(" "), _c('tbody')])
}]
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-50d1e445", esExports)
  }
}

/***/ }),

/***/ 324:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_ModalFrame_vue__ = __webpack_require__(325);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_5a68a3d6_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_ModalFrame_vue__ = __webpack_require__(326);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_ModalFrame_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_5a68a3d6_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_ModalFrame_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\ModalFrame.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] ModalFrame.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-5a68a3d6", Component.options)
  } else {
    hotAPI.reload("data-v-5a68a3d6", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 325:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    props: {
        id: String,
        title: String,
        canClose: {
            type: Boolean,
            default: function _default() {
                return true;
            }
        }
    }

});

/***/ }),

/***/ 326:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "modal fade",
    attrs: {
      "id": _vm.id,
      "tabindex": "-1",
      "role": "dialog"
    }
  }, [_c('div', {
    staticClass: "modal-dialog",
    attrs: {
      "role": "document"
    }
  }, [_c('div', {
    staticClass: "modal-content"
  }, [_c('div', {
    staticClass: "modal-header"
  }, [(_vm.canClose) ? _c('button', {
    staticClass: "close",
    attrs: {
      "type": "button",
      "data-dismiss": "modal",
      "aria-label": "Close"
    }
  }, [_c('span', {
    attrs: {
      "aria-hidden": "true"
    }
  })]) : _vm._e(), _vm._v(" "), _c('h2', [_vm._v(_vm._s(_vm.title))])]), _vm._v(" "), _c('div', {
    staticClass: "modal-body"
  }, [_vm._t("default")], 2), _vm._v(" "), _c('div', {
    staticClass: "modal-footer"
  }, [_vm._t("actions")], 2)])])])
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-5a68a3d6", esExports)
  }
}

/***/ }),

/***/ 327:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Confirm_vue__ = __webpack_require__(328);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_8dafe8cc_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Confirm_vue__ = __webpack_require__(329);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Confirm_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_8dafe8cc_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Confirm_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\components\\Confirm.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Confirm.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-8dafe8cc", Component.options)
  } else {
    hotAPI.reload("data-v-8dafe8cc", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 328:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    props: {
        id: String,
        title: {
            type: String,
            required: false
        }
    },

    data: function data() {
        return {
            callback: null
        };
    },


    computed: {
        confirm_title: function confirm_title() {
            return this.title || this.$t("Pages.ConfirmationNeededTitle");
        }
    },

    methods: {
        cancel: function cancel() {
            this.callback(false);
            $(this.$el).modal('hide');
        },
        confirm: function confirm() {
            this.callback(true);
            $(this.$el).modal('hide');
        },
        promt: function promt(callback) {
            this.callback = callback;
            $(this.$el).modal();
        }
    }
});

/***/ }),

/***/ 329:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('ModalFrame', {
    attrs: {
      "id": _vm.id,
      "title": _vm.confirm_title
    }
  }, [_vm._t("default"), _vm._v(" "), _c('button', {
    staticClass: "btn btn-primary",
    attrs: {
      "type": "button"
    },
    on: {
      "click": _vm.confirm
    },
    slot: "actions"
  }, [_vm._v(_vm._s(_vm.$t("Common.Ok")))]), _vm._v(" "), _c('button', {
    staticClass: "btn btn-link",
    attrs: {
      "type": "button"
    },
    on: {
      "click": _vm.cancel
    },
    slot: "actions"
  }, [_vm._v(_vm._s(_vm.$t("Common.Cancel")))])], 2)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-8dafe8cc", esExports)
  }
}

/***/ }),

/***/ 330:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_i18next__ = __webpack_require__(331);


/*  the Plugin */
var VueI18Next = {
    install: function install(Vue, options) {
        /*  determine options  */
        __WEBPACK_IMPORTED_MODULE_0_i18next__["a" /* default */].init(Object.assign({
            fallbackLng: 'en'
        }, options));

        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$t', {
            get: function get() {
                return function (key, options) {
                    //var opts = { resources: locale }

                    // for now we will not support language change on the fly
                    //Vue.util.extend(opts, options)
                    return __WEBPACK_IMPORTED_MODULE_0_i18next__["a" /* default */].t(key, options);
                };
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$t', {
            get: function get() {
                return function (key, options) {
                    //var opts = { resources: locale }
                    //Vue.util.extend(opts, options)
                    return __WEBPACK_IMPORTED_MODULE_0_i18next__["a" /* default */].t(key, options);
                };
            }
        });
    }

    /*  export API  */
};/* harmony default export */ __webpack_exports__["a"] = (VueI18Next);

/***/ }),

/***/ 341:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_vue__ = __webpack_require__(90);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vuex__ = __webpack_require__(342);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_pnotify__ = __webpack_require__(343);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_pnotify___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_pnotify__);



// PNotify.prototype.options.styling = "bootstrap3"

__WEBPACK_IMPORTED_MODULE_0_vue__["a" /* default */].use(__WEBPACK_IMPORTED_MODULE_1_vuex__["a" /* default */]);

var config = JSON.parse(window.vueApp.getAttribute('configuration'));

var store = new __WEBPACK_IMPORTED_MODULE_1_vuex__["a" /* default */].Store({
    state: {
        pendingHandle: null,
        pendingProgress: false,
        config: config,
        exportUrls: {
            excel: "",
            csv: "",
            tab: ""
        }
    },
    actions: {
        createInterview: function createInterview(_ref, assignmentId) {
            var rootState = _ref.rootState,
                dispatch = _ref.dispatch,
                commit = _ref.commit;

            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, function (response) {
                dispatch("showProgress", true);
                window.location = response;
            }).catch(function (data) {
                new __WEBPACK_IMPORTED_MODULE_2_pnotify___default.a({
                    title: 'Unhandled error occurred',
                    text: data.responseStatus,
                    type: 'error'
                });
                dispatch("hideProgress");
            }).then(function () {
                return dispatch("hideProgress");
            });
        },
        showProgress: function showProgress(context) {
            context.commit('SET_PROGRESS_TIMEOUT', setTimeout(function () {
                context.commit('SET_PROGRESS', true);
            }, 750));
        },
        hideProgress: function hideProgress(context) {
            clearTimeout(context.state.pendingHandle);
            context.commit('SET_PROGRESS', false);
        },
        openInterview: function openInterview(context, interviewId) {
            context.dispatch("showProgress", true);
            window.location = context.rootState.config.interviewerHqEndpoint + "/OpenInterview/" + interviewId;
        },
        discardInterview: function discardInterview(context, _ref2) {
            var callback = _ref2.callback,
                interviewId = _ref2.interviewId;

            $.ajax({
                url: context.rootState.config.interviewerHqEndpoint + "/DiscardInterview/" + interviewId,
                type: 'DELETE',
                success: callback
            });
        },
        setExportUrls: function setExportUrls(context, urls) {
            context.commit("SET_EXPORT_URLS", urls);
        }
    },
    mutations: {
        SET_PROGRESS: function SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility;
        },
        SET_PROGRESS_TIMEOUT: function SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle;
        },
        SET_EXPORT_URLS: function SET_EXPORT_URLS(state, urls) {
            state.exportUrls.excel = urls.excel;
            state.exportUrls.csv = urls.csv;
            state.exportUrls.tab = urls.tab;
        }
    },
    getters: {
        config: function config(state) {
            return state.config;
        }
    }
});

/* harmony default export */ __webpack_exports__["a"] = (store);

/***/ }),

/***/ 344:
/***/ (function(module, exports) {

module.exports = jQuery;

/***/ }),

/***/ 345:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__node_modules_vue_loader_lib_template_compiler_index_id_data_v_73cdb59e_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_App_vue__ = __webpack_require__(346);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */
var __vue_script__ = null
/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __vue_script__,
  __WEBPACK_IMPORTED_MODULE_0__node_modules_vue_loader_lib_template_compiler_index_id_data_v_73cdb59e_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_App_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\App.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] App.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-73cdb59e", Component.options)
  } else {
    hotAPI.reload("data-v-73cdb59e", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 346:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('router-view')
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-73cdb59e", esExports)
  }
}

/***/ }),

/***/ 347:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var config = JSON.parse(window.vueApp.getAttribute('configuration'));

/*  the Plugin */
var configuration = {
    install: function install(Vue, options) {
        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$config', {
            get: function get() {
                return config;
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$config', {
            get: function get() {
                return config;
            }
        });
    }

    /*  export API  */
};/* harmony default export */ __webpack_exports__["a"] = (configuration);

/***/ }),

/***/ 348:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_vue__ = __webpack_require__(90);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vue_router__ = __webpack_require__(349);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__views_Interviewer__ = __webpack_require__(350);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__views_Reports_InterviewersAndDevices__ = __webpack_require__(357);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__views_Reports_CountDaysOfInterviewInStatus__ = __webpack_require__(360);








__WEBPACK_IMPORTED_MODULE_0_vue__["a" /* default */].use(__WEBPACK_IMPORTED_MODULE_1_vue_router__["a" /* default */]);

/* harmony default export */ __webpack_exports__["default"] = (new __WEBPACK_IMPORTED_MODULE_1_vue_router__["a" /* default */]({
    base: __WEBPACK_IMPORTED_MODULE_0_vue__["a" /* default */].$config.basePath,
    mode: "history",
    routes: [{
        path: '/Reports/InterviewersAndDevices',
        component: __WEBPACK_IMPORTED_MODULE_3__views_Reports_InterviewersAndDevices__["a" /* default */]
    }, {
        path: '/Reports/CountDaysOfInterviewInStatus', component: __WEBPACK_IMPORTED_MODULE_4__views_Reports_CountDaysOfInterviewInStatus__["a" /* default */]
    }, {
        path: '/InterviewerHq/CreateNew', component: __WEBPACK_IMPORTED_MODULE_2__views_Interviewer__["a" /* Assignments */]
    }, {
        path: '/InterviewerHq/Rejected', component: __WEBPACK_IMPORTED_MODULE_2__views_Interviewer__["b" /* Interviews */]
    }, {
        path: '/InterviewerHq/Completed', component: __WEBPACK_IMPORTED_MODULE_2__views_Interviewer__["b" /* Interviews */]
    }, {
        path: '/InterviewerHq/Started', component: __WEBPACK_IMPORTED_MODULE_2__views_Interviewer__["b" /* Interviews */]
    }]
}));

/***/ }),

/***/ 350:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__Assignments__ = __webpack_require__(351);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__Interviews__ = __webpack_require__(354);
/* harmony reexport (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return __WEBPACK_IMPORTED_MODULE_0__Assignments__["a"]; });
/* harmony reexport (binding) */ __webpack_require__.d(__webpack_exports__, "b", function() { return __WEBPACK_IMPORTED_MODULE_1__Interviews__["a"]; });





/***/ }),

/***/ 351:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Assignments_vue__ = __webpack_require__(352);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_205535a1_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Assignments_vue__ = __webpack_require__(353);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Assignments_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_205535a1_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Assignments_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\views\\Interviewer\\Assignments.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Assignments.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-205535a1", Component.options)
  } else {
    hotAPI.reload("data-v-205535a1", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 352:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//


/* harmony default export */ __webpack_exports__["a"] = ({
    computed: {
        title: function title() {
            return this.config.title;
        },
        config: function config() {
            return this.$store.state.config;
        },
        dataTable: function dataTable() {
            return this.$refs.table.table;
        },
        tableOptions: function tableOptions() {
            return {
                rowId: "id",
                deferLoading: 0,
                order: [[4, 'desc']],
                columns: this.getTableColumns(),
                ajax: {
                    url: this.config.assignmentsEndpoint,
                    type: "GET",
                    contentType: 'application/json'
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'f<"table-with-scroll"t>ip'
            };
        }
    },

    mounted: function mounted() {
        this.$refs.table.reload();
    },


    methods: {
        contextMenuItems: function contextMenuItems(_ref) {
            var _this = this;

            var rowData = _ref.rowData;

            return [{
                name: this.$t("Assignments.CreateInterview"),
                callback: function callback() {
                    return _this.$store.dispatch("createInterview", rowData.id);
                }
            }];
        },
        getTableColumns: function getTableColumns() {
            var self = this;

            var columns = [{
                data: "id",
                name: "Id",
                title: this.$t("Common.Assignment"),
                responsivePriority: 2,
                width: "5%"
            }, {
                data: "quantity",
                name: "Quantity",
                "class": "type-numeric",
                title: this.$t("Assignments.InterviewsNeeded"),
                orderable: false,
                searchable: false,
                width: "11%",
                render: function render(data, type, row) {
                    var quantity = row.quantity - row.interviewsCount;
                    if (quantity <= 0) {
                        return '<span>' + self.$t("Assignments.Unlimited") + '</span>';
                    }
                    return quantity;
                },

                defaultContent: '<span>' + this.$t("Assignments.Unlimited") + '</span>'
            }, {
                data: "questionnaireTitle",
                name: "QuestionnaireTitle",
                title: this.$t("Assignments.Questionnaire"),
                orderable: true,
                searchable: true,
                render: function render(data, type, row) {
                    return "(ver. " + row.questionnaireId.version + ") " + row.questionnaireTitle;
                }
            }, {
                data: "identifyingQuestions",
                title: this.$t("Assignments.IdentifyingQuestions"),
                class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                orderable: false,
                searchable: false,
                render: function render(data) {
                    var questionsWithTitles = _.map(data, function (question) {
                        return question.title + ": " + question.answer;
                    });
                    return _.join(questionsWithTitles, ", ");
                },

                responsivePriority: 4
            }, {
                data: "updatedAtUtc",
                name: "UpdatedAtUtc",
                title: this.$t("Assignments.UpdatedAt"),
                searchable: false,
                render: function render(data) {
                    var date = moment.utc(data);
                    return date.local().format(self.config.dateFormat);
                }
            }, {
                data: "createdAtUtc",
                name: "CreatedAtUtc",
                title: this.$t("Assignments.CreatedAt"),
                searchable: false,
                render: function render(data) {
                    var date = moment.utc(data);
                    return date.local().format(self.config.dateFormat);
                }
            }];

            return columns;
        }
    }
});

/***/ }),

/***/ 353:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('Layout', {
    attrs: {
      "title": _vm.title
    }
  }, [_c('DataTables', {
    ref: "table",
    attrs: {
      "tableOptions": _vm.tableOptions,
      "contextMenuItems": _vm.contextMenuItems
    }
  })], 1)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-205535a1", esExports)
  }
}

/***/ }),

/***/ 354:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Interviews_vue__ = __webpack_require__(355);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_7575dcf2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Interviews_vue__ = __webpack_require__(356);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_Interviews_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_7575dcf2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_Interviews_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\views\\Interviewer\\Interviews.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Interviews.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-7575dcf2", Component.options)
  } else {
    hotAPI.reload("data-v-7575dcf2", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 355:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//


/* harmony default export */ __webpack_exports__["a"] = ({
    data: function data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            assignmentId: null
        };
    },

    watch: {
        questionnaireId: function questionnaireId(value) {
            this.reload();
        },
        assignmentId: function assignmentId(value) {
            this.reload();
        }
    },
    computed: {
        statuses: function statuses() {
            return this.config.statuses;
        },
        questionnaires: function questionnaires() {
            return this.config.questionnaires;
        },
        title: function title() {
            return this.config.title;
        },
        config: function config() {
            return this.$store.state.config;
        },
        tableOptions: function tableOptions() {
            return {
                rowId: "id",
                order: [[3, 'desc']],
                deferLoading: 0,
                columns: this.getTableColumns(),
                ajax: {
                    url: this.config.allInterviews,
                    type: "GET",
                    contentType: 'application/json'
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'f<"table-with-scroll"t>ip'
            };
        }
    },

    methods: {
        reload: _.debounce(function () {
            this.$refs.table.reload();
        }, 500),

        contextMenuItems: function contextMenuItems(_ref) {
            var rowData = _ref.rowData,
                rowIndex = _ref.rowIndex;

            var menu = [];
            var self = this;

            if (rowData.status != 'Completed') {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_OpenInterview"),
                    callback: function callback() {
                        return self.$store.dispatch("openInterview", rowData.interviewId);
                    }
                });
            }

            if (rowData.canDelete) {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_DiscardInterview"),
                    callback: function callback() {
                        self.discardInterview(rowData.interviewId, rowIndex);
                    }
                });
            }

            if (rowData.status == 'Completed') {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_RestartInterview"),
                    callback: function callback() {
                        self.$refs.table.disableRow(rowIndex);
                        self.restartInterview(rowData.interviewId);
                    }
                });
            }

            return menu;
        },
        discardInterview: function discardInterview(interviewId, rowIndex) {
            var self = this;
            this.$refs.confirmDiscard.promt(function (ok) {
                if (ok) {
                    self.$refs.table.disableRow(rowIndex);
                    self.$store.dispatch("discardInterview", {
                        interviewId: interviewId,
                        callback: self.reload
                    });
                }
            });
        },
        restartInterview: function restartInterview(interviewId) {
            var _this = this;

            var self = this;

            self.$refs.confirmRestart.promt(function (ok) {
                if (ok) {
                    $.post(_this.config.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: self.restart_comment }, function (response) {
                        self.restart_comment = "";
                        self.$store.dispatch("openInterview", interviewId);
                    });
                } else {
                    self.$refs.table.reload();
                }
            });
        },
        addFilteringParams: function addFilteringParams(data) {
            data.statuses = this.statuses;

            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId;
            }

            if (this.assignmentId) {
                data.assignmentId = this.assignmentId;
            }
        },
        getTableColumns: function getTableColumns() {
            var columns = [{
                data: "key",
                name: "Key",
                title: this.$t("Common.InterviewKey"),
                orderable: true,
                searchable: true
            }, {
                data: "assignmentId",
                name: "AssignmentIdKey",
                title: this.$t("Common.Assignment"),
                orderable: false,
                searchable: false
            }, {
                data: "featuredQuestions",
                title: this.$t("Assignments.IdentifyingQuestions"),
                class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                orderable: false,
                searchable: false,
                render: function render(data) {
                    var questionsWithTitles = _.map(data, function (question) {
                        return question.question + ": " + question.answer;
                    });
                    return _.join(questionsWithTitles, ", ");
                },

                responsivePriority: 4
            }, {
                data: "lastEntryDate",
                name: "UpdateDate",
                title: this.$t("Assignments.UpdatedAt"),
                searchable: false
            }];

            return columns;
        },

        clearAssignmentFilter: function clearAssignmentFilter() {
            this.assignmentId = null;
        }
    },

    mounted: function mounted() {
        this.reload();
    }
});

/***/ }),

/***/ 356:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('Layout', {
    attrs: {
      "title": _vm.title,
      "hasFilter": true
    }
  }, [_c('Filters', {
    slot: "filters"
  }, [_c('FilterBlock', {
    attrs: {
      "title": _vm.$t('Pages.Template')
    }
  }, [_c('select', {
    directives: [{
      name: "model",
      rawName: "v-model",
      value: (_vm.questionnaireId),
      expression: "questionnaireId"
    }],
    staticClass: "selectpicker",
    on: {
      "change": function($event) {
        var $$selectedVal = Array.prototype.filter.call($event.target.options, function(o) {
          return o.selected
        }).map(function(o) {
          var val = "_value" in o ? o._value : o.value;
          return val
        });
        _vm.questionnaireId = $event.target.multiple ? $$selectedVal : $$selectedVal[0]
      }
    }
  }, [_c('option', {
    domProps: {
      "value": null
    }
  }, [_vm._v(_vm._s(_vm.$t('Common.Any')))]), _vm._v(" "), _vm._l((_vm.questionnaires), function(questionnaire) {
    return _c('option', {
      key: questionnaire.key,
      domProps: {
        "value": questionnaire.key
      }
    }, [_vm._v("\n                    " + _vm._s(questionnaire.value) + "\n                ")])
  })], 2)]), _vm._v(" "), _c('FilterBlock', {
    attrs: {
      "title": _vm.$t('Pages.Filters_Assignment')
    }
  }, [_c('div', {
    staticClass: "input-group"
  }, [_c('input', {
    directives: [{
      name: "model",
      rawName: "v-model",
      value: (_vm.assignmentId),
      expression: "assignmentId"
    }],
    staticClass: "form-control with-clear-btn",
    attrs: {
      "placeholder": _vm.$t('Common.Any'),
      "type": "text"
    },
    domProps: {
      "value": (_vm.assignmentId)
    },
    on: {
      "input": function($event) {
        if ($event.target.composing) { return; }
        _vm.assignmentId = $event.target.value
      }
    }
  }), _vm._v(" "), _c('div', {
    staticClass: "input-group-btn",
    on: {
      "click": _vm.clearAssignmentFilter
    }
  }, [_c('div', {
    staticClass: "btn btn-default"
  }, [_c('span', {
    staticClass: "glyphicon glyphicon-remove",
    attrs: {
      "aria-hidden": "true"
    }
  })])])])])], 1), _vm._v(" "), _c('DataTables', {
    ref: "table",
    attrs: {
      "tableOptions": _vm.tableOptions,
      "addParamsToRequest": _vm.addFilteringParams,
      "contextMenuItems": _vm.contextMenuItems
    }
  }), _vm._v(" "), _c('Confirm', {
    ref: "confirmRestart",
    attrs: {
      "id": "restartModal"
    },
    slot: "modals"
  }, [_vm._v("\n        " + _vm._s(_vm.$t("Pages.InterviewerHq_RestartConfirm")) + "\n        "), _c('FilterBlock', [_c('div', {
    staticClass: "form-group "
  }, [_c('div', {
    staticClass: "field"
  }, [_c('input', {
    directives: [{
      name: "model",
      rawName: "v-model",
      value: (_vm.restart_comment),
      expression: "restart_comment"
    }],
    staticClass: "form-control with-clear-btn",
    attrs: {
      "type": "text"
    },
    domProps: {
      "value": (_vm.restart_comment)
    },
    on: {
      "input": function($event) {
        if ($event.target.composing) { return; }
        _vm.restart_comment = $event.target.value
      }
    }
  })])])])], 1), _vm._v(" "), _c('Confirm', {
    ref: "confirmDiscard",
    attrs: {
      "id": "discardConfirm"
    },
    slot: "modals"
  }, [_vm._v("\n        " + _vm._s(_vm.$t("Pages.InterviewerHq_DiscardConfirm")) + "\n    ")])], 1)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-7575dcf2", esExports)
  }
}

/***/ }),

/***/ 357:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_InterviewersAndDevices_vue__ = __webpack_require__(358);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_b0a3a1a2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_InterviewersAndDevices_vue__ = __webpack_require__(359);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_InterviewersAndDevices_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_b0a3a1a2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_InterviewersAndDevices_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\views\\Reports\\InterviewersAndDevices.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] InterviewersAndDevices.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-b0a3a1a2", Component.options)
  } else {
    hotAPI.reload("data-v-b0a3a1a2", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 358:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    mounted: function mounted() {
        this.$refs.table.reload();
    },

    computed: {
        tableOptions: function tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                columns: [{
                    data: "teamName",
                    name: "TeamName",
                    title: this.$t("DevicesInterviewers.Teams"),
                    orderable: true
                }, {
                    data: "neverSynchedCount",
                    name: "NeverSynchedCount",
                    "class": "type-numeric",
                    title: this.$t("DevicesInterviewers.NeverSynchronized"),
                    orderable: true,
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=NeverSynchonized'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "noQuestionnairesCount",
                    name: "NoQuestionnairesCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.NoAssignments"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=NoAssignmentsReceived'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "neverUploadedCount",
                    name: "NeverUploadedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.NeverUploaded"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=NeverUploaded'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "reassignedCount",
                    name: "ReassignedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.TabletReassigned"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=TabletReassigned'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "outdatedCount",
                    name: "OutdatedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=OutdatedApp'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "oldAndroidCount",
                    name: "OldAndroidCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.OldAndroidVersion"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=OldAndroid'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "wrongDateOnTabletCount",
                    name: "WrongDateOnTabletCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.WrongDateOnTablet"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=WrongTime'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "lowStorageCount",
                    name: "LowStorageCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.LowStorage"),
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=LowStorage'>" + data + "</a>";
                        }
                    }
                }],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip'
            };
        }
    }

});

/***/ }),

/***/ 359:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('Layout', {
    attrs: {
      "title": _vm.$t('Pages.InterviewersAndDevicesTitle')
    }
  }, [_c('ExportButtons', {
    slot: "exportButtons"
  }), _vm._v(" "), _c('DataTables', {
    ref: "table",
    attrs: {
      "tableOptions": _vm.tableOptions
    }
  })], 1)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-b0a3a1a2", esExports)
  }
}

/***/ }),

/***/ 360:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_CountDaysOfInterviewInStatus_vue__ = __webpack_require__(361);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_708db3c2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_CountDaysOfInterviewInStatus_vue__ = __webpack_require__(362);
var disposed = false
var normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var __vue_styles__ = null
/* scopeId */
var __vue_scopeId__ = null
/* moduleIdentifier (server only) */
var __vue_module_identifier__ = null
var Component = normalizeComponent(
  __WEBPACK_IMPORTED_MODULE_0__babel_loader_node_modules_vue_loader_lib_selector_type_script_index_0_CountDaysOfInterviewInStatus_vue__["a" /* default */],
  __WEBPACK_IMPORTED_MODULE_1__node_modules_vue_loader_lib_template_compiler_index_id_data_v_708db3c2_hasScoped_false_node_modules_vue_loader_lib_selector_type_template_index_0_CountDaysOfInterviewInStatus_vue__["a" /* default */],
  __vue_styles__,
  __vue_scopeId__,
  __vue_module_identifier__
)
Component.options.__file = "app\\views\\Reports\\CountDaysOfInterviewInStatus.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] CountDaysOfInterviewInStatus.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-708db3c2", Component.options)
  } else {
    hotAPI.reload("data-v-708db3c2", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

/* harmony default export */ __webpack_exports__["a"] = (Component.exports);


/***/ }),

/***/ 361:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//

/* harmony default export */ __webpack_exports__["a"] = ({
    data: function data() {
        return {
            questionnaireId: null
        };
    },

    watch: {
        questionnaireId: function questionnaireId(value) {
            this.reload();
        }
    },
    mounted: function mounted() {
        this.$refs.table.reload();
    },

    computed: {
        questionnaires: function questionnaires() {
            return this.$config.questionnaires;
        },
        tableOptions: function tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                columns: [{
                    data: "daysCountStart",
                    title: this.$t("Strings.Days"),
                    orderable: true,
                    render: function render(data, type, row) {
                        if (row.daysCountStart === row.daysCountEnd) return "<span>" + data + "</span>";else if (row.daysCountEnd == undefined) return "<span>" + data + "&#43;</span>";else return "<span>" + row.daysCountStart + "-" + row.daysCountEnd + "</span>";
                    }
                }, {
                    data: "supervisorAssignedCount",
                    title: this.$t("Strings.InterviewStatus_SupervisorAssigned"),
                    orderable: false,
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.assignmentsBaseUrl + "?dateStart=" + row.startDate + "&dateEnd=" + row.endDate + "&questionnaire=" + self.questionnaireId + "&userRole=Supervisor'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "interviewerAssignedCount",
                    title: this.$t("Strings.InterviewStatus_InterviewerAssigned"),
                    orderable: false,
                    render: function render(data, type, row) {
                        if (data === 0) return "<span>" + data + "</span>";else {
                            return "<a href='" + self.$config.assignmentsBaseUrl + "?dateStart=" + row.startDate + "&dateEnd=" + row.endDate + "&questionnaire=" + self.questionnaireId + "&userRole=Interviewer'>" + data + "</a>";
                        }
                    }
                }, {
                    data: "completedCount",
                    title: this.$t("Strings.InterviewStatus_Completed"),
                    orderable: false,
                    render: function render(data, type, row) {
                        return self.renderInterviewsUrl(row, data, 'Completed');
                    }
                }, {
                    data: "rejectedBySupervisorCount",
                    title: this.$t("Strings.InterviewStatus_RejectedBySupervisor"),
                    orderable: false,
                    render: function render(data, type, row) {
                        return self.renderInterviewsUrl(row, data, 'RejectedBySupervisor');
                    }
                }, {
                    data: "approvedBySupervisorCount",
                    title: this.$t("Strings.InterviewStatus_ApprovedBySupervisor"),
                    orderable: false,
                    render: function render(data, type, row) {
                        return self.renderInterviewsUrl(row, data, 'ApprovedBySupervisor');
                    }
                }, {
                    data: "rejectedByHeadquartersCount",
                    title: this.$t("Strings.InterviewStatus_RejectedByHeadquarters"),
                    orderable: false,
                    render: function render(data, type, row) {
                        return self.renderInterviewsUrl(row, data, 'RejectedByHeadquarters');
                    }
                }, {
                    data: "approvedByHeadquartersCount",
                    title: this.$t("Strings.InterviewStatus_ApprovedByHeadquarters"),
                    orderable: false,
                    render: function render(data, type, row) {
                        return self.renderInterviewsUrl(row, data, 'ApprovedByHeadquarters');
                    }
                }],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[0, "desc"]],
                bInfo: false,
                footer: true,
                footerCallback: function footerCallback(row, data, start, end, display) {
                    var api = this.api(),
                        data;
                    var colNumber = [1, 2, 3, 4, 5, 6, 7];

                    for (var i = 0; i < colNumber.length; i++) {
                        var colNo = colNumber[i];
                        var total = api.column(colNo, { page: 'current' }).data().reduce(function (a, b) {
                            return a + b;
                        }, 0);
                        $(api.column(colNo).footer()).html(total);
                    }
                }
            };
        }
    },
    methods: {
        reload: _.debounce(function () {
            this.$refs.table.reload();
        }, 500),

        addFilteringParams: function addFilteringParams(data) {
            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId;
            }
        },
        renderInterviewsUrl: function renderInterviewsUrl(row, data, status) {
            if (data === 0) return "<span>" + data + "</span>";
            if (row.startDate == undefined) return "<a href='" + this.$config.interviewsBaseUrl + "?unactiveDateEnd=" + row.endDate + "&status=" + status + "'>" + data + "</a>";
            if (row.endDate == undefined) return "<a href='" + this.$config.interviewsBaseUrl + "?unactiveDateStart=" + row.startDate + "&status=" + status + "'>" + data + "</a>";

            return "<a href='" + this.$config.interviewsBaseUrl + "?unactiveDateStart=" + row.startDate + "&unactiveDateEnd=" + row.endDate + "&status=" + status + "'>" + data + "</a>";
        }
    }
});

/***/ }),

/***/ 362:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
var render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('Layout', {
    attrs: {
      "hasFilter": true,
      "title": _vm.$t('Pages.CountDaysOfInterviewInStatus'),
      "subtitle": _vm.$t('Pages.CountDaysOfInterviewInStatusDescription')
    }
  }, [_c('Filters', {
    slot: "filters"
  }, [_c('FilterBlock', {
    attrs: {
      "title": _vm.$t('Pages.Template')
    }
  }, [_c('select', {
    directives: [{
      name: "model",
      rawName: "v-model",
      value: (_vm.questionnaireId),
      expression: "questionnaireId"
    }],
    staticClass: "selectpicker",
    on: {
      "change": function($event) {
        var $$selectedVal = Array.prototype.filter.call($event.target.options, function(o) {
          return o.selected
        }).map(function(o) {
          var val = "_value" in o ? o._value : o.value;
          return val
        });
        _vm.questionnaireId = $event.target.multiple ? $$selectedVal : $$selectedVal[0]
      }
    }
  }, [_c('option', {
    domProps: {
      "value": null
    }
  }, [_vm._v(_vm._s(_vm.$t('Common.Any')))]), _vm._v(" "), _vm._l((_vm.questionnaires), function(questionnaire) {
    return _c('option', {
      key: questionnaire.key,
      domProps: {
        "value": questionnaire.key
      }
    }, [_vm._v("\n                    " + _vm._s(questionnaire.value) + "\n                ")])
  })], 2)])], 1), _vm._v(" "), _c('ExportButtons', {
    slot: "exportButtons"
  }), _vm._v(" "), _c('DataTables', {
    ref: "table",
    attrs: {
      "tableOptions": _vm.tableOptions,
      "addParamsToRequest": _vm.addFilteringParams,
      "hasPaging": false,
      "hasSearch": false
    }
  })], 1)
}
var staticRenderFns = []
render._withStripped = true
var esExports = { render: render, staticRenderFns: staticRenderFns }
/* harmony default export */ __webpack_exports__["a"] = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-708db3c2", esExports)
  }
}

/***/ })

},[122]);
//# sourceMappingURL=app.bundle.js.map
webpackJsonp([0],{

/***/ 117:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });

// EXTERNAL MODULE: ./node_modules/babel-polyfill/lib/index.js
var lib = __webpack_require__(118);
var lib_default = /*#__PURE__*/__webpack_require__.n(lib);

// EXTERNAL MODULE: ./node_modules/vue/dist/vue.esm.js
var vue_esm = __webpack_require__(87);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/ExportButtons.vue
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

/* harmony default export */ var ExportButtons = ({
    props: {
        table: {
            type: Object,
            reqiured: true
        }
    }
});
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-ce5b43f2","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/ExportButtons.vue
var ExportButtons_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
ExportButtons_render._withStripped = true
var esExports = { render: ExportButtons_render, staticRenderFns: staticRenderFns }
/* harmony default export */ var components_ExportButtons = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-ce5b43f2", esExports)
  }
}
// CONCATENATED MODULE: ./app/components/ExportButtons.vue
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
  ExportButtons,
  components_ExportButtons,
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

/* harmony default export */ var app_components_ExportButtons = (Component.exports);

// EXTERNAL MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/Typeahead.vue
var Typeahead = __webpack_require__(301);
var Typeahead_default = /*#__PURE__*/__webpack_require__.n(Typeahead);

// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-663fdba1","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/Typeahead.vue
var Typeahead_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var Typeahead_staticRenderFns = []
Typeahead_render._withStripped = true
var Typeahead_esExports = { render: Typeahead_render, staticRenderFns: Typeahead_staticRenderFns }
/* harmony default export */ var components_Typeahead = (Typeahead_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-663fdba1", Typeahead_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/Typeahead.vue
var Typeahead_disposed = false
var Typeahead_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var Typeahead___vue_styles__ = null
/* scopeId */
var Typeahead___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var Typeahead___vue_module_identifier__ = null
var Typeahead_Component = Typeahead_normalizeComponent(
  Typeahead_default.a,
  components_Typeahead,
  Typeahead___vue_styles__,
  Typeahead___vue_scopeId__,
  Typeahead___vue_module_identifier__
)
Typeahead_Component.options.__file = "app\\components\\Typeahead.vue"
if (Typeahead_Component.esModule && Object.keys(Typeahead_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Typeahead_Component.options.functional) {console.error("[vue-loader] Typeahead.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-663fdba1", Typeahead_Component.options)
  } else {
    hotAPI.reload("data-v-663fdba1", Typeahead_Component.options)
  }
  module.hot.dispose(function (data) {
    Typeahead_disposed = true
  })
})()}

/* harmony default export */ var app_components_Typeahead = (Typeahead_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/Layout.vue
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

/* harmony default export */ var Layout = ({
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-00d5e640","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/Layout.vue
var Layout_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('main', [_c('div', {
    staticClass: "container-fluid"
  }, [_c('div', {
    staticClass: "row"
  }, [_vm._t("filters"), _vm._v(" "), _c('div', {
    class: _vm.information
  }, [_c('div', {
    staticClass: "page-header clearfix"
  }, [_c('h1', [_vm._v("\n                        " + _vm._s(_vm.title) + "\n                    ")]), _vm._v(" "), (_vm.subtitle) ? _c('h3', [_vm._v(_vm._s(_vm.subtitle))]) : _vm._e(), _vm._v(" "), _vm._t("exportButtons")], 2), _vm._v(" "), _vm._t("default")], 2)], 2), _vm._v(" "), _vm._t("modals"), _vm._v(" "), _c('ModalFrame', {
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
var Layout_staticRenderFns = []
Layout_render._withStripped = true
var Layout_esExports = { render: Layout_render, staticRenderFns: Layout_staticRenderFns }
/* harmony default export */ var components_Layout = (Layout_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-00d5e640", Layout_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/Layout.vue
var Layout_disposed = false
var Layout_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var Layout___vue_styles__ = null
/* scopeId */
var Layout___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var Layout___vue_module_identifier__ = null
var Layout_Component = Layout_normalizeComponent(
  Layout,
  components_Layout,
  Layout___vue_styles__,
  Layout___vue_scopeId__,
  Layout___vue_module_identifier__
)
Layout_Component.options.__file = "app\\components\\Layout.vue"
if (Layout_Component.esModule && Object.keys(Layout_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Layout_Component.options.functional) {console.error("[vue-loader] Layout.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-00d5e640", Layout_Component.options)
  } else {
    hotAPI.reload("data-v-00d5e640", Layout_Component.options)
  }
  module.hot.dispose(function (data) {
    Layout_disposed = true
  })
})()}

/* harmony default export */ var app_components_Layout = (Layout_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/Filters.vue
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

/* harmony default export */ var Filters = ({});
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-70a0b355","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/Filters.vue
var Filters_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('aside', {
    staticClass: "filters"
  }, [_vm._m(0), _vm._v(" "), _c('div', {
    staticClass: "filters-container"
  }, [_c('h4', [_vm._v(_vm._s(_vm.$t('Pages.FilterTitle')))]), _vm._v(" "), _vm._t("default")], 2)])
}
var Filters_staticRenderFns = [function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
Filters_render._withStripped = true
var Filters_esExports = { render: Filters_render, staticRenderFns: Filters_staticRenderFns }
/* harmony default export */ var components_Filters = (Filters_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-70a0b355", Filters_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/Filters.vue
var Filters_disposed = false
var Filters_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var Filters___vue_styles__ = null
/* scopeId */
var Filters___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var Filters___vue_module_identifier__ = null
var Filters_Component = Filters_normalizeComponent(
  Filters,
  components_Filters,
  Filters___vue_styles__,
  Filters___vue_scopeId__,
  Filters___vue_module_identifier__
)
Filters_Component.options.__file = "app\\components\\Filters.vue"
if (Filters_Component.esModule && Object.keys(Filters_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Filters_Component.options.functional) {console.error("[vue-loader] Filters.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-70a0b355", Filters_Component.options)
  } else {
    hotAPI.reload("data-v-70a0b355", Filters_Component.options)
  }
  module.hot.dispose(function (data) {
    Filters_disposed = true
  })
})()}

/* harmony default export */ var app_components_Filters = (Filters_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/FilterBlock.vue
//
//
//
//
//
//
//

/* harmony default export */ var FilterBlock = ({
    props: ["title"]
});
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-3695054f","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/FilterBlock.vue
var FilterBlock_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "block-filter"
  }, [_c('h5', [_vm._v(_vm._s(_vm.title))]), _vm._v(" "), _vm._t("default")], 2)
}
var FilterBlock_staticRenderFns = []
FilterBlock_render._withStripped = true
var FilterBlock_esExports = { render: FilterBlock_render, staticRenderFns: FilterBlock_staticRenderFns }
/* harmony default export */ var components_FilterBlock = (FilterBlock_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-3695054f", FilterBlock_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/FilterBlock.vue
var FilterBlock_disposed = false
var FilterBlock_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var FilterBlock___vue_styles__ = null
/* scopeId */
var FilterBlock___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var FilterBlock___vue_module_identifier__ = null
var FilterBlock_Component = FilterBlock_normalizeComponent(
  FilterBlock,
  components_FilterBlock,
  FilterBlock___vue_styles__,
  FilterBlock___vue_scopeId__,
  FilterBlock___vue_module_identifier__
)
FilterBlock_Component.options.__file = "app\\components\\FilterBlock.vue"
if (FilterBlock_Component.esModule && Object.keys(FilterBlock_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (FilterBlock_Component.options.functional) {console.error("[vue-loader] FilterBlock.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-3695054f", FilterBlock_Component.options)
  } else {
    hotAPI.reload("data-v-3695054f", FilterBlock_Component.options)
  }
  module.hot.dispose(function (data) {
    FilterBlock_disposed = true
  })
})()}

/* harmony default export */ var app_components_FilterBlock = (FilterBlock_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/DataTables.vue
//
//
//
//
//
//
//


/* harmony default export */ var DataTables = ({

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
                build: function build($trigger) {
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-50d1e445","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/DataTables.vue
var DataTables_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _vm._m(0)
}
var DataTables_staticRenderFns = [function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('table', {
    staticClass: "table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive"
  }, [_c('thead'), _vm._v(" "), _c('tbody')])
}]
DataTables_render._withStripped = true
var DataTables_esExports = { render: DataTables_render, staticRenderFns: DataTables_staticRenderFns }
/* harmony default export */ var components_DataTables = (DataTables_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-50d1e445", DataTables_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/DataTables.vue
var DataTables_disposed = false
var DataTables_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var DataTables___vue_styles__ = null
/* scopeId */
var DataTables___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var DataTables___vue_module_identifier__ = null
var DataTables_Component = DataTables_normalizeComponent(
  DataTables,
  components_DataTables,
  DataTables___vue_styles__,
  DataTables___vue_scopeId__,
  DataTables___vue_module_identifier__
)
DataTables_Component.options.__file = "app\\components\\DataTables.vue"
if (DataTables_Component.esModule && Object.keys(DataTables_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (DataTables_Component.options.functional) {console.error("[vue-loader] DataTables.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-50d1e445", DataTables_Component.options)
  } else {
    hotAPI.reload("data-v-50d1e445", DataTables_Component.options)
  }
  module.hot.dispose(function (data) {
    DataTables_disposed = true
  })
})()}

/* harmony default export */ var app_components_DataTables = (DataTables_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/ModalFrame.vue
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

/* harmony default export */ var ModalFrame = ({
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-5a68a3d6","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/ModalFrame.vue
var ModalFrame_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var ModalFrame_staticRenderFns = []
ModalFrame_render._withStripped = true
var ModalFrame_esExports = { render: ModalFrame_render, staticRenderFns: ModalFrame_staticRenderFns }
/* harmony default export */ var components_ModalFrame = (ModalFrame_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-5a68a3d6", ModalFrame_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/ModalFrame.vue
var ModalFrame_disposed = false
var ModalFrame_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var ModalFrame___vue_styles__ = null
/* scopeId */
var ModalFrame___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var ModalFrame___vue_module_identifier__ = null
var ModalFrame_Component = ModalFrame_normalizeComponent(
  ModalFrame,
  components_ModalFrame,
  ModalFrame___vue_styles__,
  ModalFrame___vue_scopeId__,
  ModalFrame___vue_module_identifier__
)
ModalFrame_Component.options.__file = "app\\components\\ModalFrame.vue"
if (ModalFrame_Component.esModule && Object.keys(ModalFrame_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (ModalFrame_Component.options.functional) {console.error("[vue-loader] ModalFrame.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-5a68a3d6", ModalFrame_Component.options)
  } else {
    hotAPI.reload("data-v-5a68a3d6", ModalFrame_Component.options)
  }
  module.hot.dispose(function (data) {
    ModalFrame_disposed = true
  })
})()}

/* harmony default export */ var app_components_ModalFrame = (ModalFrame_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/components/Confirm.vue
//
//
//
//
//
//
//
//

/* harmony default export */ var Confirm = ({
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-8dafe8cc","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/components/Confirm.vue
var Confirm_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var Confirm_staticRenderFns = []
Confirm_render._withStripped = true
var Confirm_esExports = { render: Confirm_render, staticRenderFns: Confirm_staticRenderFns }
/* harmony default export */ var components_Confirm = (Confirm_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-8dafe8cc", Confirm_esExports)
  }
}
// CONCATENATED MODULE: ./app/components/Confirm.vue
var Confirm_disposed = false
var Confirm_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var Confirm___vue_styles__ = null
/* scopeId */
var Confirm___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var Confirm___vue_module_identifier__ = null
var Confirm_Component = Confirm_normalizeComponent(
  Confirm,
  components_Confirm,
  Confirm___vue_styles__,
  Confirm___vue_scopeId__,
  Confirm___vue_module_identifier__
)
Confirm_Component.options.__file = "app\\components\\Confirm.vue"
if (Confirm_Component.esModule && Object.keys(Confirm_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Confirm_Component.options.functional) {console.error("[vue-loader] Confirm.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-8dafe8cc", Confirm_Component.options)
  } else {
    hotAPI.reload("data-v-8dafe8cc", Confirm_Component.options)
  }
  module.hot.dispose(function (data) {
    Confirm_disposed = true
  })
})()}

/* harmony default export */ var app_components_Confirm = (Confirm_Component.exports);

// EXTERNAL MODULE: ./node_modules/i18next/dist/es/index.js + 14 modules
var es = __webpack_require__(302);

// CONCATENATED MODULE: ./app/plugins/locale.js


/*  the Plugin */
var VueI18Next = {
    install: function install(Vue, options) {
        /*  determine options  */
        es["a" /* default */].init(Object.assign({
            fallbackLng: 'en'
        }, options));

        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$t', {
            get: function get() {
                return function (key, options) {
                    //var opts = { resources: locale }

                    // for now we will not support language change on the fly
                    //Vue.util.extend(opts, options)
                    return es["a" /* default */].t(key, options);
                };
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$t', {
            get: function get() {
                return function (key, options) {
                    //var opts = { resources: locale }
                    //Vue.util.extend(opts, options)
                    return es["a" /* default */].t(key, options);
                };
            }
        });
    }

    /*  export API  */
};/* harmony default export */ var locale = (VueI18Next);
// EXTERNAL MODULE: ./node_modules/vuex/dist/vuex.esm.js
var vuex_esm = __webpack_require__(303);

// EXTERNAL MODULE: ./node_modules/pnotify/dist/pnotify.js
var pnotify = __webpack_require__(304);
var pnotify_default = /*#__PURE__*/__webpack_require__.n(pnotify);

// CONCATENATED MODULE: ./app/store.js



// PNotify.prototype.options.styling = "bootstrap3"

vue_esm["a" /* default */].use(vuex_esm["a" /* default */]);

var config = JSON.parse(window.vueApp.getAttribute('configuration'));

var store = new vuex_esm["a" /* default */].Store({
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
                dispatch = _ref.dispatch;

            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, function (response) {
                dispatch("showProgress", true);
                window.location = response;
            }).catch(function (data) {
                new pnotify_default.a({
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

/* harmony default export */ var app_store = (store);
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-73cdb59e","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/App.vue
var App_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('router-view')
}
var App_staticRenderFns = []
App_render._withStripped = true
var App_esExports = { render: App_render, staticRenderFns: App_staticRenderFns }
/* harmony default export */ var App = (App_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-73cdb59e", App_esExports)
  }
}
// CONCATENATED MODULE: ./app/App.vue
var App_disposed = false
var App_normalizeComponent = __webpack_require__(16)
/* script */
var __vue_script__ = null
/* template */

/* styles */
var App___vue_styles__ = null
/* scopeId */
var App___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var App___vue_module_identifier__ = null
var App_Component = App_normalizeComponent(
  __vue_script__,
  App,
  App___vue_styles__,
  App___vue_scopeId__,
  App___vue_module_identifier__
)
App_Component.options.__file = "app\\App.vue"
if (App_Component.esModule && Object.keys(App_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (App_Component.options.functional) {console.error("[vue-loader] App.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-73cdb59e", App_Component.options)
  } else {
    hotAPI.reload("data-v-73cdb59e", App_Component.options)
  }
  module.hot.dispose(function (data) {
    App_disposed = true
  })
})()}

/* harmony default export */ var app_App = (App_Component.exports);

// CONCATENATED MODULE: ./app/config.js
var config_config = JSON.parse(window.vueApp.getAttribute('configuration'));

/*  the Plugin */
var configuration = {
    install: function install(Vue) {
        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$config', {
            get: function get() {
                return config_config;
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$config', {
            get: function get() {
                return config_config;
            }
        });
    }

    /*  export API  */
};/* harmony default export */ var app_config = (configuration);
// CONCATENATED MODULE: ./app/main.js
















vue_esm["a" /* default */].use(app_config);

vue_esm["a" /* default */].use(locale, {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': vue_esm["a" /* default */].$config.resources
    }
});

vue_esm["a" /* default */].component("Layout", app_components_Layout);
vue_esm["a" /* default */].component("Filters", app_components_Filters);
vue_esm["a" /* default */].component("FilterBlock", app_components_FilterBlock);
vue_esm["a" /* default */].component("Typeahead", app_components_Typeahead);
vue_esm["a" /* default */].component("DataTables", app_components_DataTables);
vue_esm["a" /* default */].component("ModalFrame", app_components_ModalFrame);
vue_esm["a" /* default */].component("Confirm", app_components_Confirm);
vue_esm["a" /* default */].component("ExportButtons", app_components_ExportButtons);

var router = __webpack_require__(306).default;

new vue_esm["a" /* default */]({
    el: "#vueApp",
    render: function render(h) {
        return h(app_App);
    },
    components: { App: app_App },
    store: app_store,
    router: router
});

/***/ }),

/***/ 301:
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
        onSearchBoxDownKey: function onSearchBoxDownKey() {
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
                return encodedTitle.replace(iQuery, function (matchedTxt) {
                    return '<strong>' + matchedTxt + '</strong>';
                });
            }

            return encodedTitle;
        }
    }
};

/***/ }),

/***/ 305:
/***/ (function(module, exports) {

module.exports = jQuery;

/***/ }),

/***/ 306:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });

// EXTERNAL MODULE: ./node_modules/vue/dist/vue.esm.js
var vue_esm = __webpack_require__(87);

// EXTERNAL MODULE: ./node_modules/vue-router/dist/vue-router.esm.js
var vue_router_esm = __webpack_require__(307);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/views/Interviewer/Assignments.vue
//
//
//
//
//
//


/* harmony default export */ var Assignments = ({
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-205535a1","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/views/Interviewer/Assignments.vue
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
/* harmony default export */ var Interviewer_Assignments = (esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-205535a1", esExports)
  }
}
// CONCATENATED MODULE: ./app/views/Interviewer/Assignments.vue
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
  Assignments,
  Interviewer_Assignments,
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

/* harmony default export */ var views_Interviewer_Assignments = (Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/views/Interviewer/Interviews.vue
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


/* harmony default export */ var Interviews = ({
    data: function data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            assignmentId: null
        };
    },

    watch: {
        questionnaireId: function questionnaireId() {
            this.reload();
        },
        assignmentId: function assignmentId() {
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
                    $.post(_this.config.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: self.restart_comment }, function () {
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-7575dcf2","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/views/Interviewer/Interviews.vue
var Interviews_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var Interviews_staticRenderFns = []
Interviews_render._withStripped = true
var Interviews_esExports = { render: Interviews_render, staticRenderFns: Interviews_staticRenderFns }
/* harmony default export */ var Interviewer_Interviews = (Interviews_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-7575dcf2", Interviews_esExports)
  }
}
// CONCATENATED MODULE: ./app/views/Interviewer/Interviews.vue
var Interviews_disposed = false
var Interviews_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var Interviews___vue_styles__ = null
/* scopeId */
var Interviews___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var Interviews___vue_module_identifier__ = null
var Interviews_Component = Interviews_normalizeComponent(
  Interviews,
  Interviewer_Interviews,
  Interviews___vue_styles__,
  Interviews___vue_scopeId__,
  Interviews___vue_module_identifier__
)
Interviews_Component.options.__file = "app\\views\\Interviewer\\Interviews.vue"
if (Interviews_Component.esModule && Object.keys(Interviews_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Interviews_Component.options.functional) {console.error("[vue-loader] Interviews.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-7575dcf2", Interviews_Component.options)
  } else {
    hotAPI.reload("data-v-7575dcf2", Interviews_Component.options)
  }
  module.hot.dispose(function (data) {
    Interviews_disposed = true
  })
})()}

/* harmony default export */ var views_Interviewer_Interviews = (Interviews_Component.exports);

// CONCATENATED MODULE: ./app/views/Interviewer/index.js




// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/views/Reports/InterviewersAndDevices.vue
//
//
//
//
//
//
//

/* harmony default export */ var InterviewersAndDevices = ({
    mounted: function mounted() {
        this.$refs.table.reload();
    },

    methods: {
        renderCell: function renderCell(data, row, facet) {
            if (data === 0) {
                return "<span>" + data + "</span>";
            }
            if (row.teamId === '00000000-0000-0000-0000-000000000000') {
                return "<a href='" + this.$config.interviewersBaseUrl + "?Facet=" + facet + "'>" + data + "</a>";
            } else {
                return "<a href='" + this.$config.interviewersBaseUrl + "?supervisor=" + row.teamName + "&Facet=" + facet + "'>" + data + "</a>";
            }
        }
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
                        return self.renderCell(data, row, 'NeverSynchonized');
                    }
                }, {
                    data: "noQuestionnairesCount",
                    name: "NoQuestionnairesCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.NoAssignments"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'NoAssignmentsReceived');
                    }
                }, {
                    data: "neverUploadedCount",
                    name: "NeverUploadedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.NeverUploaded"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'NeverUploaded');
                    }
                }, {
                    data: "reassignedCount",
                    name: "ReassignedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.TabletReassigned"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'TabletReassigned');
                    }
                }, {
                    data: "outdatedCount",
                    name: "OutdatedCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'OutdatedApp');
                    }
                }, {
                    data: "oldAndroidCount",
                    name: "OldAndroidCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.OldAndroidVersion"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'OldAndroid');
                    }
                }, {
                    data: "wrongDateOnTabletCount",
                    name: "WrongDateOnTabletCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.WrongDateOnTablet"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'WrongTime');
                    }
                }, {
                    data: "lowStorageCount",
                    name: "LowStorageCount",
                    "class": "type-numeric",
                    orderable: true,
                    title: this.$t("DevicesInterviewers.LowStorage"),
                    render: function render(data, type, row) {
                        return self.renderCell(data, row, 'LowStorage');
                    }
                }],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
                createdRow: function createdRow(row, data, dataIndex) {
                    if (dataIndex === 0) {
                        $(row).addClass('total-row');
                    }
                }
            };
        }
    }

});
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-b0a3a1a2","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/views/Reports/InterviewersAndDevices.vue
var InterviewersAndDevices_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var InterviewersAndDevices_staticRenderFns = []
InterviewersAndDevices_render._withStripped = true
var InterviewersAndDevices_esExports = { render: InterviewersAndDevices_render, staticRenderFns: InterviewersAndDevices_staticRenderFns }
/* harmony default export */ var Reports_InterviewersAndDevices = (InterviewersAndDevices_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-b0a3a1a2", InterviewersAndDevices_esExports)
  }
}
// CONCATENATED MODULE: ./app/views/Reports/InterviewersAndDevices.vue
var InterviewersAndDevices_disposed = false
var InterviewersAndDevices_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var InterviewersAndDevices___vue_styles__ = null
/* scopeId */
var InterviewersAndDevices___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var InterviewersAndDevices___vue_module_identifier__ = null
var InterviewersAndDevices_Component = InterviewersAndDevices_normalizeComponent(
  InterviewersAndDevices,
  Reports_InterviewersAndDevices,
  InterviewersAndDevices___vue_styles__,
  InterviewersAndDevices___vue_scopeId__,
  InterviewersAndDevices___vue_module_identifier__
)
InterviewersAndDevices_Component.options.__file = "app\\views\\Reports\\InterviewersAndDevices.vue"
if (InterviewersAndDevices_Component.esModule && Object.keys(InterviewersAndDevices_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (InterviewersAndDevices_Component.options.functional) {console.error("[vue-loader] InterviewersAndDevices.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-b0a3a1a2", InterviewersAndDevices_Component.options)
  } else {
    hotAPI.reload("data-v-b0a3a1a2", InterviewersAndDevices_Component.options)
  }
  module.hot.dispose(function (data) {
    InterviewersAndDevices_disposed = true
  })
})()}

/* harmony default export */ var views_Reports_InterviewersAndDevices = (InterviewersAndDevices_Component.exports);

// CONCATENATED MODULE: ./node_modules/babel-loader/lib!./node_modules/vue-loader/lib/selector.js?type=script&index=0!./app/views/Reports/CountDaysOfInterviewInStatus.vue
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

/* harmony default export */ var CountDaysOfInterviewInStatus = ({
    data: function data() {
        return {
            questionnaireId: null
        };
    },

    watch: {
        questionnaireId: function questionnaireId() {
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
                }, {
                    data: "daysCountStart",
                    title: this.$t("Strings.Total"),
                    orderable: false,
                    render: function render(data, type, row) {
                        var total = row.supervisorAssignedCount + row.interviewerAssignedCount + row.completedCount + row.rejectedBySupervisorCount + row.approvedBySupervisorCount + row.rejectedByHeadquartersCount + row.approvedByHeadquartersCount;
                        return "<span>" + total + "</span>";
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
                footerCallback: function footerCallback() {
                    //if ($(this).find("tfoot").length == 0)
                    //    $(this).append("<tfoot><tr><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr></tfoot>")

                    var api = this.api();
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
// CONCATENATED MODULE: ./node_modules/vue-loader/lib/template-compiler?{"id":"data-v-708db3c2","hasScoped":false}!./node_modules/vue-loader/lib/selector.js?type=template&index=0!./app/views/Reports/CountDaysOfInterviewInStatus.vue
var CountDaysOfInterviewInStatus_render = function () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
var CountDaysOfInterviewInStatus_staticRenderFns = []
CountDaysOfInterviewInStatus_render._withStripped = true
var CountDaysOfInterviewInStatus_esExports = { render: CountDaysOfInterviewInStatus_render, staticRenderFns: CountDaysOfInterviewInStatus_staticRenderFns }
/* harmony default export */ var Reports_CountDaysOfInterviewInStatus = (CountDaysOfInterviewInStatus_esExports);
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-708db3c2", CountDaysOfInterviewInStatus_esExports)
  }
}
// CONCATENATED MODULE: ./app/views/Reports/CountDaysOfInterviewInStatus.vue
var CountDaysOfInterviewInStatus_disposed = false
var CountDaysOfInterviewInStatus_normalizeComponent = __webpack_require__(16)
/* script */

/* template */

/* styles */
var CountDaysOfInterviewInStatus___vue_styles__ = null
/* scopeId */
var CountDaysOfInterviewInStatus___vue_scopeId__ = null
/* moduleIdentifier (server only) */
var CountDaysOfInterviewInStatus___vue_module_identifier__ = null
var CountDaysOfInterviewInStatus_Component = CountDaysOfInterviewInStatus_normalizeComponent(
  CountDaysOfInterviewInStatus,
  Reports_CountDaysOfInterviewInStatus,
  CountDaysOfInterviewInStatus___vue_styles__,
  CountDaysOfInterviewInStatus___vue_scopeId__,
  CountDaysOfInterviewInStatus___vue_module_identifier__
)
CountDaysOfInterviewInStatus_Component.options.__file = "app\\views\\Reports\\CountDaysOfInterviewInStatus.vue"
if (CountDaysOfInterviewInStatus_Component.esModule && Object.keys(CountDaysOfInterviewInStatus_Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (CountDaysOfInterviewInStatus_Component.options.functional) {console.error("[vue-loader] CountDaysOfInterviewInStatus.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-708db3c2", CountDaysOfInterviewInStatus_Component.options)
  } else {
    hotAPI.reload("data-v-708db3c2", CountDaysOfInterviewInStatus_Component.options)
  }
  module.hot.dispose(function (data) {
    CountDaysOfInterviewInStatus_disposed = true
  })
})()}

/* harmony default export */ var views_Reports_CountDaysOfInterviewInStatus = (CountDaysOfInterviewInStatus_Component.exports);

// CONCATENATED MODULE: ./app/router.js








vue_esm["a" /* default */].use(vue_router_esm["a" /* default */]);

/* harmony default export */ var router = __webpack_exports__["default"] = (new vue_router_esm["a" /* default */]({
    base: vue_esm["a" /* default */].$config.basePath,
    mode: "history",
    routes: [{
        path: '/Reports/InterviewersAndDevices',
        component: views_Reports_InterviewersAndDevices
    }, {
        path: '/Reports/CountDaysOfInterviewInStatus', component: views_Reports_CountDaysOfInterviewInStatus
    }, {
        path: '/InterviewerHq/CreateNew', component: views_Interviewer_Assignments
    }, {
        path: '/InterviewerHq/Rejected', component: views_Interviewer_Interviews
    }, {
        path: '/InterviewerHq/Completed', component: views_Interviewer_Interviews
    }, {
        path: '/InterviewerHq/Started', component: views_Interviewer_Interviews
    }]
}));

/***/ })

},[117]);
//# sourceMappingURL=app.bundle.js.map
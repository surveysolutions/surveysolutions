webpackJsonp([1],{

/***/ 100:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
//
//
//
//
//
//
//

exports.default = {
    props: ["title"]
};

/***/ }),

/***/ 101:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('div', {
    staticClass: "block-filter"
  }, [_c('h5', [_vm._v(_vm._s(_vm.title))]), _vm._v(" "), _vm._t("default")], 2)
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-5c3b9966", module.exports)
  }
}

/***/ }),

/***/ 102:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(103),
  /* template */
  __webpack_require__(104),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\DataTables.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] DataTables.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-2447fe87", Component.options)
  } else {
    hotAPI.reload("data-v-2447fe87", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 103:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
//
//
//
//
//
//
//


exports.default = {

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
            type: Function,
            default: function _default() {
                return [];
            }
        },
        authorizedUser: { type: Object, default: function _default() {
                return {};
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
            conditionalPaging: true
        }, this.tableOptions);

        options.ajax.data = function (d) {
            _this2.addParamsToRequest(d);
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
};

/***/ }),

/***/ 104:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _vm._m(0)
},staticRenderFns: [function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('table', {
    staticClass: "table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive"
  }, [_c('thead'), _vm._v(" "), _c('tbody')])
}]}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-2447fe87", module.exports)
  }
}

/***/ }),

/***/ 105:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(106),
  /* template */
  __webpack_require__(107),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\ModalFrame.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] ModalFrame.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-2ddebe18", Component.options)
  } else {
    hotAPI.reload("data-v-2ddebe18", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 106:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
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

exports.default = {
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

};

/***/ }),

/***/ 107:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-2ddebe18", module.exports)
  }
}

/***/ }),

/***/ 108:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(109),
  /* template */
  __webpack_require__(110),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\Confirm.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Confirm.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-dc702ed0", Component.options)
  } else {
    hotAPI.reload("data-v-dc702ed0", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 109:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
//
//
//
//
//
//
//
//

exports.default = {
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
};

/***/ }),

/***/ 110:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-dc702ed0", module.exports)
  }
}

/***/ }),

/***/ 141:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_startup__ = __webpack_require__(62);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_interviewer_CreateNewApp__ = __webpack_require__(325);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_interviewer_CreateNewApp___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_interviewer_CreateNewApp__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_store__ = __webpack_require__(41);






__WEBPACK_IMPORTED_MODULE_2_store__["default"].registerModule('createNew', {
    actions: {
        createInterview: function createInterview(_ref, assignmentId) {
            var rootState = _ref.rootState,
                dispatch = _ref.dispatch,
                commit = _ref.commit;

            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, function (response) {
                dispatch("showProgress", true);
                window.location = response;
            }).then(function () {
                return dispatch("showProgress", false);
            });
        }
    }
});

Object(__WEBPACK_IMPORTED_MODULE_0_startup__["a" /* default */])({ app: __WEBPACK_IMPORTED_MODULE_1_interviewer_CreateNewApp___default.a });

/***/ }),

/***/ 325:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(326),
  /* template */
  __webpack_require__(327),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\interviewer\\CreateNewApp.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] CreateNewApp.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-63a69e62", Component.options)
  } else {
    hotAPI.reload("data-v-63a69e62", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 326:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
//
//
//
//
//
//


exports.default = {
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
                    return date.local().format('lll');
                }
            }, {
                data: "createdAtUtc",
                name: "CreatedAtUtc",
                title: this.$t("Assignments.CreatedAt"),
                searchable: false,
                render: function render(data) {
                    var date = moment.utc(data);
                    return date.local().format('lll');
                }
            }];

            return columns;
        }
    }
};

/***/ }),

/***/ 327:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-63a69e62", module.exports)
  }
}

/***/ }),

/***/ 41:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_vue__ = __webpack_require__(50);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vuex__ = __webpack_require__(140);



__WEBPACK_IMPORTED_MODULE_0_vue__["a" /* default */].use(__WEBPACK_IMPORTED_MODULE_1_vuex__["a" /* default */]);

var config = JSON.parse(window.vueApp.getAttribute('configuration'));

var store = new __WEBPACK_IMPORTED_MODULE_1_vuex__["a" /* default */].Store({
    state: {
        pendingHandle: null,
        pendingProgress: false,
        config: config
    },
    actions: {
        showProgress: function showProgress(context) {
            context.commit('SET_PROGRESS_TIMEOUT', setTimeout(function () {
                context.commit('SET_PROGRESS', true);
            }, 750));
        },
        hideProgress: function hideProgress(context) {
            clearTimeout(context.state.pendingHandle);
            context.commit('SET_PROGRESS', false);
        }
    },
    mutations: {
        SET_PROGRESS: function SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility;
        },
        SET_PROGRESS_TIMEOUT: function SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle;
        }
    },
    getters: {
        config: function config(state) {
            return state.config;
        }
    }
});

/* harmony default export */ __webpack_exports__["default"] = (store);

/***/ }),

/***/ 62:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";

// CONCATENATED MODULE: ./Dependencies/app/plugins/locale.js
/* harmony default export */ var locale_defaultExport = (function (Vue, options) {

    Object.defineProperty(Vue.prototype, '$t', {
        get: function get() {
            var _this = this;

            return function (arg) {
                if (_this.$store) {
                    var state = _this.$store.state;

                    if (state.config) {
                        var resource = state.config.resources;

                        return state.config.resources[arg] || arg;
                    }
                }
                return arg;
            };
        }
    });
});
// CONCATENATED MODULE: ./Dependencies/app/startup.js
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_polyfill__ = __webpack_require__(111);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_polyfill___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_babel_polyfill__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vue__ = __webpack_require__(50);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__components_Typeahead__ = __webpack_require__(90);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__components_Typeahead___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2__components_Typeahead__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__components_Layout__ = __webpack_require__(93);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__components_Layout___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_3__components_Layout__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__components_Filters__ = __webpack_require__(96);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__components_Filters___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_4__components_Filters__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__components_FilterBlock__ = __webpack_require__(99);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__components_FilterBlock___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_5__components_FilterBlock__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__components_DataTables__ = __webpack_require__(102);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__components_DataTables___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_6__components_DataTables__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__components_ModalFrame__ = __webpack_require__(105);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__components_ModalFrame___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_7__components_ModalFrame__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__components_Confirm__ = __webpack_require__(108);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__components_Confirm___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_8__components_Confirm__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_10__store__ = __webpack_require__(41);














__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].use(locale_defaultExport);

__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Layout", __WEBPACK_IMPORTED_MODULE_3__components_Layout___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Filters", __WEBPACK_IMPORTED_MODULE_4__components_Filters___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("FilterBlock", __WEBPACK_IMPORTED_MODULE_5__components_FilterBlock___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Typeahead", __WEBPACK_IMPORTED_MODULE_2__components_Typeahead___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("DataTables", __WEBPACK_IMPORTED_MODULE_6__components_DataTables___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("ModalFrame", __WEBPACK_IMPORTED_MODULE_7__components_ModalFrame___default.a);
__WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */].component("Confirm", __WEBPACK_IMPORTED_MODULE_8__components_Confirm___default.a);

/* harmony default export */ __webpack_exports__["a"] = (function (_ref) {
    var app = _ref.app,
        options = _ref.options;

    var store = __webpack_require__(41).default;

    var vueApp = new __WEBPACK_IMPORTED_MODULE_1_vue__["a" /* default */](_.assign({
        el: "#vueApp",
        render: function render(h) {
            return h(app);
        },
        components: { app: app },
        store: store
    }, options));
});

/***/ }),

/***/ 90:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(91),
  /* template */
  __webpack_require__(92),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\Typeahead.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Typeahead.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-d3471dc2", Component.options)
  } else {
    hotAPI.reload("data-v-d3471dc2", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 91:
/***/ (function(module, exports, __webpack_require__) {

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
            this.$http.get(this.fetchUrl, { params: requestParams }).then(function (response) {
                _this2.options = response.body.options || [];
                _this2.isLoading = false;
            }, function (response) {

                _this2.isLoading = false;
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

/***/ 92:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
  }), _vm._v(" "), (_vm.isLoading) ? _c('li', [_c('a', [_vm._v("Loading...")])]) : _vm._e(), _vm._v(" "), (!_vm.isLoading && _vm.options.length === 0) ? _c('li', [_c('a', [_vm._v("No results found")])]) : _vm._e()], 2)]), _vm._v(" "), (_vm.value !== null) ? _c('button', {
    staticClass: "btn btn-link btn-clear",
    on: {
      "click": _vm.clear
    }
  }, [_c('span')]) : _vm._e()])
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-d3471dc2", module.exports)
  }
}

/***/ }),

/***/ 93:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(94),
  /* template */
  __webpack_require__(95),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\Layout.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Layout.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-5a677182", Component.options)
  } else {
    hotAPI.reload("data-v-5a677182", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 94:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});
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

exports.default = {
    props: {
        title: String,
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
};

/***/ }),

/***/ 95:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('main', [_c('div', {
    staticClass: "container-fluid"
  }, [_c('div', {
    staticClass: "row"
  }, [_vm._t("filters"), _vm._v(" "), _c('div', {
    class: _vm.information
  }, [_c('div', {
    staticClass: "page-header clearfix"
  }, [_c('h1', [_vm._v("\n\t\t\t\t\t\t" + _vm._s(_vm.title) + "\n\t\t\t\t\t")])]), _vm._v(" "), _vm._t("default")], 2)], 2), _vm._v(" "), _vm._t("modals"), _vm._v(" "), _c('ModalFrame', {
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
},staticRenderFns: []}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-5a677182", module.exports)
  }
}

/***/ }),

/***/ 96:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(97),
  /* template */
  __webpack_require__(98),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\Filters.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] Filters.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-49409053", Component.options)
  } else {
    hotAPI.reload("data-v-49409053", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 97:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
  value: true
});
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

exports.default = {};

/***/ }),

/***/ 98:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('aside', {
    staticClass: "filters"
  }, [_vm._m(0), _vm._v(" "), _c('div', {
    staticClass: "filters-container"
  }, [_c('h4', [_vm._v(_vm._s(_vm.$t('Pages.FilterTitle')))]), _vm._v(" "), _vm._t("default")], 2)])
},staticRenderFns: [function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
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
}]}
module.exports.render._withStripped = true
if (false) {
  module.hot.accept()
  if (module.hot.data) {
     require("vue-hot-reload-api").rerender("data-v-49409053", module.exports)
  }
}

/***/ }),

/***/ 99:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(10)(
  /* script */
  __webpack_require__(100),
  /* template */
  __webpack_require__(101),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "E:\\surveysolutions\\src\\UI\\Headquarters\\WB.UI.Headquarters\\Dependencies\\app\\components\\FilterBlock.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] FilterBlock.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (false) {(function () {
  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install(require("vue"), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-5c3b9966", Component.options)
  } else {
    hotAPI.reload("data-v-5c3b9966", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ })

},[141]);
//# sourceMappingURL=interviewer_createNew.bundle.js.map
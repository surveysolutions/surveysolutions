webpackJsonp([1],[
/* 0 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
throw new Error("Cannot find module \"startup\"");
throw new Error("Cannot find module \"interviewer/CreateNewApp\"");
throw new Error("Cannot find module \"store\"");




__WEBPACK_IMPORTED_MODULE_2_store___default.a.registerModule('createNew', {
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

__WEBPACK_IMPORTED_MODULE_0_startup___default()({ app: __WEBPACK_IMPORTED_MODULE_1_interviewer_CreateNewApp___default.a });

/***/ })
],[0]);
//# sourceMappingURL=interviewer_createNew.bundle.js.map
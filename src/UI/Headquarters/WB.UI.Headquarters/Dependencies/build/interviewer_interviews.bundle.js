webpackJsonp([0],[
/* 0 */,
/* 1 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_vue__ = __webpack_require__(2);
throw new Error("Cannot find module \"startup\"");
throw new Error("Cannot find module \"interviewer/InterviewsApp\"");
throw new Error("Cannot find module \"store\"");





__WEBPACK_IMPORTED_MODULE_3_store___default.a.registerModule('interviews', {
    actions: {
        openInterview: function openInterview(context, interviewId) {
            context.dispatch("showProgress", true);
            window.location = context.rootState.config.interviewerHqEndpoint + "/OpenInterview/" + interviewId;
        },
        discardInterview: function discardInterview(context, _ref) {
            var callback = _ref.callback,
                interviewId = _ref.interviewId;

            $.ajax({
                url: context.rootState.config.interviewerHqEndpoint + "/DiscardInterview/" + interviewId,
                type: 'DELETE',
                success: callback
            });
        }
    }
});

__WEBPACK_IMPORTED_MODULE_1_startup___default()({ app: __WEBPACK_IMPORTED_MODULE_2_interviewer_InterviewsApp___default.a });

/***/ })
],[1]);
//# sourceMappingURL=interviewer_interviews.bundle.js.map
requirejs.config({
    baseUrl: '../../Scripts/lib',
    paths: {
        app: '../assign/app'
    },
    shim: {
        "amplify": {
            deps: ['jquery'],
            exports: "amplify"
        },
        'knockout': {
            exports: "ko"
        },
        'knockout.validation': {
            deps: ['knockout']
        }
    }
});
requirejs(['jquery', 'knockout', 'amplify', 'lodash', 'app/viewmodel', 'input', 'app/datacontext', 'Math.uuid'],
function ($, ko, amplify, _, viewmodel, input, datacontext) {
    ko.validation.configure({
        insertMessages: false,
        decorateElement: true,
        errorElementClass: 'error'
    });
    $.when(datacontext.parseData(input))
    .done(function () {
        viewmodel.init();
        ko.applyBindings(viewmodel);
    });
});
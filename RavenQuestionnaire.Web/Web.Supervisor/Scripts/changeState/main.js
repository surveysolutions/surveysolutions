requirejs.config({
    baseUrl: '../../Scripts/lib',
    config: {
        'waitSeconds': 15
    },
    paths: {
        app: '../changeState/app'
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
requirejs(['jquery', 'knockout', 'amplify', 'lodash', 'app/viewmodel'],
function ($, ko, amplify, _, viewmodel) {
    $('#umbrella').attr('data-bind', 'visible:isSaving');
    $('#umbrella-message').attr('data-bind', 'text:savingMessage');
    
    $(document).ready(function () {
        viewmodel.init();
        ko.applyBindings(viewmodel);
    });
    
});
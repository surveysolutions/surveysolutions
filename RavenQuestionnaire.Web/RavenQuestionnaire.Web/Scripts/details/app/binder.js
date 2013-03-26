define('binder',
    ['jquery', 'ko', 'config', 'vm'],
    function ($, ko, config, vm) {
        
        var
            ids = config.viewIds,

            bind = function () {
                ko.applyBindings(vm.questionnaire, getView(ids.details));
            },

            getView = function (viewName) {
                return $(viewName).get(0);
            };

            
        return {
            bind: bind
        };
    });
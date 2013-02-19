define('binder',
    ['jquery', 'ko', 'config', 'vm'],
    function ($, ko, config, vm) {
        var bind = function() {
            ko.applyBindings(vm);
        };
            
        return {
            bind: bind
        };
    });
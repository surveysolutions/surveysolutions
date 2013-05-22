(function () {
    var root = this;

    define3rdPartyModules();
    loadPluginsAndBoot();

    function define3rdPartyModules() {
        // These are already loaded via bundles. 
        // We define them and put them in the root object.
        define('jquery', [], function () { return root.jQuery; });
        define('ko', [], function () { return root.ko; });
        define('amplify', [], function () { return root.amplify; });
        define('infuser', [], function () { return root.infuser; });
        define('moment', [], function () { return root.moment; });
        define('sammy', [], function () { return root.Sammy; });
        define('pnotify', [], function () { return root.jQuery.pnotify; });
        define('underscore', [], function () { return root._; });
        define('bootbox', [], function () { return root.bootbox; });
    }

    function loadPluginsAndBoot() {
        // Plugins must be loaded after jQuery and Knockout, 
        // since they depend on them.
        require.config({
            baseUrl: '/Scripts/details'
        });
        requirejs([
                'ko.bindingHandlers',
                'ko.debug.helpers',
                'input',
                'ace/theme/designer',
                'ace/mode/ncalc'
        ], boot);
    }

    function boot() {
        require(['bootstrapper'], function(bs) {
            try {
                bs.run();
            } catch(e) {
                window.location.reload();
            }
        });
    }
})();
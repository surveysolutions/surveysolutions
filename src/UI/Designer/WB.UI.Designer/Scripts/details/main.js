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
        define('input', [], function () { return root.input; });
    }

    function loadPluginsAndBoot() {
        // Plugins must be loaded after jQuery and Knockout, 
        // since they depend on them.
        require.config({
            paths: {
                //'ace/theme/designer': '../../Scripts/lib/ace/mode-ncalc',
                //'ace/mode/ncalc': '../../Scripts/lib/ace/theme-designer'
            }
        });
        requirejs([
                'ko.bindingHandlers',
                'ko.debug.helpers'
        ], boot);
    }

    function boot() {
        require(['jquery', 'config', 'presenter', 'dataprimer', 'binder', 'route-config'],
            function($, config, presenter, dataprimer, binder, routeConfig) {

                $.fn.activity.defaults.color = "#fff";

                presenter.toggleActivity(true);

                $.when(dataprimer.fetch())
                    .done(binder.bind)
                    .done(routeConfig.register)
                    .always(function() {
                        presenter.toggleActivity(false);
                    });
            });
    }
})();
define('bootstrapper',
    ['jquery', 'config', 'presenter', 'dataprimer', 'binder', 'route-config'],
    function ($, config, presenter, dataprimer, binder, routeConfig) {
        var
            run = function () {

                $.when(dataprimer.fetch())
                    .done(binder.bind)
                    .done(routeConfig.register)
                    .always(function () {
                        presenter.toggleActivity(false);
                    });
            };

        return {
            run: run
        };
    });
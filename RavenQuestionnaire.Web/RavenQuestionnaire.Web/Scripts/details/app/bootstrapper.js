define('bootstrapper',
    ['jquery', 'data', 'config', 'presenter', 'binder'],
    function ($, data, config, routeConfig, presenter, dataprimer, binder) {
        var
            run = function () {
                presenter.toggleActivity(true);

                config.dataserviceInit();

                $.when(dataprimer.fetch())
                    .done(binder.bind)
                    .always(function () {
                        presenter.toggleActivity(false);
                    });
            };

        return {
            run: run
        };
    });
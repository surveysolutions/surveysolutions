define('route-config',
    ['config', 'router', 'vm'],
    function (config, router, vm) {
        var
            logger = config.logger,
            
            register = function() {

                var routeData = [

                    // Default routes
                    {
                        view: config.viewIds.details,
                        routes: [
                            {
                                isDefault: true,
                                route: config.hashes.details,
                                title: 'Details',
                                callback: vm.questionnaire.activate
                            },
                            {
                                route: config.hashes.detailsQuestionnaire + '/:questionnaire',
                                title: 'Details',
                                callback: vm.questionnaire.activate
                            },
                            {
                                route: config.hashes.detailsGroup + '/:group',
                                title: 'Details',
                                callback: vm.questionnaire.activate
                            },
                            {
                                route: config.hashes.detailsQuestion + '/:question',
                                title: 'Details',
                                callback: vm.questionnaire.activate
                            }
                        ]
                    },
                    // Invalid routes
                    {
                        view: '',
                        route: /.*/,
                        title: '',
                        callback: function() {
                            logger.error(config.toasts.invalidRoute);
                        }
                    }
                ];

                for (var i = 0; i < routeData.length; i++) {
                    router.register(routeData[i]);
                }

                // Crank up the router
                router.run();
            };
            

        return {
            register: register
        };
    });
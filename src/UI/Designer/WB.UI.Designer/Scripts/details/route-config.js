define('route-config',
    ['config', 'router', 'vm'],
    function (config, router, vm) {
        var register = function() {

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
                        callback: vm.questionnaire.activate
                        //callback: function () {
                        //    Note from TLK: this does not work because both logger.error and config.toasts are undefined
                        //    logger.error(config.toasts.invalidRoute);
                        //}
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
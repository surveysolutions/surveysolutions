define('dataprimer',
    ['ko', 'datacontext', 'config'],
    function (ko, datacontext, config) {

        var logger = config.logger,
            
            fetch = function () {
                
                return $.Deferred(function (def) {

                    var data = {
                        menu: ko.observable(),
                        groups: ko.observable(),
                        questions: ko.observable(),
                        questionnaire: ko.observable()
                    };

                    $.when(
                        datacontext.questionnaire.getData({ results: data.rooms }),
                        datacontext.timeslots.getData({ results: data.timeslots }),
                        datacontext.tracks.getData({ results: data.tracks }),
                        datacontext.attendance.getData({ param: config.currentUserId, results: data.attendance }),
                        datacontext.persons.getSpeakers({ results: data.persons }),
                        datacontext.sessions.getData({ results: data.sessions }),
                        datacontext.persons.getFullPersonById(config.currentUserId,
                            {
                                success: function(person) {
                                    config.currentUser(person);
                                }
                            }, true)
                    )

                    .pipe(function () {
                        // Need sessions and speakers in cache before
                        // speakerSessions models can be made (client model only)
                        datacontext.speakerSessions.refreshLocal();
                    })

                    .pipe(function() {
                        logger.success('Fetched data for questionnaire');
                    })

                    .fail(function () { def.reject(); })

                    .done(function () { def.resolve(); });

                }).promise();
            };

        return {
            fetch: fetch
        };
    });
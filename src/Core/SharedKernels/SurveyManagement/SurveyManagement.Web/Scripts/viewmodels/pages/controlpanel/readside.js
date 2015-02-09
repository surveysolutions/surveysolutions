Supervisor.VM.ControlPanel.ReadSide = function (rebuildApiUrl, eventHandlersApiUrl, updateRebuildStatusApiUrl, stopRebuildApiUrl) {
    Supervisor.VM.ControlPanel.ReadSide.superclass.constructor.apply(this, arguments);

    var self = this;

    self.rebuildApiUrl = rebuildApiUrl;
    self.stopRebuildApiUrl = stopRebuildApiUrl;
    self.eventHandlersApiUrl = eventHandlersApiUrl;
    self.updateRebuildStatusApiUrl = updateRebuildStatusApiUrl;

    self.rebuildByType = ko.observable();
    self.rebuildByAll = ko.computed(function () {
        return self.rebuildByType() === Supervisor.VM.ControlPanel.ReadSide.RebuildReadSideType.All;
    });
    self.rebuildByHandlers = ko.computed(function () {
        return self.rebuildByType() === Supervisor.VM.ControlPanel.ReadSide.RebuildReadSideType.ByHandlers;
    });
    self.rebuildByHandlersAndEventSource = ko.computed(function () {
        return self.rebuildByType() === Supervisor.VM.ControlPanel.ReadSide.RebuildReadSideType.ByHandlersAndEventSource;
    });
    self.setRebuildByType = function(rebuildType) {
        self.rebuildByType(rebuildType);
    }
    self.numberOfSkipedEvents = ko.observable(0);
    self.listOfEventSourcesForRebuild = ko.observable(undefined);
    self.eventHanlers = ko.observableArray([]);
    self.eventHanlersForPartialRebuild = ko.observableArray([]);
    

    self.isRebuildRunning = ko.observable(false);

    self.readSideRepositoryWriters = ko.observableArray([]);
    self.rebuildDenormalizerStatistic = ko.observableArray([]);
    self.rebuildErrors = ko.observableArray([]);
    self.hasErrors = ko.computed(function () {
        return self.rebuildErrors().length > 0;
    });

    self.lastStatusUpdateTime = ko.observable('-');
    self.lastRebuildDate = ko.observable('-');
    self.currentRebuildStatus = ko.observable('');

    self.skippedEvents = ko.observable('-');
    self.totalEvents = ko.observable('-');
    self.processedEvents = ko.observable('-');
    self.failedEvents = ko.observable('-');
    self.speed = ko.observable('-');
    self.timeSpent = ko.observable('-');
    self.estimatedTime = ko.observable('-');

    self.getSelectedEventHandlers = function () {
        var selectedHandlers;

        if (self.rebuildByHandlers()) {
            selectedHandlers = _.pluck(_.filter(self.eventHanlers(), function(handler) { return handler.isChecked(); }), "Name");
        }

        if (self.rebuildByHandlersAndEventSource()) {
            selectedHandlers = _.pluck(_.filter(self.eventHanlersForPartialRebuild(), function (handler) { return handler.isChecked(); }), "Name");
        }

        return selectedHandlers || [];
    };

    self.getListOfEventSourcesForRebuild = function() {
        var listOfEventSourcesForRebuild;

        if (!_.isUndefined(self.listOfEventSourcesForRebuild())) {
            listOfEventSourcesForRebuild = self.listOfEventSourcesForRebuild().split("\n");
        }

        return listOfEventSourcesForRebuild || [];
    };

    self.updateStatus = function() {
        self.SendRequest(self.updateRebuildStatusApiUrl, {}, function(data) {
            self.isRebuildRunning(data.IsRebuildRunning);

            self.currentRebuildStatus(data.CurrentRebuildStatus);
            self.lastRebuildDate(data.LastRebuildDate);
            self.lastStatusUpdateTime(new Date().toString());

            self.skippedEvents(data.EventPublishingDetails.SkippedEvents);
            self.totalEvents(data.EventPublishingDetails.TotalEvents);
            self.processedEvents(data.EventPublishingDetails.ProcessedEvents);
            self.failedEvents(data.EventPublishingDetails.FailedEvents);
            self.speed(data.EventPublishingDetails.Speed);
            self.timeSpent(moment.duration(data.EventPublishingDetails.TimeSpent).format("HH:mm:ss"));
            self.estimatedTime(moment.duration(data.EventPublishingDetails.EstimatedTime).format("HH:mm:ss"));

            self.reloadRepositoryWritersList(data.StatusByRepositoryWriters);
            self.reloadDenormalizerStatistics(data.ReadSideDenormalizerStatistics);
            self.reloadErrorsList(data.RebuildErrors);

            _.delay(self.updateStatus, 3000);
        }, true, true);
    };

    self.reloadRepositoryWritersList = function (newList) {
        //remove unused writers
        _.each(self.readSideRepositoryWriters(), function(oldWriter) {
            var writer = _.find(newList, function(newWriter) {
                return newWriter.WriterName == oldWriter.WriterName;
            });

            if (_.isUndefined(writer)) {
                self.readSideRepositoryWriters.pop(oldWriter);
            }
        });

        //update existings writers
        _.each(newList, function(writer) {
            var existingWriter = _.find(self.readSideRepositoryWriters(), function(oldWriter) {
                return oldWriter.WriterName == writer.WriterName;
            });

            if (_.isUndefined(existingWriter)) {
                writer.Status = ko.observable(writer.Status);
                self.readSideRepositoryWriters.push(writer);
            } else {
                existingWriter.Status(writer.Status);
            }
        });
    };

    self.reloadDenormalizerStatistics = function (newList) {
        //remove all writers
        _.each(self.rebuildDenormalizerStatistic(), function (oldWriter) {
                self.rebuildDenormalizerStatistic.pop(oldWriter);
        });

        //update all writers
        _.each(newList, function (denormalizer) {
            denormalizer.timeSpentDescription = moment.duration(denormalizer.TimeSpent).format("HH:mm:ss");
            denormalizer.timeSpentInPersentDescription = denormalizer.Percent+"%";
            self.rebuildDenormalizerStatistic.push(denormalizer);
        });
    };

    self.reloadErrorsList = function (newList) {
        //remove old errors
        _.each(self.rebuildErrors(), function (oldError) {
            var error = _.find(newList, function (newError) {
                return newError.ErrorTime == oldError.ErrorTime && newError.ErrorMessage == oldError.ErrorMessage;
            });

            if (_.isUndefined(error)) {
                self.rebuildErrors.pop(oldError);
            }
        });

        //add new errors
        _.each(newList, function (error) {
            var existingError = _.find(self.rebuildErrors(), function (newError) {
                return newError.ErrorTime == error.ErrorTime && newError.ErrorMessage == error.ErrorMessage;
            });

            if (_.isUndefined(existingError)) {
                var maxKey = self.rebuildErrors().length == 0 ? 0 : _.max(self.rebuildErrors(), function(e) { return e.ErrorKey(); }).ErrorKey();
                error.ErrorKey = ko.observable(maxKey + 1);
                self.rebuildErrors.push(error);
            }
        });
    };

    self.load = function () {
        self.updateStatus();
        self.SendRequest(self.eventHandlersApiUrl, {}, function (eventHandlers) {

            var eventHanlersForPartialRebuild = _.map(_.filter(eventHandlers, function(handler) {
                return handler.SupportsPartialRebuild;
            }), function(handler) {
                var clonedHandler = _.clone(handler, true);
                clonedHandler.isChecked = ko.observable(false);
                return clonedHandler;
            });

            _.each(eventHandlers, function (handler) { handler.isChecked = ko.observable(false); });
            
            self.eventHanlers(eventHandlers);
            self.eventHanlersForPartialRebuild(eventHanlersForPartialRebuild);

            self.setRebuildByType(Supervisor.VM.ControlPanel.ReadSide.RebuildReadSideType.All);
        }, true, true);
    };

    self.rebuild = function() {
        if (confirm("Are you sure you want to rebuild read layer at " + window.location.host + " ?")) {
            self.SendRequest(self.rebuildApiUrl, {
                numberOfSkipedEvents: self.numberOfSkipedEvents(),
                rebuildType: self.rebuildByType(),
                listOfHandlers:  self.getSelectedEventHandlers(),
                listOfEventSources: self.getListOfEventSourcesForRebuild()
            });
        }
    }

    self.stopRebuilding = function () {
        if (confirm("Are you sure that you want to stop rebuilding?")) {
            self.SendRequest(self.stopRebuildApiUrl);
        }
    }
};
Supervisor.VM.ControlPanel.ReadSide.RebuildReadSideType = { All: 0, ByHandlers: 1, ByHandlersAndEventSource: 2 };

Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.ReadSide, Supervisor.VM.BasePage);
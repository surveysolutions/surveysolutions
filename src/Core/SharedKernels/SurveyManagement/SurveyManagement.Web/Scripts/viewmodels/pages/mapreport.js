Supervisor.VM.MapReport = function (commandExecutionUrl, questionnaireUrl, questionsUrl, mapReportUrl, interiewSummaryUrl) {
    Supervisor.VM.MapReport.superclass.constructor.apply(this, [commandExecutionUrl]);

    var self = this;

    self.IsShowRequestIndicator(true);

    self.markersLimit = 50000;

    self.questionnaireUrl = questionnaireUrl;
    self.questionsUrl = questionsUrl;
    self.mapReportUrl = mapReportUrl;
    self.interiewSummaryUrl = interiewSummaryUrl;

    self.map = null;
    self.mapClustererOptions = { gridSize: 50, maxZoom: 15 };
    self.mapClusterer = null;

    self.initializeMap = function() {

        var mapOptions = {
            zoom: 9,
            mapTypeControl: true,
            mapTypeControlOptions: {
                style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
                position: google.maps.ControlPosition.TOP_CENTER
            },
            panControl: true,
            panControlOptions: {
                position: google.maps.ControlPosition.TOP_RIGHT
            },
            zoomControl: true,
            zoomControlOptions: {
                style: google.maps.ZoomControlStyle.LARGE,
                position: google.maps.ControlPosition.TOP_RIGHT
            },
            minZoom:3,
            scaleControl: true,
            streetViewControl: false
        };

        self.map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);
       
        self.mapClusterer = new MarkerClusterer(self.map, [], self.mapClustererOptions);

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(success, error);
        } else {
            error();
        }

        function success(position) {
            var center = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
            centerMap(center);
        }

        function error() {
            var center = new google.maps.LatLng(38.895111, -77.036667); // Washington
            centerMap(center);
            
        }

        function centerMap(center) {
            self.map.setCenter(center);
        }
    };

    self.load = function () {
        self.loadQuestionnaires();
    };

    self.questionnaires = ko.observable(null);
    self.selectedQuestionnaire = ko.observable();
    self.selectedVersion = ko.observable();
    self.selectedVariable = ko.observable();

    self.markerlimitReached = ko.observable(false);
    self.readyToUpdate = ko.observable(false);

    self.questionnaireVersions = ko.computed(function () {
        return self.selectedQuestionnaire() ? self.selectedQuestionnaire().Versions.sort() : null;
    });

    self.questionnaireVariables = ko.observableArray(null);

    self.isVariablesEnabled = ko.observable(false);

    self.selectedQuestionnaire.subscribe(function () {
        if (!_.isUndefined(self.selectedQuestionnaire()) && self.selectedQuestionnaire().Versions.length === 1) {
            self.selectedVersion(undefined);
            self.selectedVersion(self.selectedQuestionnaire().Versions[0]);
        } else {
            self.selectedVersion(undefined);
        }
        self.selectedVariable(undefined);
        self.isVariablesEnabled(false);
        self.questionnaireVariables([]);
    });

    self.selectedVersion.subscribe(function (value) {
        self.questionnaireVariables([]);
        self.isVariablesEnabled(false);
        self.selectedVariable(undefined);
        
        if (_.isUndefined(value))
            return;

        var params = {
            QuestionType: 'GpsCoordinates',
            QuestionnaireId: self.selectedQuestionnaire().QuestionnaireId,
            QuestionnaireVersion: self.selectedVersion()
        };

        self.SendRequest(self.questionsUrl, params, function (data) {
            self.questionnaireVariables(data.Variables);
            if (self.questionnaireVariables().length > 0) {
                self.isVariablesEnabled(true);
                if (self.questionnaireVariables().length === 1) {
                    self.selectedVariable(self.questionnaireVariables()[0]);
                }
            } else {
                self.isVariablesEnabled(false);
                self.ShowNotification("No Geo Location question", "There are no any Geo Locations in chosen questionnaire");
            }
        }, true);
    });

    self.selectedVariable.subscribe(function (value) {

        self.clearAllMarkers();

        if (_.isUndefined(value)) {
            self.readyToUpdate(false);
            return;
        }

        self.showPointsOnMap(180, 90, -180, -90, true);
        self.readyToUpdate(true);
    });
  
    self.TotalCount = ko.observable();
    self.Pager = ko.pager(self.TotalCount);
    self.Pager().PageSize(20);
    self.Pager().CurrentPage.subscribe(function () {
        self.search(self.SortOrder);
    });
    self.Pager().CanChangeCurrentPage = ko.computed(function () { return self.IsAjaxComplete(); });

    self.loadQuestionnaires = function () {
        var params = {
            Pager: {
                Page: self.Pager().CurrentPage(),
                PageSize: self.Pager().PageSize()
            }
        };

        self.SendRequest(self.questionnaireUrl, params, function (data) {
            self.questionnaires(data.Items);
            self.TotalCount(data.TotalCount);
            if (self.questionnaires.length === 1) {
                self.selectedQuestionnaire(self.questionnaires()[0]);
            }
        }, true);
    };

    self.interviewDetailsTooltip = new InfoBubble();
    self.markers = [];

    self.showPointsOnMap = function (northEastCornerLongtitude, northEastCornerLatitude, southWestCornerLongtitude, southWestCornerLatitude, extendBounds) {

        var key = self.selectedVariable().Variable + "-" + self.selectedQuestionnaire().QuestionnaireId + "-" + self.selectedVersion();

        var params = {
                Variable: self.selectedVariable().Variable,
                QuestionnaireId: self.selectedQuestionnaire().QuestionnaireId,
                QuestionnaireVersion: self.selectedVersion(),

                NorthEastCornerLongtitude: northEastCornerLongtitude,
                NorthEastCornerLatitude: northEastCornerLatitude,
                SouthWestCornerLongtitude: southWestCornerLongtitude,
                SouthWestCornerLatitude: southWestCornerLatitude
            };

            self.SendRequest(self.mapReportUrl, params, function (data) {
                var mapPoints = data.Points;

                if (mapPoints.length == 0) {
                    self.ShowNotification(input.settings.messages.notifyNoMarkersTitle, input.settings.messages.notifyNoMarkersText);
                    return;
                }

                if (mapPoints.length >= self.markersLimit) {
                    self.ShowNotification(input.settings.messages.notifyMarkersLimitReachedTitle, input.settings.messages.notifyNoMarkersLimitReachedText);
                    self.markerlimitReached(true);
                }

                var bounds = new google.maps.LatLngBounds();

                for (var i = 0; i < mapPoints.length; i++) {
                    var mapPoint = mapPoints[i];
                    var mapPointAnswers = mapPoint.Answers.split('|');

                    for (var j = 0; j < mapPointAnswers.length; j++) {
                        var points = mapPointAnswers[j].split(';');
                        var marker = new google.maps.Marker({
                            position: new google.maps.LatLng(points[0] * 1, points[1] * 1)
                        });
                        marker.interviewId = mapPoint.Id;
                        
                        google.maps.event.addListener(marker, 'click', function () {
                            var marker = this;
                            self.SendRequest(self.interiewSummaryUrl, { InterviewId: marker.interviewId }, function (data) {
                                if (data == undefined || data == null)
                                    return;

                                data["InterviewId"] = marker.interviewId;

                                var tooltipTemplateElement = document.createElement("div");
                                $(tooltipTemplateElement).append($("#interview-tooltip-template").html());
                                ko.applyBindings(data, tooltipTemplateElement);

                                self.interviewDetailsTooltip.setContent($(tooltipTemplateElement).html());
                                self.interviewDetailsTooltip.open(self.map, marker);
                            });
                        });
                        self.markers.push(marker);
                        if (extendBounds)
                            bounds.extend(marker.getPosition());
                    }
                }

                self.mapClusterer.addMarkers(self.markers);
                if (extendBounds)
                    self.map.fitBounds(bounds);
                
            }, true);

    };
    
    self.clearAllMarkers = function() {
        for (var i = 0; i < self.markers.length; i++) {
                self.markers[i].setMap(null);
            }
            
        self.markers = [];
        self.mapClusterer.clearMarkers();
        self.interviewDetailsTooltip.close();
        self.markerlimitReached(false);
    };

    self.reloadMarkersInBounds = function () {
        self.clearAllMarkers();
        var bounds = self.map.getBounds();

        self.showPointsOnMap(bounds.getNorthEast().lng(), bounds.getNorthEast().lat(), bounds.getSouthWest().lng(), bounds.getSouthWest().lat(), false);
    };

    $('body').addClass('map-report');
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.MapReport, Supervisor.VM.BasePage);
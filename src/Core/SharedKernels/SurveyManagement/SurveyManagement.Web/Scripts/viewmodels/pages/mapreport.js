Supervisor.VM.MapReport = function (commandExecutionUrl, questionnaireUrl, questionsUrl, mapReportUrl, interiewSummaryUrl) {
    Supervisor.VM.MapReport.superclass.constructor.apply(this, [commandExecutionUrl]);

    var self = this;

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

    self.questionnaireVersions = ko.computed(function () {
        return self.selectedQuestionnaire() ? self.selectedQuestionnaire().Versions.sort() : null;
    });

    self.questionnaireVariables = ko.observableArray(null);

    self.isVariablesEnabled = ko.observable(false);

    self.selectedQuestionnaire.subscribe(function () {
        if (!_.isUndefined(self.selectedQuestionnaire()) && self.selectedQuestionnaire().Versions.length === 1) {
            self.selectedVersion(self.selectedQuestionnaire().Versions[0]);
            self.selectedVersion.valueHasMutated();
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
        });
    });

    self.selectedVariable.subscribe(function (value) {
        if (_.isUndefined(value))
            return;

        if (self.questionnaireVariables().length === 1) {
            self.showPointsOnMap();
        }
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
        });
    };

    self.interviewDetailsTooltip = new InfoBubble();
    self.markers = {};
    self.markersSetsInfo = ko.observableArray([]);
    self.showPointsOnMap = function () {

        var key = self.selectedVariable().Variable + "-" + self.selectedQuestionnaire().QuestionnaireId + "-" + self.selectedVersion();
      
        if (self.markers[key]) {

        } else {
            var params = {
                Variable: self.selectedVariable().Variable,
                QuestionnaireId: self.selectedQuestionnaire().QuestionnaireId,
                QuestionnaireVersion: self.selectedVersion()
            };

            self.SendRequest(self.mapReportUrl, params, function (data) {
                var mapPoints = data.Points;

                if (mapPoints.length == 0) {
                    self.ShowNotification(input.settings.messages.notifyNoMarkersTitle, input.settings.messages.notifyNoMarkersText);
                    return;
                }

                var bounds = new google.maps.LatLngBounds();

                var markers = [];

                for (var i = 0; i < mapPoints.length; i++) {
                    var mapPoint = mapPoints[i];
                    var mapPointAnswers = mapPoint.Answers.split('|');

                    for (var j = 0; j < mapPointAnswers.length; j++) {
                        var points = mapPointAnswers[j].split(';');
                        var marker = new google.maps.Marker({
                            position: new google.maps.LatLng(points[0] * 1, points[1] * 1)
                        });
                        marker.interviewId = mapPoint.InterviewId;
                        
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
                        markers.push(marker);
                        bounds.extend(marker.getPosition());
                    }
                }

                self.mapClusterer.addMarkers(markers);
                self.markers[key] = markers;
                self.map.fitBounds(bounds);

                self.markersSetsInfo.push({
                    id: key,
                    variable: self.selectedVariable().Variable,
                    version: self.selectedVersion(),
                    title: self.selectedQuestionnaire().Title,
                    questionnaireId: self.selectedQuestionnaire().QuestionnaireId,
                    count: markers.length
                });
            }, true);
        }
    };
    self.removeMarkersSet = function (markerSet) {
        var key = markerSet.id;
        for (var i = 0; i < self.markers[key].length; i++) {
            self.mapClusterer.removeMarker(self.markers[key][i]);
        }
        self.markers[key].length = 0;
        delete self.markers[key];

        self.markersSetsInfo.remove(markerSet);
        self.interviewDetailsTooltip.close();
    };

    self.clearAllMarkers = function() {
        for (var markersKey in self.markers) {
            for (var i = 0; i < self.markers[markersKey].length; i++) {
                self.markers[markersKey][i].setMap(null);
            }
            self.markers[markersKey].length = 0;
           
        }
        self.markers = {};
        self.mapClusterer.clearMarkers();
        self.markersSetsInfo.removeAll();
        self.interviewDetailsTooltip.close();
    };
    $('body').addClass('map-report');
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.MapReport, Supervisor.VM.BasePage);
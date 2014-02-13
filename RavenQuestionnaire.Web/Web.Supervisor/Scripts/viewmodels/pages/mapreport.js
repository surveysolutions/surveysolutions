Supervisor.VM.MapReport = function (questionnaireUrl, commandExecutionUrl) {
    Supervisor.VM.MapReport.superclass.constructor.apply(this, [commandExecutionUrl]);

    var self = this;

    self.questionnaireUrl = questionnaireUrl;

    self.map = null;

    self.initializeMap = function() {

        var mapOptions = {
            zoom: 9,
            mapTypeControl: false,

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
            //var args = {
            //    variable: "GeoLocation",
            //    questionnaireId: "ad88a56c-f041-4614-9095-79dda90bfc76",
            //    questionnaireVersion: 1
            //};

            //var url = '@Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "ReportDataApi", action = "MapReport" })';
            //$.post(url, args, null, "json").done(function (data) {
            //    var locations = data.Answers;
            //    var bounds = new google.maps.LatLngBounds();
            //    for (i = 0; i < locations.length; i++) {
            //        var points = locations[i].split(';');
            //        var marker = new google.maps.Marker({
            //            position: new google.maps.LatLng(points[0] * 1, points[1] * 1),
            //            map: map
            //        });
            //        bounds.extend(marker.getPosition());
            //    }

            //    map.fitBounds(bounds);

            //}).fail(function (data) {
            //    console.log(data);

            //}).always(function (data) {

            //    console.log(data);
            //});
        }
    };

    self.load = function () {
        self.IsAjaxComplete(false);
      
        self.IsAjaxComplete(true);
        self.IsPageLoaded(true);
    };

    $('body').addClass('map-report');
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.MapReport, Supervisor.VM.BasePage);
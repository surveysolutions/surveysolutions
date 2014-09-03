Supervisor.VM.ChartPage = function(serviceUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;

    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');
    self.Stats = ko.observable(null);
    self.Plot = null;
    self.FromDate = ko.observable(null);
    self.FromDateInput = ko.observable(null);
    self.ToDate = ko.observable(null);
    self.ToDateInput = ko.observable(null);
    self.ShouldShowDateValidationMessage = ko.observable(false);

    self.initChart = function () {
        var selectedTemplate = JSON.parse(self.SelectedTemplate());

        var startDate = moment(self.FromDate(), "MM/DD/YYYY");
        var endDate = moment(self.ToDate(), "MM/DD/YYYY");

        if (startDate.isAfter(endDate)) {
            self.ShouldShowDateValidationMessage(true);
        } else {
            self.ShouldShowDateValidationMessage(false);
        }

        var params = {
            templateId: selectedTemplate.templateId,
            templateVersion: selectedTemplate.version,
            from: self.FromDate(),
            to: self.ToDate()
        };

        self.SendRequest(self.ServiceUrl, params, function(data) {
            self.Stats(data);
            self.drawChart();
        });
    };

    self.drawChart = function() {
        if (self.Stats().Ticks.length === 0)
            return;

        self.Plot = $.jqplot('interviewChart',
            self.Stats().Stats, {
                stackSeries: true,
                showMarker: false,
                highlighter: {
                    show: true,
                    showTooltip: false
                },
                seriesDefaults: {
                    fill: true,
                },
                series: [
                    { label: 'Supervisor assigned' },
                    { label: 'Interviewer assigned' },
                    { label: 'Completed' },
                    { label: 'Rejected by Supervisor' },
                    { label: 'Approved by Supervisor' },
                    { label: 'Rejected by Headquarters' },
                    { label: 'Approved by Headquarters' }
                ],
                legend: {
                    show: true,
                    placement: 'outsideGrid'
                },
                grid: {
                    drawBorder: false,
                    shadow: false
                },
                axesDefaults:
                {
                    min: 0,
                    tickInterval: 1,
                    tickOptions: {
                        formatString: '%d'
                    }
                },
                axes: {
                    xaxis: {
                        ticks: self.Stats().Ticks,
                        tickRenderer: $.jqplot.CanvasAxisTickRenderer,
                        tickOptions: {
                            angle: -90
                        },
                        drawMajorGridlines: false
                    }
                }
            }).replot();
    };

    self.formatDate = function(today) {
        var dd = today.getDate();
        var mm = today.getMonth() + 1; //January is 0!
        var yyyy = today.getFullYear();

        if (dd < 10) {
            dd = '0' + dd;
        }

        if (mm < 10) {
            mm = '0' + mm;
        }

        return  mm + '/' + dd + '/' + yyyy;
    };

    self.load = function() {
        var today = new Date();
        today = self.formatDate(today);

        var oneWeekAgo = new Date();
        oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
        oneWeekAgo = self.formatDate(oneWeekAgo);

        $('.list-group .input-group.date').datepicker({
            format: "mm/dd/yyyy",
            keyboardNavigation: false,
            autoclose: true,
            todayHighlight: true,
            forseParse: false
        }).on("hide", function (e) {
            if (e.date !== undefined) {
                self.FromDate(self.FromDateInput());
                self.ToDate(self.ToDateInput());

                self.initChart();
            } else {
                self.FromDateInput(self.FromDate());
                self.ToDateInput(self.ToDate());
            }
        });
        
        self.FromDate(oneWeekAgo);
        self.FromDateInput(oneWeekAgo);

        self.ToDate(today);
        self.ToDateInput(today);

        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        self.SelectedTemplate.subscribe(function () { self.initChart(); });
        
        self.initChart();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
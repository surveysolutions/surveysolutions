Supervisor.VM.ChartPage = function(serviceUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;
    var dateFormat = "MM/DD/YYYY";

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

        var startDate = moment(self.FromDate(), dateFormat);
        var endDate = moment(self.ToDate(), dateFormat);

        self.ShouldShowDateValidationMessage(startDate.isAfter(endDate));

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
                seriesColors: ["#4FADDB", "#FDBD30", "#86B828", "#F08531", "#13A388", "#E06B5C", "#00647F", "#38407D", "#785C99", "#A30F2C", "#878787", "#414042"],
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
                    //tickInterval: 1,
                    autoscale:true,
                    tickOptions: {
                        formatString: '%d'
                    }
                },
                axes: {
                    xaxis: {
                        ticks: self.Stats().Ticks,
                        tickRenderer: $.jqplot.DateAxisRenderer,
                        tickOptions: {
                            //angle: -90
                        },
                        drawMajorGridlines: false
                    }
                }
            }).replot();
    };

    self.load = function() {
        var today = moment().format(dateFormat);
        var oneWeekAgo = moment().add("weeks", -1).format(dateFormat);

        self.FromDate(oneWeekAgo);
        self.FromDateInput(oneWeekAgo);

        self.ToDate(today);
        self.ToDateInput(today);

        $('.list-group .input-group.date').datepicker({
            format: "mm/dd/yyyy",
            keyboardNavigation: false,
            autoclose: true,
            todayHighlight: true,
            endDate: '+0d',
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
        
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        self.SelectedTemplate.subscribe(function () { self.initChart(); });
        
        self.initChart();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
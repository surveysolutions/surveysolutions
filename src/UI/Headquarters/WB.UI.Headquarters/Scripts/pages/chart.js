Supervisor.VM.ChartPage = function (interviewChartsUrl, serviceUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;
    var dateFormat = "MM/DD/YYYY";

    self.Url = new Url(interviewChartsUrl);

    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');
    self.Stats = null;
    self.Plot = null;
    self.FromDate = ko.observable(null);
    self.FromDateInput = ko.observable(null);
    self.ToDate = ko.observable(null);
    self.ToDateInput = ko.observable(null);
    self.ShouldShowDateValidationMessage = ko.observable(false);
    self.TemplateName = ko.observable();

        self.initChart = function () {
        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
            ? { templateId: '', version: '' }
            : JSON.parse(self.SelectedTemplate());

        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;
        self.Url.query['from'] = self.FromDate();
        self.Url.query['to'] = self.ToDate();

        if (Modernizr.history) {
            window.history.pushState({}, "Charts", self.Url.toString());
        }

        var startDate = moment(self.FromDate(), dateFormat);
        var endDate = moment(self.ToDate(), dateFormat);

        self.ShouldShowDateValidationMessage(startDate.isAfter(endDate));

        var params = {
            templateId: selectedTemplate.templateId,
            templateVersion: selectedTemplate.version,
            from: self.FromDate(),
            to: self.ToDate()
        };

        self.SendRequest(self.ServiceUrl, params, function (data) {
            self.Stats = data;
            self.drawChart();
        });
    };

    self.drawChart = function () {
        if (self.Stats.Lines[0].length === 0)
            return;

        $('#interviewChart').empty();

        if (self.Plot != null) {
            self.Plot.destroy();
        }

        self.Plot = $.jqplot('interviewChart',
            self.Stats.Lines,
            {
                seriesColors: ["#4FADDB", "#FDBD30", "#86B828", "#F08531", "#13A388", "#E06B5C", "#00647F", "#38407D", "#785C99", "#A30F2C", "#878787", "#414042"],
                stackSeries: true,
                showMarker: true,
                series: [
                    { label: 'Supervisor assigned' },
                    { label: 'Interviewer assigned' },
                    { label: 'Completed' },
                    { label: 'Rejected by Supervisor' },
                    { label: 'Approved by Supervisor' },
                    { label: 'Rejected by Headquarters' },
                    { label: 'Approved by Headquarters' }
                ],
                seriesDefaults: {
                    showMarker: true,
                    fill: true,
                    shadow: false,
                    fillAlpha: 0.8,
                    markerOptions: {
                        show: true,
                    }
                },
                legend: {
                    renderer: $.jqplot.EnhancedLegendRenderer,
                    show: true,
                    placement: 'outsideGrid',
                    showSwatches: true,
                    location: 'n',
                    rendererOptions: {
                        numberColumns: 7
                    },
                },
                grid: {
                    drawBorder: false,
                    shadow: false
                },
                axesDefaults:
                {
                    autoscale: true,
                    tickRenderer: $.jqplot.CanvasAxisTickRenderer
                },
                axes: {
                    xaxis: {
                        renderer: $.jqplot.DateAxisRenderer,
                        min: self.Stats.from,
                        drawMajorGridlines: false
                    },
                    yaxis: {
                        min: 0
                    }
                },
                highlighter: {
                    show: true,
                    showMarker: true,
                    tooltipAxes: 'xy'
                },
                cursor: {
                    show: true,
                    tooltipLocation: 'sw',
                }
            });

        var maxY = self.Plot.axes.yaxis._dataBounds.max;
        var interval = self.getCustomInterval(maxY);
        if (interval !== null) {
            self.Plot.replot({ axes: { yaxis: { min: 0, tickInterval: interval } } });
        };
        
        var legendLabels = $('.jqplot-table-legend.jqplot-table-legend-label.jqplot-seriesToggle');
        var countItemsInLegend = legendLabels.length;
        legendLabels.width(($('#interviewChart').outerWidth() - countItemsInLegend * 20) / countItemsInLegend - 1);
    };

    self.getCustomInterval = function(maxValue) {
        if (maxValue <= 10) {
            return 1;
        }
        return null;
    };

    self.load = function () {
        var today = moment().format(dateFormat);
        var oneWeekAgo = moment().add("weeks", -1).format(dateFormat);

        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";
        self.Url.query['from'] = self.QueryString['from'] || oneWeekAgo;
        self.Url.query['to'] = self.QueryString['to'] || today;

        updateTemplateName(self.SelectedTemplate());

        var from = unescape(self.Url.query['from']);
        var to = unescape(self.Url.query['to']);

        self.FromDate(from);
        self.FromDateInput(from);
        self.ToDate(to);
        self.ToDateInput(to);

        $('.list-group .input-group.date').datepicker({
            format: "mm/dd/yyyy",
            keyboardNavigation: false,
            autoclose: true,
            todayHighlight: true,
            startDate: "01/01/2013",
            endDate: '+0d',
            forseParse: false
        })
        // hack to prevent toggling selected day error https://github.com/eternicode/bootstrap-datepicker/issues/775
        .on("hide", function (e) {
            if (e.date !== undefined) {
                self.FromDate(self.FromDateInput());
                self.ToDate(self.ToDateInput());
            }
            else {
                self.FromDateInput(self.FromDate());
                self.ToDateInput(self.ToDate());

                $('.list-group .input-group.date').datepicker("update");
            }
            self.initChart();
        });

        self.initChart();

        self.SelectedTemplate.subscribe(function (value) {
            updateTemplateName(value);
            self.initChart();
        });
    };

    var updateTemplateName = function(value) {
        self.TemplateName($("#templateSelector option[value='" + value + "']").text());
    }
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
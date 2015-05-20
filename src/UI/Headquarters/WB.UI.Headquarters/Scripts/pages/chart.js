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
    self.ToDate = ko.observable(null);

    self.ShouldShowDateValidationMessage = ko.observable(false);
    self.TemplateName = ko.observable();

        self.initChart = function () {
        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
            ? { templateId: '', version: '' }
            : JSON.parse(self.SelectedTemplate());

        var startDate = self.FromDate() != null ? moment(self.FromDate()) : null;
        var endDate =  moment(self.ToDate());

        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;

        if (startDate != null) {
            self.Url.query['from'] = startDate.format(dateFormat);
        }
        
        self.Url.query['to'] = endDate.format(dateFormat);
        

        if (Modernizr.history) {
            window.history.pushState({}, "Charts", self.Url.toString());

        }

        if (startDate != null)
        {
            self.ShouldShowDateValidationMessage(startDate.isAfter(endDate));
        }


        var params = {
            templateId: selectedTemplate.templateId,
            templateVersion: selectedTemplate.version,
            from: startDate != null ? startDate.format(dateFormat): null,
            to: endDate.format(dateFormat)
        };

        self.SendRequest(self.ServiceUrl, params, function (data) {
            self.Stats = data;
            self.drawChart();
        });
    };

    self.drawChart = function () {
        var interviewChart = $('#interviewChart');
        var NoResultsFound = $('#NoResultsFound');

        interviewChart.empty();


        if (self.Plot != null) {
            self.Plot.destroy();
        }

        if (self.Stats.Lines.length === 0 || self.Stats.Lines[0].length === 0) {
            interviewChart.hide();
            NoResultsFound.show();
            return;
        }

        interviewChart.show();
        NoResultsFound.hide();
        self.Plot = $.jqplot('interviewChart',
            self.Stats.Lines,
            {
                seriesColors: ["#4FADDB", "#FDBD30", "#86B828", "#F08531", "#13A388", "#E06B5C", "#00647F", "#38407D", "#785C99", "#A30F2C", "#878787", "#414042"],
                stackSeries: true,
                gridPadding: { top: 50, right: 50, bottom: 50, left: 50 },
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
                    breakOnNull: false,
                    showLine: true,
                    lineWidth: 3,
                    fillAndStroke: true,
                    shadow: false,
                    fillAlpha: 0.8,
                    markerOptions: {
                        show: false,
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
                    tooltipAxes: 'xy',
                    tooltipLocation: 'ne'
                },
                cursor: {
                    show: true,
                    tooltipLocation: 'sw'
                }
            });

        var maxY = self.Plot.axes.yaxis._dataBounds.max;
        var interval = self.getCustomInterval(maxY);
        if (interval !== null) {
            self.Plot.replot({ axes: { yaxis: { min: 0, tickInterval: interval } } });
        };
        
        var legendLabels = $('.jqplot-table-legend.jqplot-table-legend-label.jqplot-seriesToggle');
        var countItemsInLegend = legendLabels.length;
        legendLabels.width((interviewChart.outerWidth() - countItemsInLegend * 20) / countItemsInLegend - 1);
    };

    self.getCustomInterval = function(maxValue) {
        if (maxValue <= 10) {
            return 1;
        }
        return null;
    };

    self.load = function () {
        var today = moment().format(dateFormat);
        
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";

        var fromPoint = self.QueryString['from'] || null;
        if (fromPoint != null) {
            self.Url.query['from'] = self.QueryString['from'] || null;
        }

        self.Url.query['to'] = self.QueryString['to'] || moment(today).format(dateFormat);

        updateTemplateName(self.SelectedTemplate());


        var from = fromPoint != null ? unescape(fromPoint) : null;
        var to = unescape(self.Url.query['to']);

        self.FromDate(from);
        self.ToDate(to);

        self.initChart();

        self.SelectedTemplate.subscribe(function (value) {
            updateTemplateName(value);
            self.initChart();
        });

        self.FromDate.subscribe(function () {
            self.initChart();
        });

        self.ToDate.subscribe(function () {
            self.initChart();
        });
    };

    var updateTemplateName = function(value) {
        self.TemplateName($("#templateSelector option[value='" + value + "']").text());
    }
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
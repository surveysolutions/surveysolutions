Supervisor.VM.ChartPage = function(serviceUrl, interviewDetailsUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;

    self.Url = new Url(interviewDetailsUrl);
    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');
    self.Stats = ko.observable(null);

    self.initChart = function () {
        self.SendRequest(self.ServiceUrl, {}, function (data) {
            self.Stats(data);
            self.drawChart();
        });
    };

    self.drawChart = function () {
        var plot = $.jqplot('interviewChart',
        self.Stats().Stats, {
            stackSeries: true,
            showMarker: false,
            highlighter: {
                show: true,
                showTooltip: true
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
        });

        $('#interviewChart').bind('jqplotHighlighterHighlight',
            function(ev, seriesIndex, pointIndex, data, plot) {
                var content = plot.series[seriesIndex].label + ', ' + plot.series[seriesIndex]._xaxis.ticks[pointIndex][1] + ', ' + data[1];
                var elem = $('#customTooltipDiv');
                elem.html(content);
                var h = elem.outerHeight();
                var w = elem.outerWidth();
                var left = ev.pageX - w - 10;
                var top = ev.pageY - h - 10;
                elem.stop(true, true).css({ left: left, top: top }).fadeIn(200);
            }
        );

        $('#interviewChart').bind('jqplotHighlighterUnhighlight',
            function() {
                $('#customTooltipDiv').fadeOut(300);
            }
        );
    };

    self.load = function() {
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";
        self.Url.query['status'] = self.QueryString['status'] || "";
        self.Url.query['interviewerId'] = self.QueryString['interviewerId'] || "";

        self.SelectedTemplate.subscribe(self.filter);

        self.initChart();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
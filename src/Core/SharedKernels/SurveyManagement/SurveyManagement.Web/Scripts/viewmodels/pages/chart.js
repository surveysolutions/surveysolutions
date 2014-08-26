Supervisor.VM.ChartPage = function(serviceUrl, interviewDetailsUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;

    self.Url = new Url(interviewDetailsUrl);
    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');
    self.Stats = ko.observable(null);
    self.Plot = null;

    self.initChart = function() {
        var selectedTemplate = JSON.parse(self.SelectedTemplate());

        var params = {
            templateId: selectedTemplate.templateId,
            templateVersion: selectedTemplate.version
        };

        self.SendRequest(self.ServiceUrl, params, function(data) {
            self.Stats(data);
            self.drawChart();
        });
    };

    self.drawChart = function() {
        self.Plot = $.jqplot('interviewChart',
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

        self.SelectedTemplate.subscribe(function() { self.initChart(); });

        self.initChart();

        $('.list-group .input-group.date').datepicker({});
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
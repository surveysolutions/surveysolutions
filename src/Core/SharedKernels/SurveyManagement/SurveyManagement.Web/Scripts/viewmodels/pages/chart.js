Supervisor.VM.ChartPage = function(serviceUrl, interviewDetailsUrl, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;

    self.Url = new Url(interviewDetailsUrl);
    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');

    self.getChartData = function () {
        var supervisorAssignedData = [30, 9, 5, 12, 14, 8, 7, 9, 6, 11, 3, 2, 0];
        var interviewerAssignedData = [0, 5, 5, 3, 6, 5, 3, 2, 6, 7, 4, 3, 0];
        var completedData = [0, 6, 5, 8, 2, 3, 4, 2, 1, 5, 7, 4, 0];
        var rejectedBySupervisor = [0, 6, 8, 8, 2, 3, 4, 2, 1, 4, 6, 5, 0];
        var approvedBySupervisor = [0, 6, 8, 8, 2, 3, 4, 3, 2, 5, 7, 6, 0];
        var rejectedByHeadquarters = [0, 6, 8, 8, 2, 3, 4, 3, 2, 5, 7, 6, 0];
        var approvedByHeadquarters = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 30];

        var stats = [supervisorAssignedData, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters];

        var ticks = [[1, 'Aug 3'], [2, 'Aug 4'], [3, 'Aug 5'], [4, 'Aug 6'], [5, 'Aug 7'], [6, 'Aug 8'], [7, 'Aug 9'], [8, 'Aug 10'], [9, 'Aug 11'], [10, 'Aug 12'], [11, 'Aug 13'], [12, 'Aug 14'], [13, 'Aug 15']];

        return { stats: stats, ticks: ticks };
    };

    self.initChart = function() {
        var plot = $.jqplot('interviewChart',
        self.getChartData().stats, {
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
                    ticks: self.getChartData().ticks,
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

        self.search();
        self.initChart();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);
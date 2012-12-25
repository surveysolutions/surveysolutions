plotSettings = {
    column: {
        chart: {
            renderTo: 'container',
            type: 'column',
            marginBottom: 70,
            marginRight: 5,
            marginTop: 30
        },
        xAxis: {

        },
        yAxis: {
            title: {
                text: 'Count'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }],
            allowDecimals: false
        },
        title: {
            text: ""
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + '</b><br/>' +
                        this.x + ': ' + this.y;
            }
        },
        legend: {
            align: 'center',
            verticalAlign: 'top',
            borderWidth: 0,
            y: -10
        },
        plotOptions: {
            column: {
                stacking: 'normal'
            },
            bar: {
                stacking: 'normal'
            },
            series: {
                pointWidth: 20
            },
            pie: {
                shadow: false,
                allowPointSelect: true,
                showInLegend: true
            }
        }
    },
    bar: {
        chart: {
            renderTo: 'container',
            type: 'bar',
            marginBottom: 70,
            marginRight: 5,
            marginTop: 30
        },
        xAxis: {
        },
        yAxis: {
            title: {
                text: 'Count'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }],
            allowDecimals: false
        },
        title: {
            text: ""
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + '</b><br/>' +
                        this.x + ': ' + this.y;
            }
        },
        legend: {
            align: 'center',
            verticalAlign: 'top',
            borderWidth: 0,
            y: -10
        },
        plotOptions: {
            column: {
                stacking: 'normal'
            },
            bar: {
                stacking: 'normal'
            },
            series: {
                pointWidth: 20
            }
        }
    },
    pie: {
        chart: {
            renderTo: 'container',
            type: 'pie',
            marginBottom: 70,
            marginRight: 5,
            marginTop: 30
        },

        title: {
            text: ""
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + '</b><br/>' +
                        this.x + ': ' + this.y;
            }
        },
        legend: {
            align: 'center',
            verticalAlign: 'top',
            borderWidth: 0,
            y: -10
        },
        plotOptions: {
            pie: {
                shadow: false,
                allowPointSelect: true,
                showInLegend: true
            }
        }
    },
    scatter: {
        chart: {
            renderTo: 'container',
            type: 'scatter',
            marginBottom: 70,
            marginRight: 5,
            marginTop: 75,
            zoomType: 'xy'
        },
        xAxis: {
            title: {
                text: 'In progress'
            },
            allowDecimals: false,
            min: 0,
            gridLineWidth: 1
        },
        yAxis: {
            title: {
                text: 'Done'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }],
            allowDecimals: false,
            min: 0
        },
        title: {
            text: ""
        },
        tooltip: {
            formatter: function () {
                return '' +
                        this.x + ' in progress, ' + this.y + ' complete';
            }
        },
        legend: {
            align: 'center',
            verticalAlign: 'top',
            borderWidth: 0,
            y: -10
        },
        plotOptions: {
            scatter: {
                marker: {
                    radius: 7,
                    states: {
                        hover: {
                            enabled: true,
                            lineColor: 'rgb(100,100,100)'
                        }
                    }
                },
                states: {
                    hover: {
                        marker: {
                            enabled: false
                        }
                    }
                }
            }
        }
    }
};
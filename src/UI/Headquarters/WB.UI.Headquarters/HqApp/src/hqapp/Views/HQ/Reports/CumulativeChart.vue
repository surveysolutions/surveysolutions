<script>
import { Line } from "vue-chartjs";
import Vue from "vue";

const chartOptions = {
    chartId: "interviewChart",
    elements: {
        point: { radius: 0 },
        line: { fill: true, tension: 0 }
    },
    responsive: true,
    maintainAspectRatio: false,
    tooltips: {
        mode: "x",
        intersect: false
    },
    hover: {
        mode: "index",
        intersect: false
    },
    scales: {
        xAxes: [
            {
                type: "time",

                gridLines: {
                    display: true,
                    tickMarkLength: 10
                },

                ticks: {
                    source: "data",
                    autoSkipPadding: 20,
                    maxRotation: 0,
                    autoSkip: true
                },
                time: {
                    bounds: "ticks",
                    minUnit: "week"
                }
            }
        ],
        yAxes: [
            {
                type: "linear",
                stacked: true,
                ticks: {
                    beginAtZero: true,
                    userCallback: function(label, index, labels) {
                        // when the floored value is the same as the value we have a whole number
                        if (Math.floor(label) === label) {
                            return label;
                        }
                    }
                }
            }
        ]
    }
};

export default {
    extends: Line,
    props: {
        options: { required: false },
        chartData: { required: true },
        height: 600
    },

    mounted() {
        this.render();
    },

    methods: {
        render() {
            this.renderChart(this.chartData, _.assign(chartOptions, this.options));
        }
    },

    watch: {
        chartData() {
            this.render();
        }
    }
};
</script>

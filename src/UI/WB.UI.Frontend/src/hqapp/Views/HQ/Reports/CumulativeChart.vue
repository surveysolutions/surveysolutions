<template>
    <Line :data="chartData" />
</template>

<script>

const chartOptions = {
    chartId: 'interviewChart',
    elements: {
        point: { radius: 0 },
        line: { fill: true, tension: 0 },
    },
    responsive: true,
    maintainAspectRatio: false,
    tooltips: {
        mode: 'x',
        intersect: false,
    },
    hover: {
        mode: 'index',
        intersect: false,
    },

    scales: {
        xAxes: [
            {
                type: 'time',

                gridLines: {
                    display: true,
                    tickMarkLength: 10,
                },

                ticks: {
                    source: 'data',
                    autoSkipPadding: 10,
                    maxRotation: 45,
                    autoSkip: true,
                },
                time: {
                    bounds: 'ticks',
                    minUnit: 'day',
                    displayFormats: {
                        week: 'll',
                        day: 'MMM D YYYY',
                    },
                },
            },
        ],
        yAxes: [
            {
                afterDataLimits: function (axis) {
                    axis.max += 1 // add 1px to top
                    axis.min = 0
                },
                type: 'linear',
                stacked: true,
                ticks: {
                    beginAtZero: true,
                    userCallback: function (label, index, labels) {
                        // when the floored value is the same as the value we have a whole number
                        if (Math.floor(label) === label) {
                            return label
                        }
                    },
                },
            },
        ],
    },
}

import { Line } from 'vue-chartjs'
import { Chart, LineController, LineElement, PointElement, LinearScale, Title, CategoryScale } from 'chart.js'

Chart.register(LineController, LineElement, PointElement, LinearScale, Title, CategoryScale);


export default {
    name: 'ComulativeLineChart',
    components: { Line },
    props: {
        chartData: {
            type: Object,
            required: true
        },
        chartOptions: {
            type: Object,
            default: () => chartOptions
        },
        options: {
            type: Object,
            required: false
        }
    },
    methods: {
        getImage() {
            if (this.$data._chart == null) return null

            return this.$data._chart.canvas.toDataURL('image/png')
        }
    }
}
</script>
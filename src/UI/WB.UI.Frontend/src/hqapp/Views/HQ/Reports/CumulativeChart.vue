<template>
    <div class="interviewChart">
        <LineChart :data="chartData" ref="chart" dataset-id-key="status" :options="chartOptions" />
    </div>
</template>

<script>
const chartOptions = {
    elements: {
        point: { radius: 0 },
        line: { tension: 0 },
    },
    responsive: true,
    maintainAspectRatio: false,
    layout: {
        padding: 5,
    },
    interaction: {
        intersect: false,
        mode: 'index',
    },
    plugins: {
        legend: {
            display: true,
            position: 'top',
        },
        tooltip: {
            mode: 'x',
            intersect: false,
            position: 'average',
        },
    },
    scales: {
        x:
        {
            type: 'time',
            gridLines: {
                display: true,
                tickMarkLength: 10,
            },
            time: {
                bounds: 'ticks',
                minUnit: 'day',
                displayFormats: {
                    week: 'll',
                    day: 'MMM D YYYY',
                },
            },
            ticks: {
                source: 'data',
                autoSkipPadding: 10,
                maxRotation: 45,
                autoSkip: true,
            },
        },
        y:
        {
            afterDataLimits: function (axis) {
                axis.max += 1 // add 1px to top
                axis.min = 0
            },
            type: 'linear',
            stacked: true,
            beginAtZero: true,
            ticks: {
                beginAtZero: true,
                callback: function (label, index, labels) {
                    // when the floored value is the same as the value we have a whole number
                    if (Math.floor(label) === label) {
                        return label
                    }
                },
            },
        },
    },
}

import { Line as LineChart } from 'vue-chartjs'
import 'chartjs-adapter-dayjs-4'
import { Chart, LineController, Title, Tooltip, Legend, LineElement, PointElement, LinearScale, CategoryScale, TimeScale, Filler } from 'chart.js'

Chart.register(LineController, Title, Tooltip, Legend, LineElement, PointElement, LinearScale, CategoryScale, TimeScale, Filler)


export default {
    name: 'ComulativeLineChart',
    components: { LineChart },
    props: {
        chartData: {
            type: Object,
            required: true,
        },
        options: {
            type: Object,
            required: false,
        },
    },
    computed: {
        chartOptions() {
            const options = this.options || {}
            const animation = options.animation || {}
            const userOnComplete = animation.onComplete

            return Object.assign({}, chartOptions, options, {
                animation: Object.assign({}, animation, {
                    onComplete: (...args) => {
                        if (typeof userOnComplete === 'function') {
                            userOnComplete(...args)
                        }

                        this.$emit('ready')
                    },
                }),
            })
        },
    },
    expose: ['getImage'],
    methods: {
        getImage() {
            if (this.$refs.chart.chart == null) return null
            return this.$refs.chart.chart.canvas.toDataURL('image/png')
        },
    },

}
</script>
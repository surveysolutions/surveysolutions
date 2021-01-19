<template>
    <HqLayout>
        <div class="row">
            <div class="col-md-6 col-xs-12">
                <div class="panel"
                    :class="panelStatus">
                    <div class="panel-heading">
                        <h3 v-if="report == null">
                            {{ $t("Diagnostics.WaitForHealthcheck")}}
                        </h3>
                        <h3 v-else>
                            {{ $t("Diagnostics.HealthCheckStatus", { status: report.status}) }}
                        </h3>
                    </div>
                    <div class="panel-body health-checks">
                        <ul class="list-group">
                            <a class="list-group-item"
                                v-for="entry in entries"
                                :key="entry.name"
                                :href="entry.item.data.url"
                                :class="itemStatus(entry)">
                                <h4>{{ $t("Diagnostics." + entry.name )}}</h4>
                                <p>{{entry.item.description}}</p>
                                <div class="well"
                                    v-if="entry.item.exception">{{ entry.item.exception.Message}}</div>
                            </a>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="col-md-6 col-xs-12">
                <div class="panel panel-default">
                    <div class="panel-heading"
                        style="height: 40px">
                        <h3 class="pull-left">
                            {{ $t("Diagnostics.ServerMetrics") }}
                        </h3><span class="pull-right">{{ lastUpdate }}</span>
                    </div>
                    <div class="panel-body">
                        <p v-if="metrics == null || metrics.length == 0">{{ $t("Diagnostics.WaitingForMetrics")}}</p>
                        <ul class="list-group">
                            <li class="list-group-item"
                                v-for="metric in metrics"
                                :key="metric.name">
                                <b>{{metric.name}}: </b>{{metric.value}}
                            </li>
                        </ul>
                    </div>
                </div>
                <!-- <div class="panel panel-default">
                    <div class="panel-heading">
                        <h3>
                            {{ $t("Diagnostics.Connectivity") }}
                        </h3>
                    </div>
                    <div class="panel-body">
                        <ol >
                            <li v-dateTimeFormatting
                                v-for="(m, index) in signalrDiagMessages"
                                :key="index"
                                :class="{'text-danger': m.isError}">
                                [<time :datetime="m.date"></time>]
                                {{m.msg}}
                            </li>
                        </ol>
                    </div>
                </div> -->
            </div>
        </div>
    </HqLayout>
</template>

<script>

import moment from 'moment'
import * as signalR from '@microsoft/signalr'
import { DateFormats } from '~/shared/helpers'

export default {
    data() {
        return {
            report: null,
            metrics: [],
            lastUpdate: null,
            signalrDiagMessages: [],
        }
    },
    mounted() {
        this.getMetrics()
        this.getHealth()
        this.startSignalrDiag()
    },

    computed: {
        panelStatus() {
            if(this.report == null) return [ ]
            return [ 'panel-' + this.statusToBs(this.report.status)]
        },
        entries() {
            if(this.report == null) return []

            return Object.keys(this.report.entries).map(name => {
                const item = this.report.entries[name]
                return { name, item }
            })
        },
    },

    methods: {
        pushSignalrMessage(msg, isError){
            this.signalrDiagMessages.push({
                date: new Date(),
                msg: msg,
                isError: isError || false,
            })
        },

        async startSignalrDiag() {
            this.pushSignalrMessage('Building connection to server using `/signalrdiag` url')
            const connection = new signalR.HubConnectionBuilder()
                .withUrl('/signalrdiag')
                .configureLogging(signalR.LogLevel.Debug)
                .build()


            connection.on('Pong', (response) => {
                this.pushSignalrMessage(`Pong from server received using ${response}`)
            })

            try {
                await connection.start()
                this.pushSignalrMessage('Started connection')

                for(let i = 0; i < 30; i++) {
                    await connection.invoke('Ping')
                    this.pushSignalrMessage('Ping method called')
                    await new Promise(r => setTimeout(r, 2000))
                }
            }
            catch(err) {
                this.pushSignalrMessage('Failed to invoke server method with error: ' + err.toString(), true)
            }
        },

        getHealth() {
            const self = this
            this.$hq.ControlPanel.getHealthResult().then(response => {
                self.report = response.data
                setTimeout(this.getHealth, 5000)
            })
        },
        getMetrics() {
            const self = this
            this.$hq.ControlPanel.getMetricsState().then(response => {
                self.metrics = response.data.metrics
                self.lastUpdate =  moment(response.data.lastUpdateTime).format(DateFormats.dateTime)
                setTimeout(this.getMetrics, 5000)
            })
        },

        statusToBs(status) {
            switch(status) {
                case 'Healthy': return 'success'
                case 'Degraded': return 'warning'
                case 'Unhealthy': return 'danger'
            }
        },
        itemStatus(entry) {
            return ['list-group-item-' + this.statusToBs(entry.item.status), 'check-' + entry.name]
        },
    },
}
</script>

<style></style>

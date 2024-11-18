<template>
    <HqLayout :hasFilter="true" :hasHeader="false">
        <template v-slot:filters>
            <Filters>
                <FilterBlock :title="$t('Common.GeoTrackNumber')">
                    <Typeahead control-id="questionnaireId" :placeholder="$t('Common.AllGeoTracks')" noSearch
                        :values="ddlGeoTracks" :value="selectedGeoTrackingId" v-on:selected="selectedGeoTracking" />
                </FilterBlock>
                <FilterBlock :title="$t('Common.Responsible')">
                    <Typeahead control-id="responsibleId" :placeholder="$t('Common.AllResponsible')"
                        :value="responsibleId" :ajax-params="responsibleParams" :selectedValue="query.responsible"
                        v-on:selected="selectResponsible" :fetch-url="model.responsible"></Typeahead>
                </FilterBlock>
                <FilterBlock :title="$t('Common.DateRange')">
                    <DatePicker :config="datePickerConfig" :value="selectedDateRange" :withClear="true"
                        v-on:clear="clearDateRange"></DatePicker>
                </FilterBlock>
                <FilterBlock v-if="isLoading" :title="$t('Reports.MapDataLoading')">
                    <div class="progress">
                        <div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100"
                            aria-valuemin="0" aria-valuemax="100" style="width: 100%"></div>
                    </div>
                </FilterBlock>
                <div class="preset-filters-container">
                    <div class="center-block" style="margin-left: 0">
                        <button class="btn btn-default btn-lg" id="refreshGeoTrackingHistory" v-if="readyToUpdate"
                            @click="refreshGeoTrackingHistory">
                            {{ $t('MapReport.ReloadGeoTacking') }}
                        </button>
                    </div>
                </div>
            </Filters>
        </template>


        <google-map ref="mapWithMarkers" :shapefile="shapefileName" :getMarkersParams="getMarkersParams"
            @initialized="mapInitialized"></google-map>

    </HqLayout>
</template>
<style scoped>
.progress {
    margin: 15px;
}

.progress .progress-bar.active {
    font-weight: 700;
    animation: progress-bar-stripes 0.5s linear infinite;
}

.dotdotdot:after {
    font-weight: 300;
    content: '...';
    display: inline-block;
    width: 20px;
    text-align: left;
    animation: dotdotdot 1.5s linear infinite;
}

@keyframes dotdotdot {
    0% {
        content: '...';
    }

    25% {
        content: '';
    }

    50% {
        content: '.';
    }

    75% {
        content: '..';
    }
}
</style>

<script>
import { nextTick } from 'vue'
import { debounce, delay, forEach, find } from 'lodash'
import routeSync from '~/shared/routeSync'
import { Form, Field } from 'vee-validate'
import moment from 'moment'
import { DateFormats } from '~/shared/helpers'
import GoogleMap from '../../../components/GoogleMap.vue'

export default {
    mixins: [routeSync],

    components: { GoogleMap, Form, Field },

    data() {
        return {
            selectedGeoTrackingId: null,
            tracks: [],
            tracksPath: [],
            responsibleId: null,
            responsibleParams: { showArchived: true, showLocked: true },
            dateRange: null,
            isLoadingGeoTracking: false,
        }
    },

    async mounted() {
        if (this.$route.query.track)
            this.selectedGeoTrackingId = this.tracks.find(t => t.id == this.$route.query.track)
        this.responsibleId = this.$route.query.responsible
        const startDate = this.$route.query.start
        const endDate = this.$route.query.end
        if (endDate && startDate) {
            this.dateRange = {
                startDate: startDate,
                endDate: endDate,
            }
        }
    },

    computed: {
        model() {
            return this.$config.model
        },

        map() {
            return this.$refs.mapWithMarkers.map
        },

        getMarkersParams() {
            return {
                QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                QuestionnaireVersion: this.selectedVersionValue,
                ResponsibleId: (this.responsibleId || {}).key || null,
                AssignmentId: this.assignmentId
            }
        },

        assignmentId() {
            return this.model.assignmentId;
        },

        shapefileName() {
            return this.model.targetArea;
        },

        isLoading() {
            return this.$refs.mapWithMarkers?.isLoading || this.isLoadingGeoTracking || false
        },

        selectedVersionValue() {
            return this.selectedVersion == null
                ? null
                : this.selectedVersion.key
        },

        queryString() {
            return {
                track: this.query.track,
                responsible: this.query.responsible,
                start: this.query.start,
                end: this.query.end,
            }
        },

        api() {
            return this.$hq.GeoTracking
        },

        ddlGeoTracks() {
            return this.tracks.map(i => { return { key: i.id, value: "#" + i.id } })
        },

        selectedDateRange() {
            if (this.dateRange == null) return null
            return `${moment(this.dateRange.startDate).format(DateFormats.date)} to ${moment(this.dateRange.endDate).format(DateFormats.date)}`
        },

        datePickerConfig() {
            var self = this
            return {
                mode: 'range',
                maxDate: 'today',
                wrap: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? selectedDates[0] : null
                    const end = selectedDates.length > 1 ? selectedDates[1] : null
                    if (start != null && end != null) {
                        self.dateRange = {
                            startDate: start,
                            endDate: end,
                        }
                    }

                    self.onChange((q) => {
                        q.start = self.dateRange == null ? null : moment(self.dateRange.startDate).format(DateFormats.date);
                        q.end = self.dateRange == null ? null : moment(self.dateRange.endDate).format(DateFormats.date);
                    })

                    self.refreshGeoTrackingHistory()
                },
            }
        },
    },

    methods: {

        async mapInitialized() {
            await this.refreshGeoTrackingHistory();
        },

        selectedGeoTracking(value) {
            this.selectedGeoTrackingId = value

            this.onChange((q) => {
                q.track = value == null ? null : value.key
            })

            this.refreshGeoTrackingHistory()
        },

        selectResponsible(newValue) {
            this.responsibleId = newValue
            this.onChange((q) => {
                q.responsible = newValue == null ? null : newValue.value
            })
            this.refreshGeoTrackingHistory()
        },

        async refreshGeoTrackingHistory() {
            await this.getGeoTrackingHistory()
            this.displayGeoTrackingHistory()
        },

        async getGeoTrackingHistory() {

            let request = {
                responsibleId: (this.responsibleId || {}).key || null,
                assignmentId: this.assignmentId,
                start: this.dateRange?.startDate,
                end: this.dateRange?.endDate,
            }

            this.isLoadingGeoTracking = true

            const self = this

            try {
                const result = await this.api.GetTracks(request)
                self.tracks = result.data
            }
            finally {
                self.isLoadingGeoTracking = false
            }
        },

        displayGeoTrackingHistory() {

            this.tracksPath.forEach(t => {
                t.setMap(null);
            })

            const latlngBounds = new google.maps.LatLngBounds();

            tracks.forEach(t => {
                const polyline = new google.maps.Polyline({
                    path: t.points,
                    geodesic: true,
                    strokeColor: "#0000FF",
                    strokeOpacity: 0.8,
                    strokeWeight: 4,
                });

                polyline.setMap(this.map);
                this.tracksPath.push(polyline);

                /*t.points.forEach((point) => {
                    new google.maps.Marker({
                        position: point,
                        map: this.map,
                    });
                });*/

                const path = polyline.getPath();
                path.forEach((latLng) => {
                    latlngBounds.extend(latLng);
                });
            })

            this.map.fitBounds(latlngBounds)
        },

        clearDateRange() {
            this.dateRange = null
        },
    },
}
</script>

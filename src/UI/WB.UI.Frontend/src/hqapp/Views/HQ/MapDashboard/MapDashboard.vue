<template>
    <HqLayout :hasFilter="true" :hasHeader="false">
        <template v-slot:filters>
            <Filters>
                <FilterBlock :title="$t('Common.Questionnaire')">
                    <Typeahead control-id="questionnaireId" :placeholder="$t('Common.AllQuestionnaires')"
                        :ajax-params="{}" :fetch-url="model.questionnaires" :value="selectedQuestionnaireId"
                        :selectedKey="query.questionnaireId" v-on:selected="selectQuestionnaire" />
                </FilterBlock>
                <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                    <Typeahead control-id="questionnaireVersion" :placeholder="$t('Common.AllVersions')"
                        :value="selectedVersion" :values="selectedQuestionnaireId == null
                            ? null
                            : selectedQuestionnaireId.versions
                            " v-on:selected="selectQuestionnaireVersion" :disabled="selectedQuestionnaireId == null" />
                </FilterBlock>
                <FilterBlock :title="$t('Common.Responsible')" v-if="model.userRole != 'Interviewer'">
                    <Typeahead control-id="responsibleId" :placeholder="$t('Common.AllResponsible')"
                        :value="responsibleId" :ajax-params="responsibleParams" :selectedValue="query.responsible"
                        v-on:selected="selectResponsible" :fetch-url="model.responsible"></Typeahead>
                </FilterBlock>
                <FilterBlock :title="$t('Pages.Filters_Assignment')">
                    <Form as="div" class="input-group">
                        <Field class="form-control with-clear-btn number" :placeholder="$t('Common.AllAssignments')"
                            name="assignmentId" type="number" v-model.number="assignmentId"
                            :rules="{ numeric: true }" />
                        <div class="input-group-btn" @click="clearAssignmentFilter">
                            <div class="btn btn-default">
                                <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                            </div>
                        </div>
                    </Form>
                </FilterBlock>
                <FilterBlock :title="$t('Pages.Filters_Shapefiles')">
                    <Typeahead control-id="shapefileName" :placeholder="$t('Pages.Filters_None')" :ajax-params="{}"
                        :fetch-url="model.shapefiles" :value="shapefileName" v-on:selected="selectedShapefileName" />
                </FilterBlock>

                <FilterBlock v-if="isLoading" :title="$t('Reports.MapDataLoading')">
                    <div class="progress">
                        <div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100"
                            aria-valuemin="0" aria-valuemax="100" style="width: 100%"></div>
                    </div>
                </FilterBlock>
                <div class="preset-filters-container">
                    <div class="center-block" style="margin-left: 0">
                        <button class="btn btn-default btn-lg" id="reloadMarkersInBounds" v-if="readyToUpdate"
                            @click="map.reloadMarkersInBounds">
                            {{ $t('MapReport.ReloadMarkers') }}
                        </button>
                    </div>
                </div>
            </Filters>
        </template>

        <google-map ref="mapWithMarkers" :shapefile="shapefileName?.key"
            :getMarkersParams="getMarkersParams"></google-map>

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
import GoogleMap from '../../../components/GoogleMap.vue'

export default {
    mixins: [routeSync],

    components: { GoogleMap, Form, Field },

    data() {
        return {
            readyToUpdate: false,
            selectedQuestionnaireId: null,
            responsibleId: null,
            responsibleParams: { showArchived: true, showLocked: true },
            assignmentId: null,
            shapefileName: null,
        }
    },

    watch: {
        assignmentId(to) {
            this.onChange((q) => {
                q.assignmentId = to
            })

            nextTick(() => {
                this.map?.reloadMarkersInBounds()
            })
        },
        responsibleId(to) {
            this.onChange((q) => {
                q.responsibleId = to
            })

            nextTick(() => {
                this.map?.reloadMarkersInBounds()
            })
        },
    },

    computed: {
        model() {
            return this.$config.model
        },

        isLoading() {
            return this.$refs.mapWithMarkers?.isLoading || false
        },

        map() {
            return this.$refs.mapWithMarkers
        },

        getMarkersParams() {
            return {
                QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                QuestionnaireVersion: this.selectedVersionValue,
                ResponsibleId: (this.responsibleId || {}).key || null,
                AssignmentId: this.assignmentId
            }
        },

        selectedVersionValue() {
            return this.selectedVersion == null
                ? null
                : this.selectedVersion.key
        },

        queryString() {
            return {
                QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                QuestionnaireVersion: this.selectedVersionValue,
                ResponsibleId: (this.responsibleId || {}).key || null,
                AssignmentId: this.assignmentId,
            }
        },

        selectedVersion() {
            if (
                this.selectedQuestionnaireId == null ||
                this.query.version == null
            )
                return null

            return find(this.selectedQuestionnaireId.versions, {
                key: this.query.version,
            })
        },

        api() {
            return this.$hq.MapDashboard
        },
    },

    async mounted() {
        this.assignmentId = this.query.assignmentId
        this.responsibleId = this.query.responsibleId
    },

    methods: {

        selectQuestionnaireVersion(value) {
            this.questionnaireVersion = value
            this.onChange((s) => (s.version = value == null ? null : value.key))
            this.map.reloadMarkersInBounds()
        },

        selectQuestionnaire(value) {
            this.selectedQuestionnaireId = value

            if (
                value == null ||
                this.$route.query.questionnaireId !== value.key
            )
                this.selectQuestionnaireVersion(null)
            else
                this.selectQuestionnaireVersion(
                    this.$route.query.version
                        ? { key: this.$route.query.version }
                        : null
                )

            this.onChange((q) => {
                q.questionnaireId = value == null ? null : value.key
            })
        },

        selectResponsible(newValue) {
            this.responsibleId = newValue
            this.onChange((q) => {
                q.responsible = newValue == null ? null : newValue.value
            })
            this.map.reloadMarkersInBounds()
        },

        clearAssignmentFilter() {
            this.assignmentId = null
            this.map.reloadMarkersInBounds()
        },

        selectedShapefileName(newValue) {
            this.shapefileName = newValue

            this.map.shapefile = newValue?.key
        },
    },
}
</script>

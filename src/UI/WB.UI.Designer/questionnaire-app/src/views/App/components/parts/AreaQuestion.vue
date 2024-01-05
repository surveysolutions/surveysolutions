<template>
    <div class="row">
        <div class="col-xs-12">
            <label class="wb-label" for="geometry-type">{{ $t('QuestionnaireEditor.GeometryType') }}</label>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <div class="dropdown-with-breadcrumbs-and-icons">
                <div class="btn-group" uib-dropdown>
                    <button class="btn dropdown-toggle" id="geometry-type" uib-dropdown-toggle type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ currentGeometryType }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu dropdown-menu-right" role="menu" aria-labelledby="geometry-type">
                        <li role="presentation" v-for="qtype in activeQuestion.geometryTypeOptions">
                            <a role="menuitem" tabindex="-1" @click="changeGeometryType(qtype.value)">
                                {{ qtype.text }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>

    <div class="row">
        <div class="col-xs-12">
            <label class="wb-label" for="geometry-input-mode">{{ $t('QuestionnaireEditor.GeometryInputMode') }}</label>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <div class="dropdown-with-breadcrumbs-and-icons">
                <div class="btn-group" uib-dropdown>
                    <button class="btn dropdown-toggle" id="geometry-input-mode" uib-dropdown-toggle type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ currentGeometryInputMode }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu dropdown-menu-right" role="menu" aria-labelledby="geometry-input-mode">
                        <li role="presentation" v-for="mode in getGeometryInputModeOptions">
                            <a role="menuitem" tabindex="-1" @click="changeGeometryInputMode(mode.value)">
                                {{ mode.text }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row">
        <div class="col-md-5">
            <div class="checkbox checkbox-in-column">
                <input id="cb-show-neighbours" type="checkbox" class="wb-checkbox"
                    v-model="activeQuestion.geometryOverlapDetection" />
                <label for="cb-show-neighbours"><span></span>{{ $t('QuestionnaireEditor.GeometryOverlapDetection') }}
                </label>
                <help link="HelpOverlapDetection" />
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'
import { geometryInputModeOptions } from '../../../../helpers/question'

export default {
    name: 'AreaQuestion',
    components: {
        Help,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            valid: true,
            dirty: false,
        }
    },
    computed: {
        currentGeometryType() {
            const option = this.activeQuestion.geometryTypeOptions.find(
                p => p.value == this.activeQuestion.geometryType
            );
            return option != null ? option.text : null;
        },
        currentGeometryInputMode() {
            const option = geometryInputModeOptions.find(
                p => p.value == this.activeQuestion.geometryInputMode
            );
            return option != null ? option.text : null;
        },
        getGeometryInputModeOptions() {
            return geometryInputModeOptions;
        }
    },
    methods: {
        changeGeometryType(geometry) {
            this.activeQuestion.geometryType = geometry;

            this.markFormAsChanged();
        },

        changeGeometryInputMode(mode) {
            this.activeQuestion.geometryInputMode = mode;

            this.markFormAsChanged();
        },

        markFormAsChanged() {
            this.dirty = true;
        }
    }
}
</script>

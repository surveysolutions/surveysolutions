<template>
    <div class="metadata">
        <perfect-scrollbar class="scroller">
            <h3>{{ $t('QuestionnaireEditor.SideBarMetadataHeader') }}</h3>

            <form role="form" name="metadataForm" novalidate>
                <ng-form name="metadata.form">

                    <ul class="list-unstyled metadata-blocks">
                        <li>
                            <h4>{{ $t('QuestionnaireEditor.SideBarMetadataBasicInfo') }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataTitle') }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="title" required="" minlength="1"
                                        v-model="metadata.title" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataSubtitle') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="subTitle"
                                        v-model="metadata.subTitle"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataVersionIdentificator')
                                }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="version" v-model="metadata.version">
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataVersionNotes') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="versionNotes"
                                        v-model="metadata.versionNotes"></textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ $t('QuestionnaireEditor.SideBarMetadataSurveyDataInfo') }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataStudyTypes') }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.studyType }">
                                        <button class="btn btn-default dropdown-toggle" type="button"
                                            data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                            {{ studyTypeTitle }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.studyType = null; setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li v-for="studyType in questionnaire.studyTypes">
                                                        <a @click="metadata.studyType = studyType.code; setDirty();"
                                                            :value="studyType.code" href="#">{{ studyType.title }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div>
                                    <input type="hidden" class="form-control" name="studyType"
                                        v-model="metadata.studyType" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataKindOfData') }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.kindOfData }">
                                        <button class="btn btn-default dropdown-toggle" type="button"
                                            data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                            {{ kindOfDataTitle }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.kindOfData = null; setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                            <ul class="scroller list-unstyled">
                                                <li v-for="kindOfData in questionnaire.kindsOfData">
                                                    <a @click="metadata.kindOfData = kindOfData.code; setDirty();"
                                                        :value="kindOfData.code" href="#">{{ kindOfData.title }}</a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" name="kindOfData" v-model="metadata.kindOfData" />
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataModeOfDataCollection')
                                }}</span>

                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.modeOfDataCollection }">
                                        <button class="btn btn-default dropdown-toggle" type="button"
                                            data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                            {{ modeOfDataCollectionTitle }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.modeOfDataCollection = null; setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                            <ul class="scroller list-unstyled">
                                                <li v-for="modeOfData in questionnaire.modesOfDataCollection">
                                                    <a @click="metadata.modeOfDataCollection = modeOfData.code; setDirty();"
                                                        :value="modeOfData.code" href="#">{{ modeOfData.title }}</a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" id="modeOfDataCollection"
                                    name="modeOfDataCollection" v-model="metadata.modeOfDataCollection" />
                            </div>
                        </li>
                        <li>
                            <h4>{{ $t('QuestionnaireEditor.SideBarMetadataSurveyInfo') }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataCountry') }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.country }">
                                        <button class="btn btn-default dropdown-toggle" type="button"
                                            data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                            {{ countryTitle }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.country = null; setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li v-for="country in questionnaire.countries">
                                                        <a @click="metadata.country = country.code; setDirty();"
                                                            :value="country.code" href="#">{{ country.title }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" name="country" v-model="metadata.country" />
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataYear') }}</span>
                                <div class="form-group">
                                    <input type="text" min="0" max="9999" pattern="\d*" @keypress='onKeyPressYear($event)'
                                        maxlength="4" v-pattern="/^\d+$/" class="form-control date-field" name="year"
                                        v-model="metadata.year" />
                                </div>
                                <p class="help-block ng-cloak" v-show="isInteger(metadata.year)">{{
                                    $t('QuestionnaireEditor.QuestionOnlyInts') }}
                                </p>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataLanguages') }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="language" v-model="metadata.language" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataUnitOfAlalysis')
                                }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="unitOfAnalysis"
                                        v-model="metadata.unitOfAnalysis"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataCoverage') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="coverage"
                                        v-model="metadata.coverage"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataUniverse') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="universe"
                                        v-model="metadata.universe"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataPrimaryInvestigator')
                                }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="primaryInvestigator"
                                        v-model="metadata.primaryInvestigator"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataConsultants') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="consultant"
                                        v-model="metadata.consultant">
                                            </textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataFunding') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="funding" v-model="metadata.funding">
                                            </textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ $t('QuestionnaireEditor.SideBarMetadataAdditionalInfo') }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataNotes') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="notes"
                                        v-model="metadata.notes"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ $t('QuestionnaireEditor.SideBarMetadataKeywords') }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="keywords"
                                        v-model="metadata.keywords"></textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ $t('QuestionnaireEditor.SideBarMetadataQuestionnaireAccess') }}</h4>
                            <div class="checkbox">
                                <input id="agreeToMakeThisQuestionnairePublic" name="agreeToMakeThisQuestionnairePublic"
                                    v-model="metadata.agreeToMakeThisQuestionnairePublic" type="checkbox"
                                    class="checkbox-filter" checked>
                                <label for="agreeToMakeThisQuestionnairePublic" class=""><span class="tick"></span>{{
                                    $t('QuestionnaireEditor.SideBarMetadataAgreeToMakeThisQuestionnairePublic') }}</label>
                            </div>
                        </li>
                    </ul>
                    <div class="form-buttons-holder" :class="{ dirty: dirty }">
                        <button type="button" class="btn btn-lg ng-isolate-scope"
                            :disabled="questionnaire.isReadOnlyForUser || invalid" :class="{ 'btn-primary': dirty }"
                            @click.self="saveMetadata()">{{ $t('QuestionnaireEditor.Save') }}</button>
                        <button type="button" class="btn btn-lg btn-link ng-isolate-scope" @click.self="cancelMetadata()">{{
                            $t('QuestionnaireEditor.Cancel')
                        }}</button>
                    </div>
                </ng-form>
            </form>
        </perfect-scrollbar>

    </div>
</template>
  
<script>

import { isInteger } from '../../../../helpers/number';

import { useQuestionnaireStore } from '../../../../stores/questionnaire';

export default {
    name: 'Metadata',
    props: {},
    data() {
        return {
            dirty: false
        }
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    watch: {
        metadata: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.dirty = true;
            },
            deep: true
        },
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.getInfo || {};
        },

        metadata() {
            return this.questionnaireStore.getEdittingMetadata || {};
        },

        invalid() {
            return !this.isInteger(this.metadata.year);
        },

        countryTitle() {
            if (!this.metadata.country) return this.$t('QuestionnaireEditor.SelectCountry');

            const option = this.questionnaire.countries.find(
                p => p.code == this.metadata.country
            );
            return option != null ? option.title : null;
        },

        kindOfDataTitle() {
            if (!this.metadata.kindOfData) return this.$t('QuestionnaireEditor.SelectKindOfData');

            const option = this.questionnaire.kindsOfData.find(
                p => p.code == this.metadata.kindOfData
            );
            return option != null ? option.title : null;
        },

        studyTypeTitle() {
            if (!this.metadata.studyType) return this.$t('QuestionnaireEditor.SelectStudyType');

            const option = this.questionnaire.studyTypes.find(
                p => p.code == this.metadata.studyType
            );
            return option != null ? option.title : null;
        },

        modeOfDataCollectionTitle() {
            if (!this.metadata.modeOfDataCollection) return this.$t('QuestionnaireEditor.SelectModeOfDataCollection');

            const option = this.questionnaire.modesOfDataCollection.find(
                p => p.code == this.metadata.modeOfDataCollection
            );
            return option != null ? option.title : null;
        }
    },
    methods: {
        onKeyPressYear(keyEvent) {
            const charCode = (keyEvent.which) ? keyEvent.which : keyEvent.keyCode;
            if ((charCode > 31 && (charCode < 48 || charCode > 57))
            ) {
                keyEvent.preventDefault();;
            } else {
                return true;
            }
        },

        isInteger(value) {
            return isInteger(value);
        },

        async saveMetadata() {
            await this.questionnaireStore.saveMetadataChanges();
            this.dirty = false;
        },

        async cancelMetadata() {
            await this.questionnaireStore.discardMetadataChanges();
            this.dirty = false;
        },

        setDirty() {
            this.dirty = true;
        }
    },
}
</script>
  
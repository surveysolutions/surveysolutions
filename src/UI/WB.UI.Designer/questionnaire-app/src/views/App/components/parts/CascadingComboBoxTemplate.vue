<template>
    <div id="sourceOfLinkedEntity" class="dropdown-with-breadcrumbs-and-icons"
        :class="{ 'has-error': !isLinkedToEntityValid }">
        <input type="hidden" name="cascadeFromQuestionId" v-model="activeQuestion.cascadeFromQuestionId" />
        <label for="singleOptionQuestionSource">{{ $t('QuestionnaireEditor.SelectParentQuestion') }}</label>
        <div class="btn-group dropdown" uib-dropdown>
            <button class="btn dropdown-toggle" uib-dropdown-toggle id="singleOptionQuestionSource" type="button"
                data-bs-toggle="dropdown" aria-expanded="false">
                <span class="select-placeholder" v-if="!cascadeFromQuestion">{{
            $t('QuestionnaireEditor.SelectQuestion') }}</span>

                <div class="selected-item" v-if="cascadeFromQuestion">
                    <span class="path">{{ cascadeFromQuestion.breadcrumbs }}</span>
                    <div class="selected-block">
                        <div>
                            <span class="chosen-item "><i class="dropdown-icon"
                                    :class="['icon-' + cascadeFromQuestion.type]"></i>
                                {{ cascadeFromQuestion.title }}&nbsp;</span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" v-dompurify-html="cascadeFromQuestion.varName"></span>
                        </div>
                    </div>
                </div>

                <span class="dropdown-arrow"></span>
            </button>

            <ul class="dropdown-menu" role="menu">
                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                    v-for="(breadCrumb, index) in activeQuestion.sourceOfSingleQuestions">
                    <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>
                    <a v-if="!breadCrumb.isSectionPlaceHolder" @click="setCascadeSource(breadCrumb.id)" role="menuitem"
                        tabindex="-1" href="javascript:void(0);">
                        <div>
                            <i :class="['dropdown-icon', 'icon-' + breadCrumb.type]"></i>
                            <span v-dompurify-html="breadCrumb.title"></span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" v-dompurify-html="breadCrumb.varName"></span>
                        </div>
                    </a>
                </li>
            </ul>
        </div>
        <p class="help-block ng-cloak" v-show="!isExistsCascadeFromQuestionId">{{
            $t('QuestionnaireEditor.SelectParentQuestionError') }}</p>
    </div>
    <div class="row">
        <div class="col-md-6">
            <div class="checkbox checkbox-in-column">
                <input id="cb-as-list" type="checkbox" class="wb-checkbox" v-model="activeQuestion.showAsList" />
                <label for="cb-as-list"><span></span>{{ $t('QuestionnaireEditor.ShowAsList') }}</label>
            </div>
        </div>

        <div class="col-md-5 inline-inputs">
            <div class="form-group checkbox checkbox-in-column" v-show="activeQuestion.showAsList"
                :class="{ 'has-error': !isValidShowAsListThreshold }">
                <label for="edit-question-count-for-list">{{ $t('QuestionnaireEditor.QuestionShowListLimit') }}</label>
                <input id="edit-question-count-for-list" type="text" inputmode="numeric" step="1" maxlength="2"
                    name="showAsListThreshold" min="1" max="50" v-model.number="activeQuestion.showAsListThreshold"
                    v-number class="form-control small-numeric-input wb-input">
                <p class="help-block ng-cloak" v-show="!isValidNumberShowAsListThreshold">{{
            $t('QuestionnaireEditor.QuestionOnlyInts') }}</p>
                <p class="help-block ng-cloak" v-show="!isValidMaxMin">{{
            $t('QuestionnaireEditor.QuestionOneToFiftyAllowed') }}</p>
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'
import { isInteger } from '../../../../helpers/number';
import { find, isEmpty } from 'lodash'
import { useQuestionStore } from '../../../../stores/question'

export default {
    name: 'CascadingComboBoxTemplate',
    components: {
        Help,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            isValidShowAsListThreshold: true,
            isValidNumberShowAsListThreshold: true,
            isValidMaxMin: true,
        }
    },
    setup() {
        const questionStore = useQuestionStore();

        return {
            questionStore
        };
    },
    computed: {
        isValidNumberShowAsListThreshold() {
            return this.activeQuestion.showAsListThreshold == null || this.activeQuestion.showAsListThreshold == '' || isInteger(this.activeQuestion.showAsListThreshold);
        },
        isValidMaxMin() {
            if (this.activeQuestion.showAsListThreshold == null || this.activeQuestion.showAsListThreshold == '')
                return true;

            return this.activeQuestion.showAsListThreshold <= 50 && this.activeQuestion.showAsListThreshold >= 1;
        },
        isValidShowAsListThreshold() {
            return this.isValidNumberShowAsListThreshold && this.isValidMaxMin;
        },
        isExistsCascadeFromQuestionId() {
            return this.activeQuestion.cascadeFromQuestionId != null && this.activeQuestion.cascadeFromQuestionId != '';
        },
        valid() {
            return this.isValidShowAsListThreshold && this.isExistsCascadeFromQuestionId;
        },
        isLinkedToEntityValid() {
            return this.activeQuestion.linkedToEntityId != null && this.questionStore.getLinkedSource(this.activeQuestion.linkedToEntityId) != null;
        },
        cascadeFromQuestion() {
            if (this.activeQuestion.cascadeFromQuestionId == null)
                return null;

            const sourceQuestion = find(this.activeQuestion.sourceOfSingleQuestions,
                p => p.id == this.activeQuestion.cascadeFromQuestionId != null && p.isSectionPlaceHolder != true
            );
            return sourceQuestion != undefined ? sourceQuestion : null;
        }
    },
    methods: {
        setCascadeSource(itemId) {
            if (itemId) {
                this.activeQuestion.cascadeFromQuestionId = itemId;
            }
        }
    }
}
</script>

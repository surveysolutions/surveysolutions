<template>
    <div class="form-group" :class="{ 'has-error': !isLinkedToEntityValid }">
        <div id="sourceOfLinkedEntity" class="dropdown-with-breadcrumbs-and-icons">
            <input type="hidden" name="linkedToEntity" v-model="activeQuestion.linkedToEntityId" ng-required="true" />
            <label for="linkedEntitySource">{{ $t('QuestionnaireEditor.QuestionLinkedDescr') }}</label>
            <div class="btn-group" uib-dropdown>
                <button class="btn dropdown-toggle" uib-dropdown-toggle id="linkedEntitySource" type="button"
                    data-bs-toggle="dropdown" aria-expanded="false">
                    <span class="select-placeholder" v-if="(linkedToEntity.title || '') == ''">{{
                        $t('QuestionnaireEditor.SelectQuestion') }}</span>

                    <span class="selected-item" v-if="(linkedToEntity.title || '') !== ''">
                        <span class="path">{{ linkedToEntity.breadcrumbs }}</span>
                        <span class="chosen-item" v-if="linkedToEntity.type !== 'roster'">
                            <i class="dropdown-icon" :class="['icon-' + linkedToEntity.type]"></i>
                            {{ linkedToEntity.title /*| escape*/ }} (<span class="var-name-line">{{
                                linkedToEntity.varName }}</span>)
                        </span>
                        <span class="linked-roster-source" v-if="linkedToEntity.type === 'roster'">
                            {{ linkedToEntity.title /*| escape*/ }} (<span class="var-name-line">{{
                                linkedToEntity.varName }}</span>)
                        </span>
                    </span>
                    <span class="dropdown-arrow"></span>
                </button>

                <ul class="dropdown-menu" role="menu">
                    <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                        v-for="(breadCrumb, index) in activeQuestion.sourceOfLinkedEntities">
                        <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>

                        <a v-if="!breadCrumb.isSectionPlaceHolder && breadCrumb.type === 'roster'"
                            @click="setLinkSource(breadCrumb.id, activeQuestion.linkedFilterExpression, activeQuestion.optionsFilterExpression)"
                            role="menuitem" tabindex="-1" class="linked-roster-source" href="javascript:void(0);">
                            <div>
                                <i class="dropdown-icon" :class="['icon-' + breadCrumb.questionType || 'none']"></i>
                                <span v-dompurify-html="breadCrumb.title"></span>
                            </div>
                            <div class="var-block">
                                <span class="var-name" v-text="breadCrumb.varName"></span>
                            </div>
                        </a>

                        <a v-if="!breadCrumb.isSectionPlaceHolder && breadCrumb.type !== 'roster'"
                            @click="setLinkSource(breadCrumb.id, activeQuestion.linkedFilterExpression, activeQuestion.optionsFilterExpression)"
                            role="menuitem" tabindex="-1" class="linked-question-source" href="javascript:void(0);">
                            <div>
                                <i class="dropdown-icon" :class="['icon-' + breadCrumb.type]"></i>
                                <span v-dompurify-html="breadCrumb.title"></span>
                            </div>
                            <div class="var-block">
                                <span class="var-name" v-text="breadCrumb.varName"></span>
                            </div>
                        </a>
                    </li>
                </ul>
            </div>
            <p class="help-block ng-cloak" v-show="!isLinkedToEntityValid">{{
                $t('QuestionnaireEditor.QuestionMustBeBound') }}</p>
        </div>
    </div>
</template>

<script>

import { isEmpty } from 'lodash'

export default {
    name: 'LinkTemplate',
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        };
    },
    computed: {
        linkedToEntity() {
            if (this.activeQuestion.linkedToEntityId == null)
                return {};

            const sourceQuestion = this.activeQuestion.sourceOfLinkedEntities.find(
                p => p.id == this.activeQuestion.linkedToEntityId && p.isSectionPlaceHolder != true
            );
            return sourceQuestion != undefined ? sourceQuestion : {};
        },
        isLinkedToEntityValid() {
            return this.activeQuestion.linkedToEntityId != null && this.activeQuestion.linkedToEntityId != undefined;
        },
    },
    methods: {
        setLinkSource(itemId, linkedFilterExpression, optionsFilterExpression) {

            if (itemId) {
                this.activeQuestion.linkedToEntityId = itemId;
                const linkedToEntity = find(this.sourceOfLinkedEntities, { id: this.activeQuestion.linkedToEntityId });

                var filter = linkedFilterExpression || optionsFilterExpression;
                if (linkedToEntity !== undefined && linkedToEntity.type === 'textlist') {
                    this.activeQuestion.linkedFilterExpression = null;
                    this.activeQuestion.optionsFilterExpression = filter;
                } else {
                    this.activeQuestion.linkedFilterExpression = filter;
                    this.activeQuestion.optionsFilterExpression = null;
                }
            }
            else {
                this.activeQuestion.linkedToEntityId = null;
            }
        },
    }
};
</script>

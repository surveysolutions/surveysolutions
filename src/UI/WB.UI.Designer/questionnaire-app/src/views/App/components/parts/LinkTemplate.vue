<template>
    <div id="sourceOfLinkedEntity" class="dropdown-with-breadcrumbs-and-icons">
        <input type="hidden" name="linkedToEntity" v-model="activeQuestion.linkedToEntity" ng-required="true" />
        <label for="linkedEntitySource" v-t="{ path: 'QuestionnaireEditor.QuestionLinkedDescr' }"></label>
        <div class="btn-group" uib-dropdown>
            <button class="btn dropdown-toggle" uib-dropdown-toggle id="linkedEntitySource" type="button">
                <span class="select-placeholder" v-if="(activeQuestion.linkedToEntity.title || '') == ''"
                    v-t="{ path: 'QuestionnaireEditor.SelectQuestion' }"></span>

                <span class="selected-item" v-if="(activeQuestion.linkedToEntity.title || '') !== ''">
                    <span class="path">{{ activeQuestion.linkedToEntity.breadcrumbs }}</span>
                    <span class="chosen-item" v-if="activeQuestion.linkedToEntity.type !== 'roster'">
                        <i class="dropdown-icon" :class="['icon-' + activeQuestion.linkedToEntity.type]"></i>
                        {{ activeQuestion.linkedToEntity.title | escape }} (<span
                            class="var-name-line">{{ activeQuestion.linkedToEntity.varName }}</span>)
                    </span>
                    <span class="linked-roster-source" v-if="activeQuestion.linkedToEntity.type === 'roster'">
                        {{ activeQuestion.linkedToEntity.title | escape }} (<span
                            class="var-name-line">{{ activeQuestion.linkedToEntity.varName }}</span>)
                    </span>
                </span>
                <span class="dropdown-arrow"></span>
            </button>

            <ul class="dropdown-menu" role="menu">
                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                    v-for="breadCrumb in sourceOfLinkedEntities track by $index">
                    <span v-if="breadCrumb.isSectionPlaceHolder">{{:: breadCrumb.title }}</span>

                    <a v-if="!breadCrumb.isSectionPlaceHolder && breadCrumb.type === 'roster'"
                        @click="setLinkSource(breadCrumb.id, activeQuestion.linkedFilterExpression, activeQuestion.optionsFilterExpression)"
                        role="menuitem" tabindex="-1" class="linked-roster-source" href="javascript:void(0);">
                        <div>
                            <i class="dropdown-icon icon-{{breadCrumb.questionType || 'none'}}"></i>
                            <span v-bind-html="breadCrumb.title | escape"></span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" v-bind-html="breadCrumb.varName"></span>
                        </div>
                    </a>

                    <a v-if="!breadCrumb.isSectionPlaceHolder && breadCrumb.type !== 'roster'"
                        @click="setLinkSource(breadCrumb.id, activeQuestion.linkedFilterExpression, activeQuestion.optionsFilterExpression)"
                        role="menuitem" tabindex="-1" class="linked-question-source" href="javascript:void(0);">
                        <div>
                            <i class="dropdown-icon icon-{{breadCrumb.type}}"></i>
                            <span v-bind-html="breadCrumb.title | escape"></span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" v-bind-html="breadCrumb.varName"></span>
                        </div>
                    </a>
                </li>
            </ul>
        </div>
        <p class="help-block ng-cloak" v-show=" questionForm.linkedToEntity.$error.required "
            v-t=" { path: 'QuestionnaireEditor.QuestionMustBeBound' } ">
        </p>
    </div>
</template>

<script>
export default {
    name: 'LinkTemplate',
    props: {},
    data() {
        return {};
    }
};
</script>

<template>
    Macroses

    <!--div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>
                <span ng-i18next="[i18next]({count: macros.length})SideBarMacroCounter"></span>
            </h3>
            <div class="empty-list" ng-show="macros.length == 0">
                <p ng-i18next>SideBarMacroEmptyLine1</p>
                <p ng-i18next>SideBarMacroEmptyLine2</p>
                <p>
                    <span class="variable-name" ng-i18next="VariableName"></span>
                    {{ 'SideBarMacroEmptyLine3' | i18next }}
                </p>
            </div>
            <form role="form" name="macrosForm" novalidate>
                <ul ng-model="macros">
                    <li class="macros-panel-item" ng-repeat="macro in macros">
                        <ng-form name="macro.form">
                            <a href @click="deleteMacro($index)" ng-disabled="questionnaire.isReadOnlyForUser"
                                ng-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
                            <div class="input-group macros-name">
                                <span class="input-group-addon">$</span>
                                <input focus-on-out="focusMacro{{macro.itemId}}" ng-i18next="[placeholder]SideBarMacroName"
                                    maxlength="32" spellcheck="false" ng-model="macro.name" name="name" class="form-control"
                                    type="text" />
                            </div>
                            <div class="divider"></div>
                            <div ng-model="macro.content"
                                ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }" type="text">
                            </div>
                            <div ng-show="macro.isDescriptionVisible">
                                <div class="divider"></div>
                                <textarea ng-i18next="[placeholder]SideBarMacroDescription" type="text"
                                    ng-model="macro.description" class="form-control macros-description"
                                    msd-elastic></textarea>
                            </div>
                            <div class="actions" ng-show="macro.form.$dirty">
                                <button type="submit" ng-disabled="questionnaire.isReadOnlyForUser || macro.form.$invalid"
                                    class="btn lighter-hover" @click="saveMacro(macro)" ng-i18next>Save</button>
                                <button type="button" class="btn lighter-hover" @click="cancel(macro)"
                                    ng-i18next>Cancel</button>
                                <button class="btn btn-default pull-right" ng-show="isDescriptionEmpty(macro)" type="button"
                                    @click="toggleDescription(macro)"
                                    ng-i18next="{{macro.isDescriptionVisible ? 'SideBarMacroHideDescription' : 'SideBarMacroShowDescription'}}"></button>
                            </div>
                        </ng-form>
                    </li>
                </ul>

            </form>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" ng-disabled="questionnaire.isReadOnlyForUser"
                    ng-i18next="[value]SideBarAddMacro" value="ADD NEW Macro" @click="addNewMacro()">
            </div>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Macroses',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  
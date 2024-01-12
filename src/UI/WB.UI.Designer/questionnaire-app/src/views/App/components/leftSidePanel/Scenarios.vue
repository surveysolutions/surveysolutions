<template>
    Scenarios

    <!--div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>
                <span ng-i18next="[i18next]({count: scenarios.length})SideBarScenarioCounter"></span>
            </h3>
            <div class="empty-list" ng-show="scenarios.length == 0">
                <p ng-i18next>SideBarScenarioEmptyLine1</p>
                <p ng-i18next>SideBarScenarioEmptyLine2</p>
            </div>
            <form role="form" name="scenariosForm" novalidate>
                <ul ng-model="scenarios">
                    <li class="macroses-panel-item" ng-repeat="scenario in scenarios">
                        <ng-form name="scenario.form">
                            <a href="javascript:void(0)" ng-if="!questionnaire.isReadOnlyForUser"
                                @click="deleteScenario($index)" class="btn delete-btn" tabindex="-1"></a>
                            <div class="input-group macroses-name">
                                <input focus-on-out="focusScenario{{scenario.id}}"
                                    ng-i18next="[placeholder]SideBarScenarioName" maxlength="32" spellcheck="false"
                                    ng-model="scenario.title" name="name" class="form-control" type="text" />
                            </div>
                            <div class="divider"></div>
                            <button type="button" class="btn lighter-hover" @click="runScenario(scenario)"
                                ng-i18next>Run</button>
                            <button type="button" class="btn lighter-hover"
                                @click="showScenarioEditor(scenario.id, scenario.title)" ng-i18next>View</button>
                            <div class="actions" ng-show="scenario.form.$dirty" ng-if="!questionnaire.isReadOnlyForUser">
                                <button type="submit"
                                    ng-disabled="questionnaire.isReadOnlyForUser || scenario.form.$invalid"
                                    class="btn lighter-hover" @click="saveScenario(scenario)" ng-i18next>Save</button>
                                <button type="button" class="btn lighter-hover" @click="cancel(scenario)"
                                    ng-i18next>Cancel</button>
                            </div>
                        </ng-form>
                    </li>
                </ul>
            </form>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Scenarios',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  
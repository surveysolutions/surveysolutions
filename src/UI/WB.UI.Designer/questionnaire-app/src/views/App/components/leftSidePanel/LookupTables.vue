<template>
    <div class="lookup-tables">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarLookupTablesCounter', { count: lookupTables.length }) }}</span>
            </h3>
            <div class="empty-list" v-show="lookupTables.length === 0">
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine2') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine3') }}</p>
            </div>
            <form role="form" name="lookupTablesForm" novalidate>
                <ul>
                    <LookupTableItem v-for="table in lookupTables" :tableItem="table" :questionnaire-id="questionnaireId" />
                </ul>
            </form>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" v-if="!isReadOnlyForUser"
                    :value="$t('QuestionnaireEditor.SideBarLookupTableAdd')" value="Add new Lookup table"
                    @click="addNewLookupTable()">
            </div>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import LookupTableItem from './LookupTableItem.vue';
import { addLookupTable } from '../../../../services/lookupTableService'
import { newGuid } from '../../../../helpers/guid';

export default {
    name: 'LookupTables',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: { LookupTableItem },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {}
    },
    computed: {
        lookupTables() {
            return this.questionnaire.lookupTables;
        },
    },
    methods: {
        addNewLookupTable() {
            const newLookupTable = {
                itemId: newGuid()
            };

            addLookupTable(this.questionnaireId, newLookupTable);
        },
    }
}
</script>
  
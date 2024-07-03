<template>
    <ModalFrame ref="modal" id="statusesHistoryModal">
        <template v-slot:title>
            <div>
                <h3>{{ $t("Pages.HistoryOfStatuses_Interview") }} {{ $config.model.key }}</h3>
                <p>
                    {{ this.$t('Details.Responsible') }}: <span
                        v-bind:class="[this.$config.model.responsibleRole.toLowerCase()]">{{
                            this.$config.model.responsible }}</span>
                </p>
            </div>
        </template>
        <h3>{{ $t("Pages.HistoryOfStatuses_Title") }}</h3>
        <div class="table-with-scroll">
            <table class="table table-striped history">
                <thead>
                    <tr>
                        <td>{{ $t("Pages.HistoryOfStatuses_State") }}</td>
                        <td>{{ $t("Pages.HistoryOfStatuses_On") }}</td>
                        <td>{{ $t("Pages.HistoryOfStatuses_By") }}</td>
                        <td>{{ $t("Pages.HistoryOfStatuses_AssignedTo") }}</td>
                        <td>{{ $t("Pages.HistoryOfStatuses_Comment") }}</td>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in items" :key="item.id">
                        <td>{{ item.statusHumanized }}</td>
                        <td class="date">
                            {{ formatDate(item.date) }}
                        </td>
                        <td>
                            <span v-bind:class="[item.responsibleRole]">{{ item.responsible }}</span>
                        </td>
                        <td>
                            <span v-bind:class="[item.assigneeRole]">{{ item.assignee }}</span>
                        </td>
                        <td data-bind="text: Comment">
                            {{ item.comment }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <template v-slot:actions>
            <div>
                <button type="button" class="btn btn-link" @click="hide">
                    {{ $t("Pages.CloseLabel") }}
                </button>
            </div>
        </template>
    </ModalFrame>
</template>

<script>
import { DateFormats } from '~/shared/helpers'
//import Vue from 'vue'
//TODO: MIGRATION
import moment from 'moment'

export default {
    data: function () {
        return {
            items: null,
        }
    },
    methods: {
        formatDate(d) {
            return moment.utc(d).local().format(DateFormats.dateTimeInList)
        },
        hide() {
            $(this.$refs.modal).modal('hide')
        },
        async show() {
            if (this.items == null) {
                this.items = await this.$api.interview.get('getStatusesHistory')
            }

            this.$refs.modal.modal()
        },
    },
}
</script>

<template>
    <ModalFrame ref="modal" id="statusesHistoryModal">
        <div slot="title">
            <h3>{{$t("Pages.HistoryOfStatuses_Interview")}} {{$config.model.key}}</h3>
            <p>
                {{this.$t('Details.Responsible')}}: <span v-bind:class="[this.$config.model.responsibleRole.toLowerCase()]">{{this.$config.model.responsible}}</span>
            </p>
        </div>
        <h3>{{$t("Pages.HistoryOfStatuses_Title")}}</h3>
        <div class="table-with-scroll">
            <table class="table table-striped history">
                <thead>
                    <tr>
                        <td>{{$t("Pages.HistoryOfStatuses_State")}}</td>
                        <td>{{$t("Pages.HistoryOfStatuses_On")}}</td>
                        <td>{{$t("Pages.HistoryOfStatuses_By")}}</td>
                        <td>{{$t("Pages.HistoryOfStatuses_AssignedTo")}}</td>
                        <td>{{$t("Pages.HistoryOfStatuses_Comment")}}</td>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in items" :key="item.id">
                        <td>{{item.StatusHumanized}}</td>
                        <td class="date">{{formatDate(item.Date)}}</td>
                        <td>
                            <span v-bind:class="[item.ResponsibleRole]">{{item.Responsible}}</span>
                        </td>
                        <td>
                            <span v-bind:class="[item.AssigneeRole]">{{item.Assignee}}</span>
                        </td>
                        <td data-bind="text: Comment">{{item.Comment}}</td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div slot="actions">
            <button type="button" class="btn btn-link" @click="hide">{{ $t("Pages.CloseLabel") }}</button>
        </div>
    </ModalFrame>
</template>

<script>
import { DateFormats } from "~/shared/helpers";
import vue from "vue"

export default {
  data: function() {
    return {
      items: null
    };
  },
  methods: {
    formatDate(d) {
      return moment(d).format(DateFormats.dateTimeInList);
    },
    hide() {
      $(this.$refs.modal).modal("hide");
    },
    async show() {
      if (this.items == null)
        this.items = await vue.$api.call(api => api.getStatusesHistory());
        
      this.$refs.modal.modal();
    }
  }
};
</script> 

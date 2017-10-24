<<template>
  <ModalFrame id="statusHistoryModal">
                <div slot="title">
                <h3>{{$t("Pages.HistoryOfStatuses_Interview")}}</h3>
                <p>
                    {{$config.model.key}} by <span v-bind:class="[this.$config.model.responsibleRole.toLowerCase()]">{{this.$config.model.responsible}}</span>
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
                            <tr v-for="item in $store.state.webinterview.statusesHistory">
                                <td>{{item.StatusHumanized}}</td>
                                <td class="date">{{formatDate(item.Date)}}</td>
                                <td><span v-bind:class="[item.ResponsibleRole]">{{item.Responsible}}</span></td>
                                <td><span v-bind:class="[item.AssigneeRole]">{{item.Assignee}}</span></td>
                                <td data-bind="text: Comment">{{item.Comment}}</td>
                            </tr>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div slot="actions">
                <button type="button"
                        class="btn btn-link"
                        data-dismiss="modal">{{ $t("Pages.CloseLabel") }}</button>
            </div>
        </ModalFrame>
</template>

<script>
export default {
  methods: {
    formatDate(d) {
      return moment
        .utc(d)
        .local()
        .format("MMM DD, YYYY HH:mm");
    }
  }
};
</script> 
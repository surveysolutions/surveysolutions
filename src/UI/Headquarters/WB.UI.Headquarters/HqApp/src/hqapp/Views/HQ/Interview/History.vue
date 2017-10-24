<<template>
  <ModalFrame id="statuses" :title="$t('Pages.HistoryOfStatuses_Interview')">
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
                    <tbody data-bind="foreach:statusHistory">
                        <tr>
                            <td data-bind="text: StatusHumanized"></td>
                            <td class="date"
                                data-bind="text: $root.formatDate(Date)"></td>
                            <td>
                                <span data-bind="text: Responsible, attr: { 'class' : ResponsibleRole }"></span>
                            </td>
                            <td>
                                <span data-bind="text: Assignee, attr: { 'class' : AssigneeRole }"></span>
                            </td>
                            <td data-bind="text: Comment"></td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div slot="title">
                <h3>{{$t("Pages.HistoryOfStatuses_Interview")}}</h3>
                <p>
                    <a data-bind="attr: { href: interviewUrl, title: key },text: key"
                       class="title-row"></a> by
                    <span href="#"
                          data-bind="text: responsible, css: { interviewer: isResponsibleInterviewer, supervisor: !isResponsibleInterviewer}"></span>
                </p>
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
    data() {
        return {}
    },
    computed: {
    },
    mounted() {
    }
}

</script> 
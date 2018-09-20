Supervisor.VM.Questionnaires = function (listViewUrl, notifier, ajax, $newInterviewUrl,
    $batchUploadUrl, $cloneQuestionnaireUrl, $deleteQuestionnaireUrl, $webInterviewUrl, $exportQuestionnaireUrl,
    $migrateAssignmentsUrl) {
    Supervisor.VM.Questionnaires.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);

    self.onDataTableDataReceived = function (data) {};

    self.load = function () {

        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
        self.reloadDataTable();
    };

    self.onTableInitCompleteExtra = function () { };

    self.onTableInitComplete = function () {
        self.onTableInitCompleteExtra();
    };

    self.selectRowAndGetData = function(selectedItem) {
        self.Datatable.rows().deselect();
        var rowIndex = selectedItem.parent().children().index(selectedItem);
        self.Datatable.row(rowIndex).select();
        var selectedRows = self.Datatable.rows({ selected: true }).data()[0];
        return selectedRows;
    };

    self.sendDeleteQuestionnaireCommand = function (item) {
        ajax.sendRequest($deleteQuestionnaireUrl, "post", { questionnaireId: item.questionnaireId, version: item.version }, false,
            // onSuccess
            function () {
                setTimeout(function () { self.reloadDataTable(); }, 1000);
            });
    };

    self.addNewInterview = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        window.location.href = $newInterviewUrl + '/' + selectedRow.questionnaireId + '?version=' + selectedRow.version;
    };

    self.webInterviewSetup = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        var questionnaireId = selectedRow.questionnaireId + '$' + selectedRow.version;
        window.location.href = $webInterviewUrl + '/' + encodeURI(questionnaireId);
        console.log(selectedRow);
    };

    self.interviewsBatchUpload = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        window.location.href = $batchUploadUrl + '/' + selectedRow.questionnaireId + '?version=' + selectedRow.version;
    };

    self.migrateAssignments = function(key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        window.location.href = $migrateAssignmentsUrl + '/' + selectedRow.questionnaireId + '?version=' + selectedRow.version;
    }

    self.cloneQuestionnaire = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        window.location.href = $cloneQuestionnaireUrl + '/' + selectedRow.questionnaireId + '?version=' + selectedRow.version;
    };

    self.deleteQuestionnaire = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);

        notifier.confirm('Confirmation Needed', input.settings.messages.deleteQuestionnaireConfirmationMessage,
            // confirm
            function () { self.sendDeleteQuestionnaireCommand(selectedRow); },
            // cancel
            function () { });
    };

    self.exportQuestionnaire = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);
        window.location.href = $exportQuestionnaireUrl + '/' + selectedRow.questionnaireId + '?version=' + selectedRow.version;
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Questionnaires, Supervisor.VM.ListView);

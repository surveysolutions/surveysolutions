function ItemViewModel() {
    var self = this;
    self.itemId = "";
    self.itemName = "";
    self.itemType = "";
    self.pdfStatusUrl = '';

    self.deleteItem = function (id, type, name) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;

        $('#delete-modal-questionnaire-id').val(self.itemId);
        $('#delete-modal-questionnaire-title').html(self.itemName);
    };

    self.exportItemAsPdf = function (id, type, name, pdfDownloadUrl, pdfStatusUrl, pdfRetryUrl) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.pdfStatusUrl = pdfStatusUrl;
        self.pdfDownloadUrl = pdfDownloadUrl;
        self.pdfRetryUrl = pdfRetryUrl;

        $('#export-pdf-modal-questionnaire-id').val(self.itemId);
        $('#export-pdf-modal-questionnaire-title').text(self.itemName);

        $('#pdfDownloadButton').hide();
        $('#pdfRetryGenerate').hide();

        self.updateExportPdfStatusNeverending();
    };

    self.retryPdfExport = function() {
        $.post(self.pdfRetryUrl, { id: self.itemId });
        $('#pdfRetryGenerate').hide();
        self.setPdfMessage("Retrying export as PDF.");
    };

    self.updateExportPdfStatus = function () {
        if (self.pdfStatusUrl == '') return { always: function() {} };
        return $.ajax({
            url: self.pdfStatusUrl,
            cache: false
        }).done(function (result) {
            if (result.Message != null) {
                self.setPdfMessage(result.Message);
            } else {
                self.setPdfMessage("Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.");
            }
            if (result.ReadyForDownload == true) {
                $('#pdfDownloadButton').unbind('click');
                $('#pdfDownloadButton').click(function () {
                    self.pdfStatusUrl = '';
                    window.location = self.pdfDownloadUrl;
                    $('#pdfCancelButton').click();
                });
                $('#pdfDownloadButton').show();
            }
            if (result.CanRetry) {
                $('#pdfRetryGenerate').show();
            } else {
                $('#pdfRetryGenerate').hide();
            }
        }).fail(function (xhr, status, error) {
            self.pdfStatusUrl = '';
            self.setPdfMessage("Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.");
        });
    }

    self.updateExportPdfStatusNeverending = function () {
        $.when(self.updateExportPdfStatus()).always(function () {
            setTimeout(self.updateExportPdfStatusNeverending, 1000);
        });
    }

    self.setPdfMessage = function (message) {
        $('#export-pdf-modal-status').text(
            message
            //+ '\r\n\r\n' + 'Status updated ' + new Date().toLocaleTimeString()
        );
    }
}

$(function () {
    window.questionnaireActionModel = new ItemViewModel();
    
    $('#table-content-holder > .scroller-container').perfectScrollbar();
});
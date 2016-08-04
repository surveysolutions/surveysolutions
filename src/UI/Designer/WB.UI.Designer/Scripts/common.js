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

    self.exportItemAsPdf = function (id, type, name, pdfDownloadUrl, pdfStatusUrl) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.pdfStatusUrl = pdfStatusUrl;

        $('#export-pdf-modal-questionnaire-id').val(self.itemId);
        $('#export-pdf-modal-download-url').attr('href', pdfDownloadUrl);
        //$('#export-pdf-modal-download-url').click(function () {
        //    alert('x');
        //    $("#mExportPdf").modal('hide');
        //    //window.href = pdfDownloadUrl;
        //});
        $('#export-pdf-modal-questionnaire-title').html(self.itemName);
        self.updateExportPdfStatusNeverending();
    };

    self.updateExportPdfStatus = function () {
        if (self.pdfStatusUrl == '') return { always: function() {} };
        return $.ajax({
            url: self.pdfStatusUrl,
            cache: false
        }).done(function (result) {
            $('#export-pdf-modal-status').text('Updated from server: ' + new Date().toTimeString() + '\r\n\r\n' + result);
        }).fail(function (xhr, status, error) {
            self.pdfStatusUrl = '';
            $('#export-pdf-modal-status').text(error + '\r\n' + xhr.responseText + '\r\n\r\nUpdated from server: ' + new Date().toTimeString());
        });
    }

    self.updateExportPdfStatusNeverending = function () {
        $.when(self.updateExportPdfStatus()).always(function () {
            setTimeout(self.updateExportPdfStatusNeverending, 100);
        });
    }
}

$(function () {
    window.questionnaireActionModel = new ItemViewModel();
    
    $('#table-content-holder > .scroller-container').perfectScrollbar();
});
function ItemViewModel() {
    var self = this;
    self.itemId = ko.observable('');
    self.itemName = ko.observable('');
    self.itemType = ko.observable('');

    self.deleteItem = function (id, type, name) {
        var encName = decodeURIComponent(name);
        self.itemName(encName);
        self.itemType(type);
        self.itemId(id);
    };
}

$(function () {
    ko.applyBindings(new ItemViewModel());

    $('#table-content-holder > .scroller-container').perfectScrollbar();
});
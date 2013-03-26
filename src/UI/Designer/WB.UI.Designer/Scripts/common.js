function ItemViewModel() {
    var self = this;
    self.itemId = ko.observable('');
    self.itemName = ko.observable('');
    self.itemType = ko.observable('');

    self.deleteItem = function (id, type, name) {
        self.itemName(name);
        self.itemType(type);
        self.itemId(id);
    };
}

$(function () {
    ko.applyBindings(new ItemViewModel());
});
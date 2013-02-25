define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store'],
    function(ko, _, config, datacontext, router, messenger, store) {
        var
            selectedMenuItem = ko.observable(),
            menu = ko.observableArray(),
            activate = function (routeData, callback) {
                messenger.publish.viewModelActivated({ canleaveCallback: canLeave });
                getMenu();
                setSelectedMenuItem(routeData);
                debugger;
            },
            canLeave = function() {
                return true;
            },
             getMenu = function () {
                 if (!menu().length) {
                     menu(datacontext.menu.getAllLocal());
                 }
             },
             setSelectedMenuItem = function (data) {
                 var value = data.group || selectedMenuItem();
                 selectedMenuItem(value);
                 selectedMenuItem.valueHasMutated();
             },
            syncSelectedMenuItemWithIsSelected = function (value) {
                for (var i = 0, len = menu().length; i < len; i++) {
                    var menuItem = menu()[i];
                    menuItem.isSelected(false);
                    if (menuItem.id() === selectedMenuItem()) {
                        menuItem.isSelected(true);
                        return;
                    }
                }
            },
            init = function () {
                 selectedMenuItem.subscribe(syncSelectedMenuItemWithIsSelected);
             };

        init();

        return {
            activate: activate,
            menu: menu
        };
    });
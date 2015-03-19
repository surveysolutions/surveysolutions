(function () {
    angular.module('designerApp')
        .factory('utilityService', [
            '$rootScope', '$timeout',
            function ($rootScope, $timeout) {
                var utilityService = {};

                utilityService.guid = function () {
                    function s4() {
                        return Math.floor((1 + Math.random()) * 0x10000)
                            .toString(16)
                            .substring(1);
                    }

                    return s4() + s4() + s4() + s4() +
                        s4() + s4() + s4() + s4();
                };

                utilityService.format = function (format) {
                    var args = Array.prototype.slice.call(arguments, 1);
                    return format.replace(/{(\d+)}/g, function (match, number) {
                        return typeof args[number] !== 'undefined'
                            ? args[number]
                            : match;
                    });
                };

                utilityService.focus = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOn', name);
                    });
                };
                
                utilityService.focusout = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOut', name);
                    });
                };
                
                utilityService.createQuestionForDeleteConfirmationPopup = function(title) {
                    var trimmedTitle = title.substring(0, 25) + (title.length > 25 ? "..." : "");
                    var message = 'Are you sure you want to delete "' + trimmedTitle + '"?';
                    return {
                        title: message,
                        okButtonTitle: "DELETE",
                        cancelButtonTitle: "BACK TO DESIGNER"
                    };
                };

                utilityService.createEmptyGroup = function (parent) {
                    var newId = utilityService.guid();
                    var emptyGroup = {
                        "itemId": newId,
                        "title": "New group",
                        "items": [],
                        itemType: 'Group',
                        getParentItem: function () { return parent; }
                    };
                    return emptyGroup;
                };

                utilityService.createEmptyRoster = function (parent) {
                    var newId = utilityService.guid();
                    var emptyRoster = {
                        "itemId": newId,
                        "title": "New roster",
                        "items": [],
                        itemType: 'Group',
                        isRoster: true,
                        getParentItem: function () { return parent; }
                    };
                    return emptyRoster;
                };

                utilityService.createEmptyQuestion = function (parent) {
                    var newId = utilityService.guid();
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": '',
                        "type": 'Text',
                        itemType: 'Question',
                        getParentItem: function () { return parent; }
                    };
                    return emptyQuestion;
                };

                utilityService.createEmptyStaticText = function (parent) {
                    var newId = utilityService.guid();
                    var emptyStaticText = {
                        "itemId": newId,
                        "text": "New static text",
                        itemType: 'StaticText',
                        getParentItem: function () { return parent; }
                    };
                    return emptyStaticText;
                };

                return utilityService;
            }
        ]);
})();
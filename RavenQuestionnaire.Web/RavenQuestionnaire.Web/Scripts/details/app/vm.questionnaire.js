define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store', 'model'],
    function(ko, _, config, datacontext, router, messenger, store, model) {
        var selectedMenuItem = ko.observable(),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            menu = ko.observableArray(),
            chapters = ko.observableArray(),
            
            isInitialized = false;
            activate = function (routeData, callback) {
                messenger.publish.viewModelActivated({ canleaveCallback: canLeave });
                getMenu();
                
                if (!isInitialized) {
                    getChapters();
                    questionnaire(datacontext.questionnaire);
                     $('#groups .body').css('top', ($('#groups .title').outerHeight() + 'px'));
                }
                if (routeData.has('question')) {
                    editQuestion(routeData.question);
                }
                if (routeData.has('group')) {
                    editGroup(routeData.group);
                    setSelectedMenuItem(routeData.group);
                }
                isInitialized = true;
            },
            canLeave = function() {
                return true;
            },
            getChapters = function () {
                 if (!chapters().length) {
                     chapters(datacontext.groups.getChapters());
                 }
             },
            getMenu = function () {
                if (!menu().length) {
                    menu(datacontext.menu.getAllLocal());
                }
            },
             setSelectedMenuItem = function (data) {
                 var value = data || selectedMenuItem();
                 selectedMenuItem(value);
                 selectedMenuItem.valueHasMutated();
             },
            syncSelectedMenuItemWithIsSelected = function (value) {
                _.forEach(menu(), function(item) {
                    item.isSelected(false);
                });

                for (var i = 0; i < menu().length; i++) {
                    var item = menu()[i];
                    if (item.id() == selectedMenuItem()) {
                        item.isSelected(true);
                        break;
                    }
                }
            },
            editQuestion = function(id) {
                var question = datacontext.questions.getLocalById(id);
                question.localPropagatedGroups(datacontext.groups.getPropagateableGroups());
                selectedQuestion(question);
                openDetails("show-question");
            },
            editGroup = function (id) {
                var group = datacontext.groups.getLocalById(id);
                
                selectedGroup(group);
                openDetails("show-group");
            },
            openDetails = function (style) {
                $('#stacks').removeClass("show-question").removeClass("show-group");
                $('#stacks').addClass('detail-visible').addClass(style);
                $('#details-question .body').css('top', ($('#details-question .title').outerHeight() + 'px'));
                $('#details-group .body').css('top', ($('#details-group .title').outerHeight() + 'px'));
            },
            closeDetails = function () {
                $('#stacks').removeClass("show-question").removeClass("show-group");
                $('#stacks').removeClass('detail-visible');
            },
            closeMenu = function () {
                $('#stacks').addClass('menu-hidden');
            },
            showMenu = function () {
                 $('#stacks').removeClass('menu-hidden');
            },
            addQuestion = function (parent) {
                console.log(parent);
                var question = model.Question.Nullo;
                question.id(Math.uuid());
                
                parent.childrenID.push({type:question.type(), id :question.id() });
                parent.fillChildren();
                datacontext.questions.add(question);
            },
            addGroup = function (parent) {
                console.log(parent);
                var group = model.Group.Nullo;
                group.id(Math.uuid());
                debugger;
                parent.childrenID.push({type :group.type(), id :group.id() });
                parent.fillChildren();
                datacontext.groups.add(group);
                
            },
            init = function () {
                selectedMenuItem.subscribe(syncSelectedMenuItemWithIsSelected);
             };

        init();

        return {
            activate: activate,
            menu: menu,
            questionnaire:questionnaire,
            chapters: chapters,
            selectedGroup : selectedGroup,
            selectedQuestion: selectedQuestion,
            closeDetails: closeDetails,
            closeMenu: closeMenu,
            showMenu: showMenu,
            addQuestion: addQuestion,
            addGroup : addGroup
        };
    });
define('model.group',
    ['ko', 'config'],
    function(ko, config) {

        var _dc = null,
            Group = function() {
                var self = this;
                self.id = ko.observable(Math.uuid());
                self.isNew = ko.observable(true);

                self.title = ko.observable('New Group').extend({ required: true });
                self.parent = ko.observable();

                self.type = ko.observable("GroupView"); // Object type
                self.template = "GroupView"; // inner html template name
                self.gtype = ko.observable("None"); // Group type

                self.level = ko.observable();
                self.description = ko.observable('');
                self.condition = ko.observable('');
                
                self.children = ko.observableArray();
                self.childrenID = ko.observableArray();
                
                // UI stuff
                self.tip = ko.computed(function () {
                    if (self.isNew()) return config.tips.newGroup;
                    return null;
                });
                
                self.getHref = function () {
                    return config.hashes.detailsGroup + "/" + self.id();
                };

                self.isSelected = ko.observable();
                self.typeOptions = config.groupTypes;
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.gtype, self.description, self.condition]);
                self.dirtyFlag().reset();
                
                self.errors = ko.validation.group(self);

                return self;
            };
        
        Group.datacontext = function(dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        var BaseGroup = function() {
            var dc = Group.datacontext,
                fillChildren = function () {
                     var items =_.map(this.childrenID(), function (item) {
                        if (item.type === "GroupView")
                            return dc().groups.getLocalById(item.id);
                        return dc().questions.getLocalById(item.id);
                     });
                     this.children(items);
                     this.children.id = this.id();
                    //return self.children();
                };
            return {
                isNullo: false,
                fillChildren: fillChildren
            };
        };


        Group.prototype = new BaseGroup();

        Group.Nullo = new Group().id(0).title('Title').type('GroupView');
        Group.Nullo.isNullo = true;
        Group.Nullo.dirtyFlag().reset();

        return Group;
    });

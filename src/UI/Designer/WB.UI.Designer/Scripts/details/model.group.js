define('model.group',
    ['ko', 'config', 'utils', 'validator'],
    function (ko, config, utils, validator) {

        var _dc = null,
            Group = function() {
                var self = this;
                self.id = ko.observable(Math.uuid());
                self.isNew = ko.observable(true);
                self.isClone = ko.observable(false);
                
                self.title = ko.observable('New Group').extend({ required: true });
                self.parent = ko.observable();

                self.type = ko.observable("GroupView"); // Object type
                self.template = "GroupView"; // inner html template name

                self.isRoster = ko.observable(false);
                self.rosterSizeQuestion = ko.observable().extend({
                    required: {
                        onlyIf: function () {
                            if (self.isRoster()) {
                                self.rosterSizeQuestion.valueHasMutated();
                                return true;
                            }
                            return false;
                        }
                    }
                });
                
                self.allowedQuestions = ko.observableArray([]);

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
                    return utils.groupUrl(self.id());
                };
                self.isExpanded = ko.observable(true);
                self.allowDrop = ko.computed(function() {
                    if (self.isExpanded()) return true;
                    return false;
                });
                self.cloneSource = ko.observable();
                self.isSelected = ko.observable();
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.description, self.condition, self.isRoster, self.rosterSizeQuestion]);
                self.dirtyFlag().reset();
                
                self.errors = ko.validation.group(self);
                self.hasErrors = ko.computed(function() {
                    return self.errors().length > 0;
                });
                self.canUpdate = ko.observable(true);
                this.cache = function () { };
                
                self.attachValidation = function () {
                    self.condition.extend({
                        validation: [
                        {
                            validator: function (val) {
                                var validationResult = validator.isValidExpression(val);
                                if (validationResult.isValid) {
                                    return true;
                                }
                                this.message = validationResult.errorMessage;
                                return false;
                            },
                            message: 'Error'
                        },
                        {
                            validator: function (val) {
                                if (_.isUndefined(val) || _.isNull(val)) {
                                    return true;
                                }
                                return (val.indexOf("[this]") == -1);
                            },
                            message: 'You cannot use self-reference in conditions'
                        }]
                    });
                };
                
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
                index = function () {
                    if (this.hasParent()) {
                        var parent = this.parent();
                        var item = utils.findById(parent.childrenID(), this.id());
                        return item.index;
                    }
                    return 0;
                },
                hasParent = function () {
                    if (_.isNull(this.parent()) || _.isUndefined(this.parent())) {
                        return false;
                    }
                    return true;
                },
                fillChildren = function () {
                     var items =_.map(this.childrenID(), function (item) {
                        if (item.type === "GroupView")
                            return dc().groups.getLocalById(item.id);
                        return dc().questions.getLocalById(item.id);
                     });
                     this.children(items);
                     this.children.id = this.id();
                    //return self.children();
                },
                clone = function () {
                    var item = new Group();
                    item.title(this.title());
                    item.type(this.type());
                    item.condition(this.condition());
                    item.level(this.level());
                    item.description(this.description());
                    item.parent(this.parent());
                    item.id(Math.uuid());
                    item.isNew(true);
                    item.isClone(true);
                    item.isRoster(this.isRoster());
                    item.rosterSizeQuestion(this.rosterSizeQuestion());

                    item.childrenID(_.map(this.childrenID(), function (child) {
                        var clonedItem;
                        
                        if (child.type === "GroupView") {
                            clonedItem = dc().groups.getLocalById(child.id).clone();
                            dc().groups.add(clonedItem);
                        } else {
                            clonedItem = dc().questions.getLocalById(child.id).clone();
                            dc().questions.add(clonedItem);
                        }
                        clonedItem.parent(item);
                        return { type: clonedItem.type(), id: clonedItem.id() };
                    }));

                    if (this.isClone() && this.isNew()) {
                        item.cloneSource(this.cloneSource());
                    } else {
                        item.cloneSource(this);
                    }

                    item.dirtyFlag().reset();
                    item.fillChildren();
                    return item;
                };;
            return {
                isNullo: false,
                fillChildren: fillChildren,
                index: index,
                hasParent: hasParent,
                clone: clone
            };
        };


        Group.prototype = new BaseGroup();

        Group.Nullo = new Group().id(0).title('Title').type('GroupView');
        Group.Nullo.isNullo = true;
        Group.Nullo.dirtyFlag().reset();

        ko.utils.extend(Group.prototype, {
            update: function (data) {
                
                this.title(data.title);
                this.description(data.description);
                this.condition(data.condition);
                this.isRoster(data.isRoster);
                this.rosterSizeQuestion(data.rosterSizeQuestion);

                //save off the latest data for later use
                this.cache.latestData = data;
            },
            revert: function () {
                this.update(this.cache.latestData);
            },
            commit: function () {
                this.cache.latestData = ko.toJS(this);
            }
        });

        return Group;
    });

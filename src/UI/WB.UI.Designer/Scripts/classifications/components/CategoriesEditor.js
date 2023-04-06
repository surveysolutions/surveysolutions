app.component('CategoriesEditor',
    {
        template: '#categories-editor-template',
        data: function() {
            return {
                optionsMode: true,
                regex: Vue.$config.optionsParseRegex,
                stringifiedOptions: ''
            }
        },
        computed: {
            categories() {
                return this.$store.state.categories;
            },
            isEditMode() {
                return Vue.$config.isAdmin || Vue.$config.userId === this.$store.state.activeClassification.userId;
            },
            activeGroup() {
                return this.$store.state.activeGroup;
            },
            activeClassification() {
                return this.$store.state.activeClassification;
            },
            isFormDirty() {
                return Object.keys(this.fields).some(key => this.fields[key].dirty);
            },
            validator() {
                return this.$validator;
            }
        },
        watch: {
            activeClassification: function(val) {
                this.optionsMode = true;
            }
        },
        methods: {
            validateOptionAsText(option) {
                return this.regex.test((option || ""));
            },

            stringifyCategories() {
                var stringifiedOptions = "";
                var maxLength = _.max(_.map(this.categories, function (o) { return o.title.length; })) + 3;
                _.each(this.categories, function (category) {
                    if (!_.isEmpty(category)) {
                        stringifiedOptions += (category.title || "") + Array(maxLength + 1 - (category.title || "").length).join('.') + (category.value === 0 ? "0" : (category.value || ""));
                        stringifiedOptions += "\n";
                    }
                });
                this.stringifiedOptions = stringifiedOptions.trim("\n");
            },

            parseOptions() {
                var self = this;
                var optionsStringList = (this.stringifiedOptions || "").split("\n");
                optionsStringList = _.filter(optionsStringList, function (line) { return !_.isEmpty(line); });

                var options = _.map(optionsStringList, function(item) {
                    var matches = item.match(self.regex);
                    return {
                        value: matches[2] * 1,
                        title: matches[1]
                    };
                });

                return options;
            },
            moveFocus($event) {
                var parentCell = $($event.target).closest('div.option-cell');
                var nextCell = parentCell.next('div.option-cell');

                if (nextCell.length != 0) {
                    nextCell.find('input').focus();
                } else {
                    $($event.target).closest('div.option-line').next('div.option-line').find('input').first().focus();
                }
            },
            deleteCategory(index) {
                this.$store.dispatch('deleteCategory', index);
                this.$validator.flag('collectionSizeTracker', { dirty: true });
            },
            showStrings() {
                this.stringifyCategories();
                this.optionsMode = false;
            },
            showList() {
                var self = this;
                this.$validator.validate().then(function(result) {
                    if (self.$validator.errors.has('stringifiedOptions')) {

                    } else {
                        var parsedCategories = self.parseOptions();

                        var commonLength = Math.min(self.categories.length, parsedCategories.length);
                        for (var i = 0; i < commonLength; i++) {
                            self.$store.dispatch('updateCategory',
                                {
                                    index: i,
                                    title: parsedCategories[i].title,
                                    value: parsedCategories[i].value
                                });
                        }

                        if (self.categories.length < parsedCategories.length) {
                            // need to add
                            for (var i = commonLength; i < parsedCategories.length; i++) {
                                self.$store.dispatch('addCategory',
                                    {
                                        id: guid(),
                                        isNew: true,
                                        title: parsedCategories[i].title,
                                        value: parsedCategories[i].value,
                                        parent: self.$store.state.activeClassification.id
                                    });
                            }
                        } else if (self.categories.length > parsedCategories.length) {
                            //  need to remove
                            for (var i = commonLength - 1; i < self.categories.length; i++) {
                                self.$store.dispatch('deleteCategory', i);
                            }
                        }

                        self.optionsMode = true;
                    }
                });
            },
            save() {
                if (!this.optionsMode) {
                    this.showList();
                }

                var self = this;
                this.$validator.validate().then(function(result) {
                    if (result) {
                        self.$store.dispatch('updateCategories', self.$store.state.activeClassification.id).then(function() {
                            self.$validator.pause();
                            self.$nextTick(() => {
                                self.$validator.fields.items.forEach(field => field.reset());
                                self.$validator.reset();
                                self.$validator.errors.clear();
                                self.$validator.resume();
                            });
                        });
                    }
                });
            },
            addCategory() {
                this.$store.dispatch('addCategory',
                    {
                        id: guid(),
                        isNew: true,
                        title: '',
                        value: null,
                        parent: this.$store.state.activeClassification.id
                    });
                this.$validator.flag('collectionSizeTracker', { dirty: true });
            }
        }
    });

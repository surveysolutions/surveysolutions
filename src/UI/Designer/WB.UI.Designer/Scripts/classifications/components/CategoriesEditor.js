Vue.component('CategoriesEditor',
    {
        template: '#categories-editor-template',
        data: function() {
            return {
                optionsMode: true,
                regex: new RegExp(/^(.+?)[\…\.\s]+([-+]?\d+)\s*$/),
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
            },
            showStrings() {
                this.stringifyCategories();
                this.optionsMode = false;
            },
            showList() {
                var parsedCategories = this.parseOptions();

                var commonLength = Math.min(this.categories.length, parsedCategories.length);
                for (var i = 0; i < commonLength; i++) {
                    this.$store.dispatch('updateCategory',
                        {
                            index: i,
                            title: parsedCategories[i].title,
                            value: parsedCategories[i].value
                        });
                }

                if (this.categories.length < parsedCategories.length) {
                    // need to add
                    for (var i = commonLength; i < parsedCategories.length; i++) {
                        this.$store.dispatch('addCategory',
                            {
                                id: guid(),
                                isNew: true,
                                title: parsedCategories[i].title,
                                value: parsedCategories[i].value,
                                parent: this.$store.state.activeClassification.id
                            });
                    }
                } else if (this.categories.length > parsedCategories.length) {
                    //  need to remove
                    for (var i = commonLength - 1; i < this.categories.length; i++) {
                        this.$store.dispatch('deleteCategory', i);
                    }
                }

                this.optionsMode = true;
            },
            save() {
                var self = this;
                this.$validator.validate().then(function(result) {
                    if (result) {
                        store.dispatch('updateCategories', self.$store.state.activeClassification.id).then(
                            function() {

                            });
                    }
                });
            },
            addCategory() {
                store.dispatch('addCategory',
                    {
                        id: guid(),
                        isNew: true,
                        title: '',
                        value: null,
                        parent: this.$store.state.activeClassification.id
                    });
            }
        }
    });

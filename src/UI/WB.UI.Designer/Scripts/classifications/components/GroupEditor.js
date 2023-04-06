app.component('GroupEditor',
    {
        template: '#group-editor-template',
        data: function() {
            return {
                isEditMode: this.group.isNew,
                title: this.group.title
            };
        },
        props: ['group', 'index'],
        computed: {
            isFormDirty() {
                return Object.keys(this.fields).some(key => this.fields[key].dirty);
            }
        },
        methods: {
            cancel() {
                if (this.group.isNew) {
                    store.dispatch('deleteGroup', this.index);
                } else {
                    this.title = this.group.title;
                    this.isEditMode = false;
                }
            },
            edit() {
                this.isEditMode = true;
            },
            select() {
                store.dispatch('selectGroup', this.index);
            },
            deleteItem() {
                if (confirm(`Are you sure you want to delete classification group '${this.title}'?`)) {
                    store.dispatch('deleteGroup', this.index);
                }
            },
            save() {
                var self = this;
                var group = {
                    isNew: this.group.isNew,
                    id: this.group.id,
                    title: this.title,
                    index: this.index
                };

                this.$validator.validate().then(function(result) {
                    if (result) {
                        store.dispatch('updateGroup', group).then(function() {
                            self.isEditMode = false;
                        });
                    }
                });
            }
        },
        mounted: function() {
            var self = this;

            if (!Vue.$config.isAdmin)
                return;

            this.$nextTick(function() {
                $(this.$el).contextMenu({
                    selector: 'a',
                    callback: function(key, options) {
                        self[key]();
                    },
                    items: {
                        "edit": { name: "Edit", icon: "edit" },
                        "sep1": "---------",
                        "deleteItem": { name: "Delete", icon: "delete" }
                    }
                });
            });
        }
    });

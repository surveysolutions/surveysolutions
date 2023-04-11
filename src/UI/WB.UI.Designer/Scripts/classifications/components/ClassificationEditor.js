export default defineComponent({

    template: '#classification-editor-template',
    data: function() {
        return {
            isEditMode: this.classification.isNew,
            title: this.classification.title
        };
    },
    props: ['classification', 'index'],
    computed: {
        isFormDirty() {
            return Object.keys(this.fields).some(key => this.fields[key].dirty);
        },
        isPrivate() {
            return Vue.$config.userId === this.classification.userId;
        }
    },
    methods: {
        cancel() {
            if (this.classification.isNew) {
                store.dispatch('deleteClassification', this.index);
            } else {
                this.title = this.classification.title;
                this.isEditMode = false;
            }
        },
        edit() {
            this.isEditMode = true;
        },
        select() {
            store.dispatch('selectClassification', this.index);
        },
        deleteItem() {
            if (confirm(`Are you sure you want to delete classification '${this.title}'?`)) {
                store.dispatch('deleteClassification', this.index);
            }
        },
        save() {
            var self = this;
            var classification = {
                isNew: this.classification.isNew,
                id: this.classification.id,
                title: this.title,
                index: this.index,
                parent: this.classification.parent
            };

            this.$validator.validate().then(function(result) {
                if (result) {
                    store.dispatch('updateClassification', classification)
                        .then(function() {
                            self.isEditMode = false;
                        });
                }
            });

        }
    },
    mounted: function() {
        var self = this;

        if (!Vue.$config.isAdmin && (Vue.$config.userId !== this.classification.userId))
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

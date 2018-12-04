Vue.component('CategoriesEditor',
    {
        template: '#categories-editor-template',
        data: function() {
            return {
            }
        },
        computed: {
            categories() {
                return this.$store.state.categories;
            },
            isEditMode() {
                return Vue.$config.isAdmin || Vue.$config.userId === this.$store.state.activeClassification.userId;
            }
        },
        methods: {
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
                store.dispatch('deleteCategory', index);
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
            cancel() {
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

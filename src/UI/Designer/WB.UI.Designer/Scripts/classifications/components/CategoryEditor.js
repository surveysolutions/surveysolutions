Vue.component('CategoryEditor',
    {
        template: '#category-editor-template',
        data: function() {
            return {
                isNew: this.category.isNew,
                title: this.category.title,
                value: this.category.value,
                parent: this.category.parent,
                id: this.category.id
            }
        },
        props: ['category', 'index'],
        computed: {
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
            deleteCategory() {
                store.dispatch('deleteCategory', this.index);
            }
        }
    });

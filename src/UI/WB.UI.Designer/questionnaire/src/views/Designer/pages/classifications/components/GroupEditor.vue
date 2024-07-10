<template>
    <div>
        <div v-if="isEditMode" class="edit-classification-group-name">
            <form>
                <div class="form-group" :class="{ 'has-error': errors.has('title') }">
                    <textarea v-elastic name="title" type="text" v-validate="'required'" required v-model="title"
                        class="form-control"
                        :placeholder="$t('QuestionnaireEditor.ClassificationGroupTitle')"></textarea>
                    <span class="help-block" v-show="errors.has('title')">{{ errors.first('title') }}</span>
                </div>
                <button type="button" :disabled="!isFormDirty" @@click="save" class="btn btn-success">{{
            $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" @click="cancel()" class="btn btn-link">{{ $t('QuestionnaireEditor.Cancel')
                    }}</button>

            </form>
        </div>
        <div v-else class="line-wrapper">
            <a @click="select()">{{ title }} <span class="badge pull-right">{{ group.count }}</span></a>
        </div>
    </div>
</template>
<script>

export default {
    name: 'GroupEditor',
    data: function () {
        return {
            isEditMode: this.group.isNew,
            title: this.group.title
        };
    },
    props: ['group', 'index'],
    mounted: function () {
        var self = this;

        if (!this.$store.state.isAdmin) return;

        this.$nextTick(function () {
            $(this.$el).contextMenu({
                selector: 'a',
                callback: function (key, options) {
                    self[key]();
                },
                items: {
                    edit: { name: 'Edit', icon: 'edit' },
                    sep1: '---------',
                    deleteItem: { name: 'Delete', icon: 'delete' }
                }
            });
        });
    },
    computed: {
        isFormDirty() {
            return Object.keys(this.fields).some(key => this.fields[key].dirty);
        }
    },
    methods: {
        cancel() {
            if (this.group.isNew) {
                this.$store.dispatch('deleteGroup', this.index);
            } else {
                this.title = this.group.title;
                this.isEditMode = false;
            }
        },
        edit() {
            this.isEditMode = true;
        },
        select() {
            this.$store.dispatch('selectGroup', this.index);
        },
        deleteItem() {
            if (
                confirm(
                    `Are you sure you want to delete classification group '${this.title}'?`
                )
            ) {
                this.$store.dispatch('deleteGroup', this.index);
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

            this.$validator.validate().then(function (result) {
                if (result) {
                    this.$store.dispatch('updateGroup', group).then(function () {
                        self.isEditMode = false;
                    });
                }
            });
        }
    },
}
</script>

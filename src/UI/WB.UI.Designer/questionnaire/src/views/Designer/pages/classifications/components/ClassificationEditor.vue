<template>
    <div>
        <div v-if="isEditMode" class="edit-classification-group-name">
            <form>
                <div class="form-group">
                    <textarea v-elastic name="title" type="text" v-validate="'required'" required v-model="title"
                        class="form-control" :placeholder="$t('QuestionnaireEditor.ClassificationTitle')"></textarea>
                </div>
                <button type="button" :disabled="!isFormDirty" @click="save" class="btn btn-success">{{
            $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" @click="cancel()" class="btn btn-link">{{ $t('QuestionnaireEditor.Cancel')
                    }}</button>
            </form>
        </div>
        <div v-else class="line-wrapper" :class="{ 'private': isPrivate }">
            <a @click="select()">{{ title }} <span class="badge pull-right">{{ classification.count }}</span></a>
        </div>
    </div>
</template>
<script>

export default {
    name: 'ClassificationEditor',
    data: function () {
        return {
            isEditMode: this.classification.isNew,
            title: this.classification.title
        };
    },
    props: ['classification', 'index'],
    mounted: function () {
        var self = this;

        if (
            !Vue.$config.isAdmin &&
            Vue.$config.userId !== this.classification.userId
        )
            return;

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
            if (
                confirm(
                    `Are you sure you want to delete classification '${this.title}'?`
                )
            ) {
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

            this.$validator.validate().then(function (result) {
                if (result) {
                    store
                        .dispatch('updateClassification', classification)
                        .then(function () {
                            self.isEditMode = false;
                        });
                }
            });
        }
    },

}
</script>

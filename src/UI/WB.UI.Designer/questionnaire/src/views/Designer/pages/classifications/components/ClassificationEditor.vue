<template>
    <div>
        <div v-if="isEditMode" class="edit-classification-group-name">
            <vee-form v-slot="{ meta }">
                <div class="form-group">
                    <textarea v-elastic name="title" type="text" v-validate="'required'" required v-model="title"
                        class="form-control" :placeholder="$t('QuestionnaireEditor.ClassificationTitle')"></textarea>
                </div>
                <button type="button" :disabled="!meta.dirty" @click="save" class="btn btn-success">{{
            $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" @click="cancel()" class="btn btn-link">{{ $t('QuestionnaireEditor.Cancel')
                    }}</button>
            </vee-form>
        </div>
        <div v-else class="line-wrapper" :class="{ 'private': isPrivate }">
            <a @click="select()">{{ title }} <span class="badge pull-right">{{ classification.count }}</span></a>
        </div>
    </div>
</template>
<script>

import { Form, Field, ErrorMessage } from 'vee-validate';

export default {
    name: 'ClassificationEditor',
    components: {
        VeeForm: Form,
        VField: Field,
        ErrorMessage: ErrorMessage,
    },
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
            !this.$store.state.isAdmin &&
            this.$store.state.userId !== this.classification.userId
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
        isPrivate() {
            return this.$store.state.userId === this.classification.userId;
        }
    },
    methods: {
        cancel() {
            if (this.classification.isNew) {
                this.$store.dispatch('deleteClassification', this.index);
            } else {
                this.title = this.classification.title;
                this.isEditMode = false;
            }
        },
        edit() {
            this.isEditMode = true;
        },
        select() {
            this.$store.dispatch('selectClassification', this.index);
        },
        deleteItem() {
            if (
                confirm(
                    `Are you sure you want to delete classification '${this.title}'?`
                )
            ) {
                this.$store.dispatch('deleteClassification', this.index);
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

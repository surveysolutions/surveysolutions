<template>
    <div v-contextmenu="'classification-context-menu-' + index" menu-item-class="context-menu-item">
        <div v-if="isEditMode" class="edit-classification-group-name">
            <vee-form ref="form" v-slot="{ errors, meta }">
                <div class="form-group" :class="{ 'has-error': errors.title }">
                    <vee-field as="textarea" v-autosize name="title" type="text" required v-model="title"
                        rules="required" class="form-control" :validateOnInput="true"
                        :placeholder="$t('QuestionnaireEditor.ClassificationTitle')" />
                    <span class="help-block" v-show="errors.title">{{ errors.title }}</span>
                </div>
                <button type="button" :disabled="!meta.dirty ? 'disabled' : null" @click="save"
                    class="btn btn-success">{{
        $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" @click="cancel()" class="btn btn-link">{{ $t('QuestionnaireEditor.Cancel')
                    }}</button>
            </vee-form>
        </div>
        <div v-else class="line-wrapper" :class="{ 'private': isPrivate }">
            <a @click="select()">{{ title }} <span class="badge pull-right">{{ classification.count }}</span></a>
        </div>
    </div>
    <Teleport to="body">
        <ul v-if="isContextMenuSupport" class="context-menu-list context-menu-root" style="z-index: 2;"
            :id="'classification-context-menu-' + index">
            <li class="context-menu-item context-menu-icon context-menu-icon-edit context-menu-visible" @click="edit()">
                <span>Edit</span>
            </li>
            <li class="context-menu-item context-menu-separator context-menu-not-selectable"></li>
            <li class="context-menu-item context-menu-icon context-menu-icon-delete" @click="deleteItem()">
                <span>Delete</span>
            </li>
        </ul>
    </Teleport>
</template>
<script>

import { Form, Field, ErrorMessage } from 'vee-validate';

export default {
    name: 'ClassificationEditor',
    components: {
        VeeForm: Form,
        VeeField: Field,
        ErrorMessage: ErrorMessage,
    },
    data: function () {
        return {
            isEditMode: this.classification.isNew,
            title: this.classification.title,
        };
    },
    props: ['classification', 'index'],
    mounted: function () {

    },
    computed: {
        isPrivate() {
            return this.$store.state.userId === this.classification.userId;
        },
        isContextMenuSupport() {
            return !(
                !this.$store.state.isAdmin &&
                this.$store.state.userId !== this.classification.userId
            )
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

            this.$refs.form.validate().then(result => {
                if (result.valid) {
                    self.$store
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

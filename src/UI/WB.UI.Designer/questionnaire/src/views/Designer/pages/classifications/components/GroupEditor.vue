<template>
    <div v-contextmenu="'group-context-menu-' + index" menu-item-class="context-menu-item">
        <div v-if="isEditMode" class="edit-classification-group-name">
            <vee-form ref="form" v-slot="{ errors, meta }">
                <div class="form-group" :class="{ 'has-error': errors.title }">
                    <vee-field as="textarea" v-autosize name="title" type="text" rules="required" required
                        v-model="title" class="form-control" v-validate="'required'" :validateOnChange="true"
                        :placeholder="$t('QuestionnaireEditor.ClassificationGroupTitle')" />
                    <span class="help-block" v-show="errors.title">{{ errors.title }}</span>
                </div>
                <button type="button" :disabled="!meta.dirty ? 'disabled' : null" @click="save"
                    class="btn btn-success">{{ $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" @click="cancel()" class="btn btn-link">{{ $t('QuestionnaireEditor.Cancel')
                    }}</button>
            </vee-form>
        </div>
        <div v-else class="line-wrapper">
            <a @click="select()">{{ title }} <span class="badge pull-right">{{ group.count }}</span></a>
        </div>
    </div>
    <Teleport to="body">
        <ul v-if="isContextMenuSupport" class="context-menu-list context-menu-root" :id="'group-context-menu-' + index"
            style="z-index: 2;">
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
    name: 'GroupEditor',
    components: {
        VeeForm: Form,
        VeeField: Field,
        ErrorMessage: ErrorMessage,
    },
    data: function () {
        return {
            isEditMode: this.group.isNew,
            title: this.group.title
        };
    },
    props: ['group', 'index'],
    mounted: function () {

    },
    computed: {
        isContextMenuSupport() {
            return this.$store.state.isAdmin
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

            this.$refs.form.validate().then(function (result) {
                if (result) {
                    self.$store.dispatch('updateGroup', group).then(function () {
                        self.isEditMode = false;
                    });
                }
            });
        }
    },
}
</script>

<template>
    <div class="slider" v-if="isLoading">
        <div class="line"></div>
        <div class="subline inc"></div>
        <div class="subline dec"></div>
    </div>
    <div class="classifications-wrapper">
        <div class="classifications-block">
            <div class="col-xs-3 column classification-groups">
                <div class="classification-list-wrapper">
                    <perfect-scrollbar class="scroller">
                        <ul class="breadcrumb">
                            <li class="active">{{ $t('QuestionnaireEditor.ClassificationGroupBreadcrumbs') }}</li>
                        </ul>
                        <ul class="list-unstyled classification-list">
                            <li v-for="(group, index) in groups" :key="group.id" :class="{ 'active': group.isActive }">
                                <group-editor :group="group" :index="index"></group-editor>
                            </li>
                        </ul>
                        <button type="button" class="btn lighter-hover" @click="addGroup()" v-if="isAdmin">{{
                            $t('QuestionnaireEditor.ClassificationAddGroup') }}</button>
                    </perfect-scrollbar>
                </div>
            </div>
            <div class="col-xs-4 column classifications">
                <div class="classification-list-wrapper">
                    <perfect-scrollbar class="scroller" ref="groupScroller">
                        <ul class="breadcrumb">
                            <li class="active">{{ activeGroup.title }}</li>
                        </ul>
                        <ul class="list-unstyled classification-list">
                            <li v-for="(classification, index) in classifications" :key="classification.id"
                                :class="{ 'active': classification.isActive }">
                                <classification-editor :classification="classification"
                                    :index="index"></classification-editor>
                            </li>
                        </ul>
                        <button v-if="activeGroup.id" type="button" class="btn lighter-hover"
                            @click="addClassification()">{{ $t('QuestionnaireEditor.ClassificationAdd') }}</button>
                    </perfect-scrollbar>
                </div>
            </div>
            <div class="col-xs-5 column categories-groups">
                <categories-editor></categories-editor>
            </div>
        </div>
    </div>

    <!--Teleport to="body">
        <CategoriesEditor></CategoriesEditor>
        <ClassificationEditor></ClassificationEditor>
        <GroupEditor></GroupEditor>
    </Teleport-->
</template>

<script>

import { newGuid } from '../../../helpers/guid'

import CategoriesEditor from './classifications/components/CategoriesEditor.vue'
import ClassificationEditor from './classifications/components/ClassificationEditor.vue'
import GroupEditor from './classifications/components/GroupEditor.vue'

import '../../../../../Content/classifications.css';
import '../../../../../Content/plugins/jquery.contextMenu.min.css';


export default {
    name: 'Classifications',
    components: { CategoriesEditor, ClassificationEditor, GroupEditor },
    data() {
        return {

        }
    },
    created() {
        this.$store.dispatch('getUserInfo');
        this.$store.dispatch('loadGroups');
    },
    watch: {
        activeGroup: function (val) {
            this.$nextTick(() => {
                this.$refs.groupScroller.$el.scrollTop = 0
            });
        }
    },
    computed: {
        isAdmin() {
            return this.$store.state.isAdmin;
        },
        isLoading() {
            return this.$store.state.isLoading;
        },
        groups() {
            return this.$store.state.groups;
        },
        classifications() {
            return this.$store.state.classifications;
        },
        categories() {
            return this.$store.state.categories;
        },
        activeGroup() {
            return this.$store.state.activeGroup;
        },
        activeClassification() {
            return this.$store.state.activeClassification;
        }
    },
    methods: {
        addGroup() {
            this.$store.dispatch('addGroup', { id: newGuid(), isNew: true, title: '', count: 0 });
        },
        addClassification() {
            this.$store.dispatch('addClassification',
                { id: newGuid(), isNew: true, title: '', parent: this.activeGroup.id, userId: this.$store.state.userId, count: 0 });
        }
    }
};
</script>

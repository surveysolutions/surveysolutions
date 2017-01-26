<template>
    <div class="panel panel-default">
        <div class="panel-heading" role="tab">
            <h3 class="panel-title" :class="titleCss">
                <button class="btn btn-link btn-plus" v-if="hasChild" :class="{collapsed: collapsed}" type="button" @click="toggle"><span></span></button>
                <router-link :to="id">{{title}}</router-link>
            </h3>
        </div>
        <div class="panel-collapse collapse" :class="{in: !collapsed}" role="tabpanel" v-if="hasChild">
            <div class="panel-body">
                <div class="panel-group" role="tablist">
                    <sidebar-panel v-for="panel in panels"
                        :id="panel.id"
                        :title="panel.title"
                        :state="panel.state"
                        :childPanels="panel.panels"
                        :collapsed="panel.collapsed"></sidebar-panel>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang='ts'>
    import { GroupStatus } from "components/entities"

    export default {
        name: 'sidebar-panel',
        props: {
            id: { required: true, type: String },
            title: { required: true, type: String },
            panels: {},
            collapsed: { type: Boolean, default: true },
            state: { type: Number, default: GroupStatus.Other }
        },
        computed: {
            hasChild() {
                return this.panels != null && this.panels.length > 0
            },
            titleCss() {
                return [{
                    group: !this.hasChild,
                    'has-error': this.state == GroupStatus.Invalid,
                    'complete': this.state == GroupStatus.Completed,
                }]
            }
        },
        methods: {
            toggle() {
                this.collapsed = !this.collapsed
            }
        }
    }
</script>

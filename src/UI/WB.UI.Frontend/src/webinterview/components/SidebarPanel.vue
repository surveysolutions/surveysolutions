<template>
    <div class="panel panel-default"
        :data-id="this.panel.id">
        <div class="panel-heading"
            role="tab">
            <h3 class="panel-title"
                :class="titleCss"
                :disabled="isDisabled">
                <button class="btn btn-link btn-plus"
                    v-if="hasChild"
                    :class="{collapsed: isCollapsed}"
                    :disabled="isDisabled"
                    type="button"
                    @click="toggle">
                    <span></span>
                </button>
                <router-link :to="to"
                    :disabled="isDisabled"
                    :class="{'disabled':isDisabled}"
                    v-if="this.panel">
                    <span v-html="title"/>
                </router-link>
            </h3>
        </div>
        <div class="panel-collapse collapse"
            :class="{in: !isCollapsed}"
            role="tabpanel"
            v-if="hasChild && !isCollapsed">
            <div class="panel-body">
                <div class="panel-group"
                    role="tablist">
                    <sidebar-panel v-for="childPanel in childPanels"
                        :key="childPanel.id"
                        :panel="childPanel"
                        :currentPanel="currentPanel">
                    </sidebar-panel>
                    <sidebar-panel :panel="loading"
                        :currentPanel="currentPanel"
                        v-if="childPanels.length === 0"></sidebar-panel>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="js">
import { GroupStatus } from './questions'

export default {
    name: 'sidebar-panel',
    props: {
        panel: { required: true },
        currentPanel: {},
    },
    computed: {
        loading() {
            return {
                collapsed: true,
                title: '...',
                id: this.panel.id,
                validity: {
                    isValid: true,
                },
            }
        },
        title() {
            if (this.panel.hasCustomRosterTitle || !this.panel.isRoster) {
                return this.panel.title
            }

            const rosterTitle = this.panel.rosterTitle || '[...]'
            return `${this.panel.title} - ${rosterTitle}`
        },
        to() {
            if (this.panel.to) {
                return this.panel.to
            }

            var coverPageId = this.$config.coverPageId != undefined ? this.$config.coverPageId : this.$config.model.coverPageId
            if (coverPageId && this.panel.id == coverPageId) {
                return { name: 'cover', params: { sectionId: this.panel.id } }
            }

            return { name: 'section', params: { sectionId: this.panel.id } }
        },
        isCollapsed() {
            return this.panel.collapsed
        },
        isDisabled() {
            return this.panel.isDisabled
        },
        hasChild() {
            return this.panel.hasChildren
        },
        childPanels() {
            return this.$store.state.webinterview.sidebar.panels[this.panel.id] || []
        },
        isActive() {
            if (this.panel.to) {
                return this.$route.name === this.panel.to.name
            }

            return this.currentPanel == this.panel.id
        },
        titleCss() {
            return [{
                disabled:this.panel.isDisabled,
                current: this.panel.current,
                active: this.isActive,
                complete: this.panel.status === GroupStatus.Completed && !this.hasError ,
                'has-error': this.hasError,
            }]
        },
        hasError() {
            return this.panel.validity.isValid == false
        },

    },
    watch: {
        ['$route.params.sectionId']() {
            this.update()

        },
        'panel'() {
            this.update()
        },
    },
    mounted() {
        this.update()
        if(this.isActive)
            this.$el.scrollIntoView({ behavior: 'smooth' })
    },
    methods: {
        fetchChild() {
            this.$store.dispatch('fetchSidebar', this.panel.id)
        },
        update() {
            if (this.panel.hasChildren && !this.panel.collapsed) {
                this.fetchChild()
            }
        },
        toggle() {
            this.$store.dispatch('toggleSidebar', {
                panel: this.panel,
                collapsed: !this.panel.collapsed,
            })
        },
    },
}

</script>

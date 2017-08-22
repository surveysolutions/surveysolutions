<template>
    <div class="panel panel-default" :data-id="this.panel.id">
        <div class="panel-heading" role="tab">
            <h3 class="panel-title" :class="titleCss">
                <button class="btn btn-link btn-plus" v-if="hasChild" :class="{collapsed: isCollapsed}" type="button" @click="toggle"><span></span></button>
                <router-link :to="to" v-if="this.panel" v-html="title"></router-link>
            </h3>
        </div>
        <div class="panel-collapse collapse" :class="{in: !isCollapsed}" role="tabpanel" v-if="hasChild && !isCollapsed">
            <div class="panel-body">
                <div class="panel-group" role="tablist">
                    <sidebar-panel v-for="childPanel in childPanels" :key="childPanel.id" :panel="childPanel" :currentPanel="currentPanel">
                    </sidebar-panel>
                    <sidebar-panel :panel="loading" :currentPanel="currentPanel" v-if="childPanels.length === 0"></sidebar-panel>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="js">
    export default {
        name: 'sidebar-panel',
        props: {
            panel: { required: true },
            currentPanel: {}
        },
        computed: {
            loading() {
                return {
                    collapsed: true,
                    title: "...",
                    id: this.panel.id,
                    validity: {
                        isValid: true
                    }
                }
            },
            title() {
                return decodeURIComponent(this.panel.title + (this.panel.isRoster ? (this.panel.rosterTitle ? ` - ${this.panel.rosterTitle}` :" - [...]") : ""))
            },
            to() {
                if (this.panel.to) {
                    return this.panel.to
                }

                return { name: 'section', params: { sectionId: this.panel.id } }
            },
            isCollapsed() {
                return this.panel.collapsed
            },
            hasChild() {
                return this.panel.hasChildren
            },
            childPanels() {
                return this.$store.state.sidebar.panels[this.panel.id] || []
            },
            isActive() {
                if (this.panel.to) {
                    return this.$route.name === this.panel.to.name
                }

                return this.currentPanel == this.panel.id
            },
            titleCss() {
                return [{
                    current: this.panel.current,
                    active: this.isActive,
                    complete: this.panel.state === "Completed" && this.panel.validity.isValid,
                    "has-error": !this.panel.validity.isValid
                }]
            }
        },
        watch: {
            $route(to, from) {
                this.update()
            },
            "panel"(to, from) {
                this.update()
            }
        },
        mounted() {
            this.update()
        },
        methods: {
            fetchChild() {
                this.$store.dispatch("fetchSidebar", this.panel.id)
            },
            update() {
                if (this.panel.hasChildren && !this.panel.collapsed) {
                    this.fetchChild()
                }
            },
            toggle() {
                this.$store.dispatch("toggleSidebar", {
                    panel: this.panel,
                    collapsed: !this.panel.collapsed
                })
            }
        }
    }

</script>

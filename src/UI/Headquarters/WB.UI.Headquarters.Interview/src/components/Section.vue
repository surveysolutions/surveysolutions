<template>
    <div v-if="section">
        <div class="unit-section" :class="[{'complete-section': section.status == 1, 'section-with-error': section.status == -1}]" >
            <div class="unit-title is_stuck" >
                <ol class="breadcrumb">
                    <li><a href="">Breadcrumbs</a></li>
                </ol>
                <h3>{{section.title}}</h3>
        </div>
        <component v-for="entity in section.entities" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
        <!--<h1>
            You are browsing section component
        </h1>-->
        </div>
    </div>
</template>  

<script lang="ts">

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        computed: {
            section() {
                return this.$store.state.section
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("loadSection", this.$route.params.sectionId)
            }
        }
        
    }
</script>

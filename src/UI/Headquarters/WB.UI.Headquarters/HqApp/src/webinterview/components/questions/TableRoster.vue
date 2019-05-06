<template>
    <wb-question :question="$me" :questionDivCssClassName="titleClass" :questionCssClassName="statusClass" noTitle="true" noValidation="true" noInstructions="true" noComments="true" noFlag="true">
        <span v-html="$me.title"></span>

        <link rel="stylesheet" href="https://unpkg.com/x-data-spreadsheet@1.0.13/dist/xspreadsheet.css">
        <script src="https://unpkg.com/x-data-spreadsheet@1.0.13/dist/xspreadsheet.js"></script>

        <script>
            x.spreadsheet('#xspreadsheet');
        </script>

        <div id="xspreadsheet"></div>
    </wb-question>
</template>

<script lang="js">
    import { entityDetails } from "../mixins"
    import { GroupStatus } from "./index"
    import { debounce, every, some } from "lodash"

    export default {
        name: 'TableRoster',
        mixins: [entityDetails],
        
        watch: {
            ["$store.getters.scrollState"]() {
                 this.scroll();
            }
        },

        mounted() {
            this.scroll();
        },

        computed: {
            navigateTo() {
                return {
                    name: 'section', params: {
                        sectionId: this.id,
                        interviewId: this.$route.params.interviewId
                    }
                }
            },
            isNotStarted() {
                return every(this.$me.instances, function(instance) { instance.status === GroupStatus.NotStarted});
            },
            isStarted() {
                return some(this.$me.instances, function(instance) { instance.status === GroupStatus.Started}); 
            },
            isCompleted() {
                return every(this.$me.instances, function(instance) { instance.status === GroupStatus.Completed}); 
            },
            titleClass() {
                return ['roster-title']
            },
            statusClass() {
                return ['roster-section-block', {
                    'started': this.$me.validity.isValid && this.isStarted,
                    'has-error': !this.$me.validity.isValid,
                    '': this.$me.validity.isValid && !this.isCompleted
                },
                {
                    'answered': this.isCompleted
                }]
            }           
        },
        methods : {
            doScroll: debounce(function() {
                if(this.$store.getters.scrollState ==  this.id){
                    window.scroll({ top: this.$el.offsetTop, behavior: "smooth" })
                    this.$store.dispatch("resetScroll")
                }
            }, 200),

            scroll() {
                if(this.$store && this.$store.state.route.hash === "#" + this.id) {
                    this.doScroll(); 
                }
            }
        }
    }
</script>

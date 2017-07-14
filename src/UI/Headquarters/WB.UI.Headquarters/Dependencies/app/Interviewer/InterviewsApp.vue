<template>

    <Layout :title="title" hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="title">
                <Typeahead data-vv-name="questionnaireId"
                                       data-vv-as="questionnaire"
                                       :placeholder="$t('Common.Any')"
                                       control-id="questionnaireId"
                                       :ajaxParams="{ statuses: statuses.toString() }"
                                       :value="questionnaireId"
                                       v-on:selected="questionnaireSelected"
                                       :fetch-url="$config.interviewerHqEndpoint + '/QuestionnairesCombobox'"></Typeahead>
            </FilterBlock>
        </Filters>
        <router-view
            :questionnaireId="questionnaireId"
        ></router-view>
    </Layout>

</template>

<script>

export default {
    data() {
        return {
            questionnaireId: null,
            title: "Change me"// this.$t('MainMenu.' + this.$router.props.status)
        }
    },

    computed: {
        statuses() {
            // magic. leaving as is for now
            return this.$route.matched[0].props.default.statuses
        }
    },

    methods: {
        questionnaireSelected(newValue) {
             this.questionnaireId = newValue;
        }
    }
}
</script>
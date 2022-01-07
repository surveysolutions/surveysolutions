<template>
    <div class="unit-section first-last-chapter"
        v-if="hasCompleteInfo"
        v-bind:class="{'section-with-error' : hasInvalidQuestions, 'complete-section' : isAllAnswered  }" >
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>{{ competeButtonTitle }}</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>{{ $t('WebInterviewUI.CompleteAbout') }}</h2>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">
                    {{ $t('WebInterviewUI.CompleteQuestionsStatus') }}
                </h4>
                <div class="question-status">
                    <ul class="list-inline clearfix">
                        <li class="answered"
                            v-bind:class="{'has-value' : hasAnsweredQuestions }">{{ answeredQuestionsCountString }}
                            <span>{{$t('WebInterviewUI.CompleteQuestionsAnswered')}}</span>
                        </li>
                        <li class="unanswered"
                            v-bind:class="{'has-value' : hasUnansweredQuestions }">{{ unansweredQuestionsCountString }}
                            <span>{{$t('WebInterviewUI.CompleteQuestionsUnanswered')}}</span>
                        </li>
                        <li class="errors"
                            v-bind:class="{'has-value' : hasInvalidQuestions }">{{ invalidQuestionsCountString }}
                            <span>{{$t('WebInterviewUI.Error', { count: completeInfo.errorsCount })}}</span>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="wrapper-info"
            v-if="completeInfo.entitiesWithError.length > 0">
            <div class="container-info">
                <h4 class="gray-uppercase">{{ doesShowErrorsCommentWithCount
                    ? $t('WebInterviewUI.CompleteFirstErrors', { count: completeInfo.entitiesWithError.length })
                    : $t('WebInterviewUI.CompleteErrors') }}</h4>
                <ul class="list-unstyled marked-questions">
                    <li v-for="entity in completeInfo.entitiesWithError"
                        :key="entity.id">
                        <a href="javascript:void(0);"
                            @click="navigateTo(entity)"
                            v-html="entity.title"></a>
                    </li>
                </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <label class="gray-uppercase"
                    for="comment-for-supervisor">
                    {{ noteToSupervisor }}
                </label>
                <div class="field">
                    <textarea class="field-to-fill"
                        id="comment-for-supervisor"
                        :placeholder="$t('WebInterviewUI.TextEnter')"
                        v-model="comment"></textarea>
                    <button type="submit"
                        class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>
            </div>
        </div>
        <div class="wrapper-info"
            v-if="mayBeSwitchedToWebMode">
            <div class="container-info">
                <input
                    v-if="mayBeSwitchedToWebMode"
                    class="wb-checkbox"
                    type="checkbox"
                    id="switchToWeb_id"
                    name="switchToWeb"
                    v-model="switchToWeb"/>
                <label for="switchToWeb_id"
                    class="font-bold"
                    v-if="mayBeSwitchedToWebMode">
                    <span class="tick"></span>
                    {{$t('WebInterviewUI.SwitchToWebMode')}}
                </label>
            </div>
            <div class="container-info action-block"
                v-if="switchToWeb">
                <p calss="gray-uppercase">
                    {{$t('WebInterviewUI.SwitchToWebMode_LinkDescription')}}
                </p>
                <p class="font-bold">
                    {{webLink}}
                </p>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <a href="javascript:void(0);"
                    id="btnComplete"
                    class="btn btn-lg"
                    v-bind:class="{
                        'btn-success': isAllAnswered,
                        'btn-primary' : hasUnansweredQuestions,
                        'btn-danger' : hasInvalidQuestions }"
                    @click="completeInterview">{{ competeButtonTitle }}</a>
            </div>
        </div>
    </div>
</template>

<script lang="js">
import modal from '@/shared/modal'
import Vue from 'vue'

export default {
    name: 'complete-view',
    beforeMount() {
        this.fetchCompleteInfo()
    },
    watch: {
        $route(to, from) {
            this.fetchCompleteInfo()
        },
        shouldCloseWindow(to){
            if(to === true) {
                this.completeInterview()
            }
        },
    },
    computed: {
        completeInfo() {
            return this.$store.state.webinterview.completeInfo
        },
        shouldCloseWindow() {
            return this.$store.state.webinterview.interviewCompleted && this.$config.inWebTesterMode
        },
        competeButtonTitle() {
            return this.$config.customTexts.completeButton
        },
        noteToSupervisor() {
            return this.$config.customTexts.noteToSupervisor
        },
        hasCompleteInfo() {
            return this.completeInfo != undefined
        },
        hasAnsweredQuestions() {
            return this.completeInfo.answeredCount > 0
        },
        isAllAnswered() {
            return this.completeInfo.unansweredCount == 0 && this.completeInfo.errorsCount == 0
        },
        answeredQuestionsCountString() {
            return this.hasAnsweredQuestions ? this.completeInfo.answeredCount : this.$t('WebInterviewUI.No')
        },
        hasUnansweredQuestions() {
            return this.completeInfo.unansweredCount > 0
        },
        unansweredQuestionsCountString() {
            return this.hasUnansweredQuestions ? this.completeInfo.unansweredCount : this.$t('WebInterviewUI.No')
        },
        hasInvalidQuestions() {
            return this.completeInfo.errorsCount > 0
        },
        invalidQuestionsCountString() {
            return this.hasInvalidQuestions ? this.completeInfo.errorsCount : this.$t('WebInterviewUI.No')
        },
        doesShowErrorsCommentWithCount() {
            return this.completeInfo.entitiesWithError.length < this.completeInfo.errorsCount
        },
        mayBeSwitchedToWebMode(){
            return this.$config.mayBeSwitchedToWebMode === true
        },
        webLink(){
            return this.$config.webInterviewUrl ?? ''
        },
    },
    data () {
        return {
            comment: '',
            switchToWeb: false,
        }
    },
    methods: {
        fetchCompleteInfo() {
            this.$store.dispatch('fetchCompleteInfo')
        },

        completeInterview() {
            if(this.shouldCloseWindow) {
                modal.dialog({
                    title: this.$t('WebInterviewUI.WebTesterSessionOver'),
                    message: '<p>' + this.$t('WebInterviewUI.WebTesterSessionOverMessage') +'</p>',
                    showCancelButton: false,
                    showConfirmButton: false,
                    onEscape: false,
                    closeButton: false,
                })

                return
            }
            if(this.switchToWeb)
                this.$store.dispatch('requestWebInterview', this.comment)
            else
                this.$store.dispatch('completeInterview', this.comment)
        },

        navigateTo(entityWithError) {
            if(entityWithError.isPrefilled){
                this.$router.push({ name: 'prefilled' })
                return
            }

            const navigateToEntity = {
                name: 'section',
                params: {
                    sectionId: entityWithError.parentId,
                    interviewId: this.$route.params.interviewId,
                },
                hash: '#' + entityWithError.id,
            }

            this.$router.push(navigateToEntity)
        },
    },
}

</script>

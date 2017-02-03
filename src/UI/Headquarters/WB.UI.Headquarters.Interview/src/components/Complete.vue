<template>
    <div class="unit-section complete-section">
        <div class="unit-title">
            <h3>Finish questionnaire</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>You are about to complete this questionnaire</h2>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">Questions status:</h4>
                <ul  class="list-unstyled value-is-absent">
                    <li>Interview not started,</li>
                    <li>no questions are answered,</li>
                    <li>no data collected</li>
                </ul>
                <div class="question-status">
                    <ul class="list-inline clearfix">
                        <li class="answered has-value">2632
                            <span>Answered</span>
                        </li>
                        <li class="unanswered">No
                            <span>Unanswered</span>
                        </li>
                        <li class="errors has-value">2
                            <span>Error(s)</span>
                        </li>
                    </ul>
                    <p class="gray-uppercase">Time spent: 04 hours 22 minutes</p>
                    <p class="gray-uppercase">Data size: 4312KB</p>
                </div>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
            <h4 class="gray-uppercase">Questions with errors:</h4>
            <ul class="list-unstyled marked-questions">
                <li><a href="#">How much quantity in total did your hoesehold consume in the past 7 days?</a></li>
                <li><a href="#">What is your relations status?</a></li>
            </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <p class="gray-uppercase">No notes for supervisor</p>
                <label class="gray-uppercase" for="comment-for-supervisor">Note for supervisor</label>
                <div class="field">
                    <textarea class="field-to-fill" id="comment-for-supervisor" placeholder="Tap to enter text"></textarea>
                    <button type="submit" class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>


            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">Supervisor's note:</h4>
                <p><b>Lorem ipsum dolor sit amet, pro ne dicta saepe albucius, habemus fabellas luptatum eam in. Eam eu delectus accusata theophrastus, quaeque gubergren assentior id mel, sed ad feugiat repudiandae mediocritatem. Saepe quaerendum te sit, no appellantur suscipiantur sea, clita noluisse in est. Persius denique appellantur nam eu. Epicurei invenire sit no, his aeterno expetenda no, vix eu erant lucilius.</b></p>
                <h4 class="gray-uppercase">Questions with supervisor's comments:</h4>
                <ul class="list-unstyled marked-questions">
                    <li><a href="#">How much quantity in total did your hoesehold consume in the past 7 days?</a></li>
                    <li><a href="#">What is your relations status?</a></li>
                </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <a href="#" class="btn btn-lg btn-success">Finish</a>
                <p class="gray-uppercase">After you finish this questionnaire it will become read only and will be uploaded to hq during your next synchronization</p>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <p class="gray-uppercase">this questionnaire is complete, it is in read-only mode. you can access chapters from top-right menu</p>
                <a href="#" class="btn btn-lg btn-success">view</a>
                <p class="gray-uppercase">if you need to change some data you need to reinitialize it</p>
                <a href="#" class="btn btn-lg btn-primary">reinitialize</a>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import * as Vue from 'vue'
    import * as debounce from "lodash/debounce"

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        watch: {
            $route(to, from) {
                this.loadSection()
            }
        },
        data: () => {
            return {
                // scrolls current section view when all fetch actions are done
                scroll: debounce(function () {
                    this.$store.dispatch("scroll")
                }, 300)
            }
        },
        computed: {

        },
        methods: {
            loadSection() {
                //this.$store.dispatch("fetchSectionEntities")
            }
        }
    }
</script>
